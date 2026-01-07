using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Booth.DockerVolumeBackup.Application.BackgroundJobs
{

    internal class RestoreJob(int volumeBackupId, string volumeName, IServiceScopeFactory scopeFactory) : IBackgroundJob
    {
        public int Id => volumeBackupId;

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var scope = scopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var dockerService = scope.ServiceProvider.GetRequiredService<IDockerService>();
                var mountPointBackupService = scope.ServiceProvider.GetRequiredService<IMountPointBackupService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<RestoreJob>>();

                logger.LogInformation("Starting restore of backup '{VolumeBackupId}' to volume {VolumeNaem}", volumeBackupId, volumeName);

                var query = dataContext.Backups
                  .Include(x => x.Volumes)
                  .Where(x => x.Volumes.Any(v => v.BackupVolumeId == volumeBackupId));

                var backup = await query.FirstOrDefaultAsync(cancellationToken);
                if (backup == null)
                {
                    logger.LogError("Backup not found for Volume BackupId '{VolumeBackupId}'", volumeBackupId);
                    return;
                }

                var volume = backup.Volumes.FirstOrDefault(x => x.BackupVolumeId == volumeBackupId);
                if (volume == null)
                {
                    logger.LogError("Backup not found for Volume BackupId '{VolumeBackupId}'", volumeBackupId);
                    return;
                }

                if (backup.BackupDirectory == null || volume.BackupFile == null)
                {
                    logger.LogError("Cannot restore, the backup was not found");
                    return;
                }

                // Check that the backup was successful
                if (backup.Status != Status.Complete)
                {
                    logger.LogError("Cannot restore, the backup was not completed successfully");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // If the volume does not exist, create it
                var volumes = await dockerService.GetVolumesAsync();
                var dockerVolume = volumes.FirstOrDefault(x => x.Name == volumeName);
                if (dockerVolume == null)
                {
                    dockerVolume = await dockerService.CreateVolumeAsync(volumeName);
                    if (dockerVolume == null)
                    {
                        logger.LogError("An error occured when creating volume {VolumeName}", volumeName);
                        return;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Restore from backup
                var backupPath = Path.Combine(backup.BackupDirectory, volume.BackupFile);
                logger.LogInformation("Restoring volume from backup file '{BackupPath}' into '{MountPoint}'", backupPath, dockerVolume.MountPoint);
                var restoreSuccessfull = await mountPointBackupService.RestoreDirectoryAsync(dockerVolume.MountPoint, backupPath);

                if (restoreSuccessfull)
                    logger.LogInformation("Restore completed successfullly");
                else
                    logger.LogError("Restore failed");
            }
        }

    }
}
