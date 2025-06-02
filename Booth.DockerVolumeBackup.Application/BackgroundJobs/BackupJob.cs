using Microsoft.Extensions.Logging;

using MediatR;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace Booth.DockerVolumeBackup.Application.BackgroundJobs
{

     internal class BackupJob(int backupId, IServiceScopeFactory scopeFactory) : IBackgroundJob
     {       
        private IPublisher? _Publisher = null;

        public int Id => backupId;

        public async Task Execute(CancellationToken cancellationToken)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                _Publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var dockerService = scope.ServiceProvider.GetRequiredService<IDockerService>();
                var mountPointBackupService = scope.ServiceProvider.GetRequiredService<IMountPointBackupService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<BackupJob>>();

                var backup = await dataContext.Backups
                    .AsTracking()
                    .Where(x => x.BackupId == backupId)
                    .Include(x => x.Volumes)
                    .SingleAsync(cancellationToken);

                if (backup == null)
                {
                    logger.LogError("Error loading backup definition with ID {Id}", backupId);
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                logger.LogInformation("Starting backup {Id}", backupId);

                backup.BackupStatusChanged += Backup_BackupStatusChanged;
                backup.BackupVolumeStatusChanged += Backup_BackupVolumeStatusChanged;

                backup.StartBackup();
                await dataContext.SaveChangesAsync(cancellationToken);
                
                var volumeNames = backup.Volumes.Select(x => x.Volume);
                var allVolumes = await dockerService.GetVolumesAsync();
                var volumeDefinitions = allVolumes.Where(x => volumeNames.Contains(x.Name));

                var dependentServices = await dockerService.GetDependentServices(volumeDefinitions);

                cancellationToken.ThrowIfCancellationRequested();

                var backupSuccessful = false;
                try
                {
                    // Create backup folder
                    var backupFolder = $"/backup/{DateTime.Now:yyyy-MM-dd}_{backup.BackupId}";
                    var folderCreated = await mountPointBackupService.CreateDirectoryAsync(backupFolder);
                    if (folderCreated)
                    {
                        logger.LogInformation("Backup destination folder {Folder} created", backupFolder);
                    }
                    else
                    {
                        logger.LogError("Error creating backup destination folder {Folder}", backupFolder);
                        return;
                    }

                    logger.LogInformation("Stopping dependent services");
                    await dockerService.StopServices(dependentServices, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    var allVolumesBackedUp = true;

                    foreach (var backupVolume in backup.Volumes)
                    {
                        var volumeDefinition = volumeDefinitions.FirstOrDefault(x => x.Name == backupVolume.Volume);
                        if (volumeDefinition == null)
                            continue;

                        logger.LogInformation("Starting backup of volume {Volumne}", volumeDefinition.Name);

                        var fileName = $"{volumeDefinition.Name}.tar.gz";
                        backup.StartVolumeBackup(backupVolume.Volume, fileName);
                        await dataContext.SaveChangesAsync(cancellationToken);

                        var backupSize = await mountPointBackupService.BackupDirectoryAsync(volumeDefinition.MountPoint, $"{backupFolder}/{fileName}");
                        var volumeBackedUp = backupSize > 0;

                        backup.EndVolumeBackup(backupVolume.Volume, volumeBackedUp, backupSize);
                        await dataContext.SaveChangesAsync(cancellationToken);
                        logger.LogInformation("Backup of {Volume} complete", volumeDefinition.Name);

                        allVolumesBackedUp &= volumeBackedUp;

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    backupSuccessful = allVolumesBackedUp;
                }
                finally
                {
                    logger.LogInformation("Restarting dependent services");
                    await dockerService.StartServices(dependentServices, cancellationToken);

                    backup.EndBackup(backupSuccessful);
                    await dataContext.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Backup complete");
                }

            }
        }


        private void Backup_BackupStatusChanged(object? sender, BackupStatusChangedEvent e)
        {
            _Publisher?.Publish(e);
        }

        private void Backup_BackupVolumeStatusChanged(object? sender, BackupVolumeStatusChangedEvent e)
        {
            _Publisher?.Publish(e);
        } 
    }
}
