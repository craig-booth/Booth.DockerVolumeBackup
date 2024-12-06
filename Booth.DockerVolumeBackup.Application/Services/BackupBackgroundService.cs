using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MediatR;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using Booth.DockerVolumeBackup.Application.Backups.Queries;
using Booth.DockerVolumeBackup.Application.Backups.Events;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;

namespace Booth.DockerVolumeBackup.Application.Services
{
    public class BackupBackgroundService : BackgroundService
    {
        private const int SLEEP_TIME = 15;
        private readonly IDockerClient _DockerClient;
        private IMediator _Mediator;
        private readonly IBackupNotificationService _NotificationService;
        private ILogger<BackupBackgroundService> _Logger;
        public BackupBackgroundService(IDockerClient dockerClient, IMediator mediator, IBackupNotificationService notificationService, ILogger<BackupBackgroundService> logger) 
        {
            _DockerClient = dockerClient;
            _Mediator = mediator;
            _NotificationService = notificationService;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10000);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextBackup = await _Mediator.Send(new GetNextBackupToRunQuery());
                if (nextBackup > 0)
                {
                    await ExecuteBackupAsync(nextBackup, stoppingToken);
                }

                await Task.Delay(SLEEP_TIME * 1000);
            }
        }


        private async Task ExecuteBackupAsync(int backupId, CancellationToken stoppingToken)
        {
            _Logger.LogInformation($"Starting backup {backupId}");

            var backup = await _Mediator.Send(new GetBackupQuery(backupId));
            if (backup == null)
            {
                _Logger.LogError("Error loading backup definition");
                return;
            }
            await _Mediator.Publish(new BackupStatusChangedEvent(backupId, Status.Active));

            var volumeNames = backup.Volumes.Select(x => x.Volume);
            var volumeDefinitions = await GetBackupVolumes(volumeNames);
            var dependentServices = await GetDependentServices(volumeDefinitions);

            if (stoppingToken.IsCancellationRequested)
            {
                _Logger.LogWarning("Backup Cancelled");
                return;
            }

            try
            {
                _Logger.LogInformation("Stopping dependent services");
                await StopServices(dependentServices, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    _Logger.LogWarning("Backup Cancelled");
                    return;
                }

                // Create process to backup volumes
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "/bin/sh",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    };

                    _Logger.LogDebug("Starting shell process to run backup commands");
                    process.Start();
                    _Logger.LogDebug("Process started");

                    var backupFolder = $"/backup/{DateTime.Now.ToString("yyyy-MM-dd")}_{backupId}";
                    await RunShellCommand(process, $"mkdir {backupFolder}");
                    _Logger.LogInformation($"Backup destination folder {backupFolder} created");

                    foreach (var backupVolume in backup.Volumes)
                    {
                        var volumeDefinition = volumeDefinitions.FirstOrDefault(x => x.Name == backupVolume.Volume);
                        if (volumeDefinition == null)
                            continue;

                        _Logger.LogInformation($"Starting backup of volume {volumeDefinition.Name}");
                        await _Mediator.Publish(new BackupVolumeStatusChangedEvent(backupId, backupVolume.BackupVolumeId, Status.Active));

                        await RunShellCommand(process, $"tar -czf {backupFolder}/{volumeDefinition.Name}.tar.gz -C {volumeDefinition.Mountpoint} ./");

                        await _Mediator.Publish(new BackupVolumeStatusChangedEvent(backupId, backupVolume.BackupVolumeId, Status.Complete));
                        _Logger.LogInformation($"Backup of {volumeDefinition.Name} complete");
                    }

                };

            }
            finally
            {
                _Logger.LogInformation("Restarting dependent services");
                await RestartServices(dependentServices, CancellationToken.None);
            }

            await _Mediator.Publish(new BackupStatusChangedEvent(backupId, Status.Complete));
            _Logger.LogInformation("Backup complete");
        }

        private async Task RunShellCommand(Process process, string command)
        {
            var completedText = "--@!@Completed@!@--";

            _Logger.LogDebug($"Executing shell command '{command}'");
            await process.StandardInput.WriteLineAsync(command);
            await process.StandardInput.WriteLineAsync($"echo {completedText}");


            _Logger.LogDebug($"Command executed, waiting for completion");
            while (true)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                if ((output != null) && (output == completedText))
                    break;
            }

            _Logger.LogDebug("Command completed");
        }

        private async Task<List<Volume>> GetBackupVolumes(IEnumerable<string> volumeNames)
        {
            var allVolumes = await _DockerClient.Volumes.ListAsync();

            var backupVolumes = allVolumes.Where(x => volumeNames.Contains(x.Name));

            return backupVolumes.ToList();
        }

        private async Task<List<Service>> GetDependentServices(IEnumerable<Volume> volumes)
        {
            var services = new List<Service>();

            var allServices = await _DockerClient.Services.ListAsync();

            foreach (var service in allServices)
            {
                if (service.Spec.TaskTemplate.ContainerSpec.Mounts != null)
                {
                    foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                    {
                        if (volumes.Any(x => x.Name == mount.Source))
                        {
                            services.Add(service);
                            break;
                        }
                    }
                }
            }

            return services;
        }

        private async Task StopServices(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await _DockerClient.Services.ScaleAsync(service.Id, 0);
            }
        }

        private async Task RestartServices(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await _DockerClient.Services.ScaleAsync(service.Id, service.Spec.Mode.Replicated.Replicas);
            }
        }

    }
}
