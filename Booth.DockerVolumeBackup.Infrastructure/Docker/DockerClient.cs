namespace Booth.DockerVolumeBackup.Infrastructure.Docker
{

    internal class DockerClient : IDockerClient
    {
        public IVolumeResource Volumes { get; }
        public IServiceResource Services { get; }
        public IContainerResource Containers { get; }

        public DockerClient(HttpClient httpClient)
        {
            Volumes = new VolumeResource(httpClient);
            Services = new ServiceResource(httpClient);
            Containers = new ContainerResource(httpClient);
        }
    }
}
