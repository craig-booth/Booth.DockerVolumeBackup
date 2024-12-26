namespace Booth.DockerVolumeBackup.Application.Backups.Dtos
{
    public enum Status { Queued, Active, Complete, Error }

    public class BackupDto
    {
        public int BackupId { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public Status Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public List<BackupVolumeDto> Volumes { get; set; } = new List<BackupVolumeDto>();
    }

    public class BackupVolumeDto
    {
        public int BackupVolumeId { get; set; }
        public string Volume { get; set; } = string.Empty;
        public Status Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }
}
