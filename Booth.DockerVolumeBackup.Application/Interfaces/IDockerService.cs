namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IDockerService
    {
        Task<List<Volume>> GetVolumesAsync();
        Task<Volume?> CreateVolumeAsync(string name);
        Task<List<string>> GetDependentVolumesAsync();
        Task<List<Service>> GetDependentServicesAsync(IEnumerable<Volume> volumes);
        Task StopServicesAsync(IEnumerable<Service> services, CancellationToken stoppingToken);
        Task StartServicesAsync(IEnumerable<Service> services, CancellationToken stoppingToken);
    }

    public class Volume
    {
        public required string Name { get; set; }
        public required string MountPoint { get; set; }
        public long Size { get; set; }
    }

    public class Service
    {
        public required string Id { get; set; }
        public int Replicas { get; set; }
    }
}
