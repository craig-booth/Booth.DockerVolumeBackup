namespace Booth.DockerVolumeBackup.Application.Volumes.Dtos
{
    public class VolumeDto
    {
        public required string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? LastBackup { get; set; }
    }
}
