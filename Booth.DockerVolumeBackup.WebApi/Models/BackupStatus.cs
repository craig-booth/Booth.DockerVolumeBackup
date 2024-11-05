namespace Booth.DockerVolumeBackup.WebApi.Models
{

    public enum Status { Queued, Active, Complete, Error}

    public class BackupStatus
    {
        public int BackupId { get; set; }
        public Status Status { get; set; }
        public List<VolumeBackupStatus> Volumes { get; set; } = new List<VolumeBackupStatus>();
    }

    public class VolumeBackupStatus
    {
        public string VolumeName { get; set; } = "";
        public Status Status { get; set; }
        public DateTimeOffset? BackupTime { get; set; }
    }
}
