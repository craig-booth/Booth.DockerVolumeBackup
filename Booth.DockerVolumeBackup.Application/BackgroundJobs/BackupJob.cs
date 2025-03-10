﻿using Microsoft.Extensions.Logging;

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

                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Backup {Id} cancelled", backupId);
                    return;
                }

                if (backup == null)
                {
                    logger.LogError("Error loading backup definition with ID {Id}", backupId);
                    return;
                }

                logger.LogInformation("Starting backup {Id}", backupId);

                backup.BackupStatusChanged += Backup_BackupStatusChanged;
                backup.BackupVolumeStatusChanged += Backup_BackupVolumeStatusChanged;

                backup.StartBackup();
                await dataContext.SaveChangesAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Backup {Id} cancelled", backupId);
                    return;
                }

                var volumeNames = backup.Volumes.Select(x => x.Volume);

                var allVolumes = await dockerService.GetVolumesAsync();
                var volumeDefinitions = allVolumes.Where(x => volumeNames.Contains(x.Name));

                var dependentServices = await dockerService.GetDependentServices(volumeDefinitions);
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Backup {Id} cancelled", backupId);
                    return;
                }

                try
                {
                    logger.LogInformation("Stopping dependent services");
                    await dockerService.StopServices(dependentServices, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Backup {Id} cancelled", backupId);
                        return;
                    }

                    // Create backup folder
                    var backupFolder = $"/backup/{DateTime.Now:yyyy-MM-dd}_{backup.BackupId}";
                    await mountPointBackupService.CreateDirectoryAsync(backupFolder);
                    logger.LogInformation("Backup destination folder {Folder} created", backupFolder);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.LogWarning("Backup {Id} cancelled", backupId);
                        return;
                    }

                    foreach (var backupVolume in backup.Volumes)
                    {
                        var volumeDefinition = volumeDefinitions.FirstOrDefault(x => x.Name == backupVolume.Volume);
                        if (volumeDefinition == null)
                            continue;

                        logger.LogInformation("Starting backup of volume {Volumne}", volumeDefinition.Name);
                        backup.StartVolumeBackup(backupVolume.Volume);
                        await dataContext.SaveChangesAsync(cancellationToken);

                        await mountPointBackupService.BackupDirectoryAsync(volumeDefinition.MountPoint, $"{backupFolder}/{volumeDefinition.Name}.tar.gz");

                        backup.EndVolumeBackup(backupVolume.Volume);
                        await dataContext.SaveChangesAsync(cancellationToken);
                        logger.LogInformation("Backup of {Volume} complete", volumeDefinition.Name);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            logger.LogWarning("Backup {Id} cancelled", backupId);
                            return;
                        }
                    }
                }
                finally
                {
                    logger.LogInformation("Restarting dependent services");
                    await dockerService.StartServices(dependentServices, cancellationToken);
                }

                backup.EndBackup();
                await dataContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Backup complete");
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
