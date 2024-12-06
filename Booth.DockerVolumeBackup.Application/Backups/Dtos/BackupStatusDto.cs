namespace Booth.DockerVolumeBackup.Application.Backups.Dtos
{

    public class BackupStatusDto
    {
        public int BackupId { get; set; }
        public Status Status { get; set; }
        public List<VolumeBackupStatusDto> Volumes { get; set; } = new List<VolumeBackupStatusDto>();
    }

    public class VolumeBackupStatusDto
    {
        public string VolumeName { get; set; } = "";
        public Status Status { get; set; }
        public DateTimeOffset? BackupTime { get; set; }
    }
}
