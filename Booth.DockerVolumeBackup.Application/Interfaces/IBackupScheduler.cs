using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IBackupScheduler
    {
        void QueueJob(IBackgroundJob backupJob);
        void ScheduleBackup(BackupSchedule schedule, IBackgroundJob backupJob);
        void RemoveScheduledBackup(int scheduleId);
    }
}
