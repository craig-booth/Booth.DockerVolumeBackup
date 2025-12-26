using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;

namespace Booth.DockerVolumeBackup.Infrastructure.Docker
{
    public interface IDockerClient
    {
        IVolumeResource Volumes { get; }
        IServiceResource Services { get; }
        IContainerResource Containers { get; }
    }

    public interface IVolumeResource
    {
        Task<IList<Volume>> ListAsync();
        Task<Volume?> CreateAsync(string name);
    }

    public interface IServiceResource
    {
        Task<IList<Service>> ListAsync();
        Task ScaleAsync(string id, int scale);
    }

    public interface IContainerResource
    {
        Task<IList<Container>> ListAsync();
    }
}
