using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Booth.DockerVolumeBackup.Application.BackgroundJobs
{

    internal class LoadUnmanagedBackupsJob(IServiceScopeFactory scopeFactory) : IBackgroundJob
    {
        public int Id => -1;

        public async Task Execute(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var scope = scopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var mountPointService = scope.ServiceProvider.GetRequiredService<IMountPointBackupService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<LoadUnmanagedBackupsJob>>();

                var allBackups = await dataContext.Backups
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Searching for unmanaged backups");

                cancellationToken.ThrowIfCancellationRequested();

                // Get unmanaged backups
                var backupDirectories = await mountPointService.GetBackupDirectoriesAsync("/backup");
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var backupDirectory in backupDirectories)
                {
                    var backupPath = "/backup/" + backupDirectory.Name;

                    // Only include unmanaged backups that do not have the same name as an existing backup
                    if (!allBackups.Any(x => x.BackupDirectory == backupPath))
                    {
                        var backupFiles = await mountPointService.GetBackupFilesAsync(backupPath);

                        if (backupFiles.Count > 0)
                        {
                            var newBackup = new Backup
                            {
                                BackupType = BackupType.Unmanaged,
                                BackupDirectory = backupPath,
                                StartTime = backupDirectory.CreationDate,
                                EndTime = backupDirectory.CreationDate,
                                Status = Status.Complete
                            };

                            var volumeBackups = backupFiles.Select(x => new BackupVolume()
                            {
                                Volume = GetVolumeName(x.Name),
                                BackupFile = x.Name,
                                BackupSize = x.Size,
                                StartTime = x.CreationDate,
                                EndTime = x.CreationDate,
                                Status = Status.Complete
                            });
                            newBackup.Volumes.AddRange(volumeBackups);

                            dataContext.Backups.Add(newBackup);
                        }
                    }
                }
                await dataContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Loading unmanaged backups job completed successfully");
            }
        }
        private string GetVolumeName(string fileName)
        {
            var index = fileName.IndexOf('.');
            if (index > 0)
            {
                return fileName.Substring(0, index);
            }
            else
            {
                return fileName;
            }
        }


    }
}
