using Microsoft.Extensions.Logging;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace Booth.DockerVolumeBackup.Application.BackgroundJobs
{

     internal class CleanOldBackupsJob(IServiceScopeFactory scopeFactory) : IBackgroundJob
     {       
        public int Id => -1;

        public async Task Execute(CancellationToken cancellationToken)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var mountPointBackupService = scope.ServiceProvider.GetRequiredService<IMountPointBackupService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<CleanOldBackupsJob>>();

                logger.LogInformation("Starting cleanup of old backups");

                var backupsQuery = dataContext.Backups
                    .AsTracking()
                    .Where(x => x.ScheduleId.HasValue && (x.Status == Status.Complete || x.Status == Status.Error));
                var backups = await backupsQuery.ToListAsync(cancellationToken);

                var schedulesQuery = dataContext.Schedules
                    .Include(x => x.BackupDefinition)
                    .Where(x => x.BackupDefinition.KeepLast > 0);
                var schedules = await schedulesQuery.ToListAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                // Clean up old backups based on KeepLast setting
                foreach (var schedule in schedules)
                {
                    var scheduleBackups = backups.Where(x => x.ScheduleId == schedule.ScheduleId)
                                                    .OrderByDescending(x => x.StartTime)
                                                    .ToList();
                    var successfulBackups = 0;
                    foreach (var backup in scheduleBackups)
                    {
                        if (backup.Status == Status.Complete)
                            successfulBackups++;

                        if (successfulBackups > schedule.BackupDefinition.KeepLast)
                        {
                            logger.LogInformation("Deleting old backup {BackupId} for schedule {ScheduleId}", backup.BackupId, schedule.ScheduleId);

                            // Delete the backup
                            if (backup.BackupDirectory != null) 
                                await mountPointBackupService.DeleteDirectoryAsync(backup.BackupDirectory);

                            dataContext.Backups.Remove(backup);
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                await dataContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Cleanup of old backups completed successfully");
            }
        }
 
    }
}
