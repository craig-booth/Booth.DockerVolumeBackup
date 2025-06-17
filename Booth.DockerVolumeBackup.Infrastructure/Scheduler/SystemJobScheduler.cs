using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Coravel.Queuing.Interfaces;
using Coravel.Scheduling.Schedule;
using Coravel.Scheduling.Schedule.Event;
using Coravel.Scheduling.Schedule.Interfaces;


namespace Booth.DockerVolumeBackup.Infrastructure.Scheduler
{
    internal class SystemJobScheduler(IScheduler scheduler) : ISystemJobScheduler
    {
        public void ScheduleJob(IBackgroundJob backupJob)
        {
            var scheduledJob = scheduler.ScheduleAsync(async () =>
            {
                await backupJob.Execute(CancellationToken.None);
            }).Weekly().Zoned(TimeZoneInfo.Local).RunOnceAtStart();
        }
    }
}
