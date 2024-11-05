namespace Booth.DockerVolumeBackup.WebApi.DataProviders.Models
{

    public enum Status { Queued, Active, Complete, Error }

    public class Backup
    {
        public int BackupId { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset ScheduledTime { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<BackupVolume> Volumes { get; set; } = new List<BackupVolume>();
    }

    public class BackupVolume
    {
        public int BackupVolumeId { get; set; }
        public required string Volume { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }
}
