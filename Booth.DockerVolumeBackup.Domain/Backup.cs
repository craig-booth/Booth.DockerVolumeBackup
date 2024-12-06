
namespace Booth.DockerVolumeBackup.Domain
{
    public enum Status { Queued, Active, Complete, Error }

    public class Backup
    {
        public int BackupId { get;set; }
        public Status Status { get;set; }
        public int? ScheduleId { get ;set; }
        public DateTimeOffset ScheduledTime { get;set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
