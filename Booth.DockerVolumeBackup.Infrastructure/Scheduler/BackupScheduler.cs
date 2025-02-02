using Coravel.Queuing.Interfaces;
using Coravel.Scheduling.Schedule.Interfaces;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Booth.DockerVolumeBackup.Infrastructure.Scheduler
{
    internal class BackupScheduler(IQueue queue, IScheduler scheduler) : IBackupScheduler
    {

        public void QueueBackup(IBackgroundJob backupJob)
        {
            queue.QueueAsyncTask(async () =>
            {
                await backupJob.Execute(CancellationToken.None);
            });
        }

        public void ScheduleBackup(BackupSchedule schedule, IBackgroundJob backupJob)
        {
            var scheduledJob = scheduler.ScheduleAsync(async () =>
            {
                await backupJob.Execute(CancellationToken.None);
            }).DailyAt(schedule.Time.Hour, schedule.Time.Minute);

            if (schedule.Sunday) scheduledJob.Sunday();
            if (schedule.Monday) scheduledJob.Monday();
            if (schedule.Tuesday) scheduledJob.Tuesday();
            if (schedule.Wednesday) scheduledJob.Wednesday();
            if (schedule.Thursday) scheduledJob.Thursday();
            if (schedule.Friday) scheduledJob.Friday();
            if (schedule.Saturday) scheduledJob.Saturday();  
        }
    }
}
