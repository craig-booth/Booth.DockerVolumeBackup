using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using MediatR;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Events;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Services
{
    public interface IBackupService
    {
        Task RunBackupAsync(Backup backup, CancellationToken stoppingToken);
    }

    internal class BackupService : IBackupService
    {

        private readonly IDockerService _DockerService;
        private readonly IMountPointBackupService _MountPointBackupService;
        private readonly IPublisher _Publisher;
        private readonly ILogger<BackupService> _Logger;
        public BackupService(IDockerService dockerService, IMountPointBackupService mountPointBackupService, IPublisher publisher, ILogger<BackupService> logger)
        {
            _DockerService = dockerService;
            _MountPointBackupService = mountPointBackupService;
            _Publisher = publisher;
            _Logger = logger;
        }

        public async Task RunBackupAsync(Backup backup, CancellationToken stoppingToken)
        {
            _Logger.LogInformation($"Starting backup {backup.BackupId}");

            backup.BackupStatusChanged += Backup_BackupStatusChanged;
            backup.BackupVolumeStatusChanged += Backup_BackupVolumeStatusChanged;
                      
            backup.StartBackup();

            var volumeNames = backup.Volumes.Select(x => x.Volume);

            var allVolumes = await _DockerService.GetVolumesAsync();
            var volumeDefinitions = allVolumes.Where(x => volumeNames.Contains(x.Name));

            var dependentServices = await _DockerService.GetDependentServices(volumeDefinitions);

            if (stoppingToken.IsCancellationRequested)
            {
                _Logger.LogWarning("Backup Cancelled");
                return;
            }

            try
            {
                _Logger.LogInformation("Stopping dependent services");
                await _DockerService.StopServices(dependentServices, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                {
                    _Logger.LogWarning("Backup Cancelled");
                    return;
                }

                // Create backup folder
                var backupFolder = $"/backup/{DateTime.Now.ToString("yyyy-MM-dd")}_{backup.BackupId}";
                await _MountPointBackupService.CreateDirectoryAsync(backupFolder);
                _Logger.LogInformation($"Backup destination folder {backupFolder} created");

                foreach (var backupVolume in backup.Volumes)
                {
                    var volumeDefinition = volumeDefinitions.FirstOrDefault(x => x.Name == backupVolume.Volume);
                    if (volumeDefinition == null)
                        continue;

                    _Logger.LogInformation($"Starting backup of volume {volumeDefinition.Name}");
                    backup.StartVolumeBackup(backupVolume.Volume);

                    await _MountPointBackupService.BackupDirectoryAsync(volumeDefinition.MountPoint, $"{backupFolder}/{volumeDefinition.Name}.tar.gz");

                    backup.EndVolumeBackup(backupVolume.Volume);
                    _Logger.LogInformation($"Backup of {volumeDefinition.Name} complete");
                }
            }
            finally
            {
                _Logger.LogInformation("Restarting dependent services");
                await _DockerService.StartServices(dependentServices, CancellationToken.None);
            }

            backup.EndBackup();
            _Logger.LogInformation("Backup complete"); 
        }


        private void Backup_BackupStatusChanged(object? sender, BackupStatusChangedEvent e)
        {
            _Publisher.Publish(e);
        }

        private void Backup_BackupVolumeStatusChanged(object? sender, BackupVolumeStatusChangedEvent e)
        {
            _Publisher.Publish(e);
        }

    }
}
