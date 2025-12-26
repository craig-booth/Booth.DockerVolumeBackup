namespace Booth.DockerVolumeBackup.Domain.Models
{
    public class BackupDefinition
    {
        public int BackupDefinitionId { get; set; }
        public int ScheduleId { get; set; }
        public int KeepLast { get; set; }
        public List<BackupDefinitionVolume> Volumes { get; set; } = [];
    }

    public class BackupDefinitionVolume
    {
        public int BackupDefinitionVolumeId { get; set; }
        public int BackupDefinitionId { get; set; }
        public required string Volume { get; set; }
    }
}
