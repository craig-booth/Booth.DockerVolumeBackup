namespace Booth.DockerVolumeBackup.Infrastructure.Docker.Models
{
    public class VolumeConfiguration
    {
        public required string Name { get; set; }
        public string Driver { get; set; } = "local";
        public IDictionary<string, string> DriverOpts { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        public ClusterVolumeSpec? ClusterVolume { get; set; }
    }

    public class ClusterVolumeSpec
    {
        public required string Group { get; set; }
    }
}
