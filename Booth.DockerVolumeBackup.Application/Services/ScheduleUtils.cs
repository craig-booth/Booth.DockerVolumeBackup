using Microsoft.EntityFrameworkCore;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Services
{
    internal interface IScheduleUtils
    {
        Task<Backup?> CreateBackupFromScheduleAsync(int scheduleId, CancellationToken cancellationToken);
    }

    internal class ScheduleUtils(IDataContext dataContext) : IScheduleUtils
    {
        public async Task<Backup?> CreateBackupFromScheduleAsync(int scheduleId, CancellationToken cancellationToken)
        {
            var backupDefinition = await dataContext.Schedules
                .Where(x => x.ScheduleId == scheduleId)
                .Include(x => x.BackupDefinition)
                .ThenInclude(x => x.Volumes)
                .Select(x => x.BackupDefinition)
                .FirstOrDefaultAsync(cancellationToken);
            if (backupDefinition == null)
                return null;


            var backup = new Backup
            {
                ScheduleId = scheduleId,
                Status = Status.Queued,
                BackupType = BackupType.Scheduled
            };
            backup.Volumes.AddRange(backupDefinition.Volumes.Select(x => new BackupVolume { Volume = x.Volume, Status = Status.Queued }));

            dataContext.Backups.Add(backup);
            await dataContext.SaveChangesAsync(cancellationToken);

            return backup;
        }
    }
}
