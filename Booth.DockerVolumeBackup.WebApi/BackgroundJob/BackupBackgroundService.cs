
using Booth.Docker;
using Booth.Docker.Models;
using Booth.DockerVolumeBackup.WebApi.DataProviders;
using Booth.DockerVolumeBackup.WebApi.DataProviders.Models;

namespace Booth.DockerVolumeBackup.WebApi.Backup
{
    public class BackupBackgroundService : BackgroundService
    {
        private const int SLEEP_TIME = 15;
        private readonly IDockerClient _DockerClient;
        private readonly IBackupDataProvider _DataProvider;
        private readonly IBackupNotificationService _NotificationService;
        public BackupBackgroundService(IDockerClient dockerClient, IBackupDataProvider dataProvider, IBackupNotificationService notificationService) 
        {
            _DockerClient = dockerClient;
            _DataProvider = dataProvider;
            _NotificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10000);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextBackup = await _DataProvider.GetNextBackupIdAsync();
                if (nextBackup > 0)
                {
                    await ExecuteBackupAsync(nextBackup, stoppingToken);
                }

                await Task.Delay(SLEEP_TIME * 1000);
            }
        }


        private async Task ExecuteBackupAsync(int backupId, CancellationToken stoppingToken)
        {
            var backup = await _DataProvider.GetBackupAsync(backupId);
            if (backup == null)
                return;

            await _DataProvider.UpdateBackupStatusAsync(backup.BackupId, Status.Active);
            _NotificationService.SignalStatusChanged(backup.BackupId);

            var volumeNames = backup.Volumes.Select(x => x.Volume);
            var volumeDefinitions = await GetBackupVolumes(volumeNames);
            var dependentServices = await GetDependentServices(volumeDefinitions);

            if (stoppingToken.IsCancellationRequested)
                return;
         
            try
            {
                await StopServices(dependentServices, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    return;

                foreach (var backupVolume in backup.Volumes)
                {
                    var volumeDefinition = volumeDefinitions.FirstOrDefault(x => x.Name == backupVolume.Volume);
                    if (volumeDefinition == null)
                        continue;

                    await BackupVolume(backupVolume.BackupVolumeId, volumeDefinition, stoppingToken);
                }
            }
            finally
            {
                await RestartServices(dependentServices, CancellationToken.None);
            }


            await _DataProvider.UpdateBackupStatusAsync(backup.BackupId, Status.Complete);
            _NotificationService.SignalStatusChanged(backup.BackupId);
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

        private async Task BackupVolume(int backupVolumeId, Volume volume, CancellationToken stoppingToken)
        {
            await _DataProvider.UpdateVolumeStatusAsync(backupVolumeId, Status.Active);
            _NotificationService.SignalStatusChanged(backupVolumeId);

            var command = $"tar -czf /backup/{volume.Name}.tar.gz -C {volume.Mountpoint} ./";



            await Task.Delay(10000, stoppingToken);

            await _DataProvider.UpdateVolumeStatusAsync(backupVolumeId, Status.Complete);
            _NotificationService.SignalStatusChanged(backupVolumeId);
        }
    }
}
