namespace Booth.DockerVolumeBackup.WebApi.Models
{
    public class VolumeBackupRequest
    {
        public string[] Volumes { get; set; } = [];
    }
}
