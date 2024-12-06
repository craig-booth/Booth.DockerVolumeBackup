namespace Booth.DockerVolumeBackup.Domain
{ 
    public class BackupVolume
    {
        public int BackupVolumeId { get; set; }
        public int BackupId { get; set; }
        public required string Volume { get ; set; }
        public Status Status { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
