namespace Booth.DockerVolumeBackup.WebApi.Models
{
    public class Volume
    {
        public required string Name { get; set; }
        public Int64 Size { get; set; }
        public DateTimeOffset? LastBackup { get; set; }
    }
}
