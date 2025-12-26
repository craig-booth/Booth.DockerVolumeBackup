namespace Booth.DockerVolumeBackup.Infrastructure.Docker.Models
{
    public class Volume
    {
        public required string Name { get; set; }
        public required string Mountpoint { get; set; }
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        public VolumeUsageData? UsageData { get; set; }
    }

    public class VolumeUsageData
    {
        public long Size { get; set; }
        public int RefCount { get; set; }
    }
}
