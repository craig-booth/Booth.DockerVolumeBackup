namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface ISystemJobScheduler
    {
        void ScheduleJob(IBackgroundJob backupJob);
    }
}
