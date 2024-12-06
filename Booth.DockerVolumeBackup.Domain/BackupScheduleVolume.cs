
namespace Booth.DockerVolumeBackup.Domain
{
    public class BackupScheduleVolume
    {
        public int BackupScheduleVolumeId { get; set; }
        public int BackupScheduleId { get; set; }
        public required string Volume { get; set; }
    }
}
