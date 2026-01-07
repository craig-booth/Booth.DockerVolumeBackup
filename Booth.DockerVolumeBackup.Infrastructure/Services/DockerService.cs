using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Docker;

namespace Booth.DockerVolumeBackup.Infrastructure.Services
{
    internal class DockerService(IDockerClient dockerClient) : IDockerService
    {
        private static List<string>? _DependentVolumes = null;

        public async Task<List<Volume>> GetVolumesAsync()
        {
            var volumes = await dockerClient.Volumes.ListAsync();

            return volumes.Select(x => new Volume { Name = x.Name, MountPoint = x.Mountpoint, Size = x.UsageData != null ? x.UsageData.Size : 0 }).ToList();
        }

        public async Task<List<Service>> GetDependentServicesAsync(IEnumerable<Volume> volumes)
        {
            var services = new List<Service>();

            var allServices = await dockerClient.Services.ListAsync();

            foreach (var service in allServices)
            {
                if (service != null)
                {
                    var serviceId = service.Id;
                    var replicas = service.Spec?.Mode?.Replicated?.Replicas;

                    if (service?.Spec?.TaskTemplate?.ContainerSpec?.Mounts != null)
                    {
                        foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                        {
                            if (volumes.Any(x => x.Name == mount.Source))
                            {
                                services.Add(new Service { Id = serviceId, Replicas = replicas ?? 0 });
                                break;
                            }
                        }
                    }
                }

            }

            return services;
        }
        public async Task StopServicesAsync(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await dockerClient.Services.ScaleAsync(service.Id, 0);
            }
        }

        public async Task StartServicesAsync(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await dockerClient.Services.ScaleAsync(service.Id, service.Replicas);
            }
        }

        public async Task<List<string>> GetDependentVolumesAsync()
        {
            if (_DependentVolumes == null)
            {
                var containers = await dockerClient.Containers.ListAsync();
                if (containers == null)
                    return new List<string>();

                var thisContainer = containers.FirstOrDefault(x => x.Command == "dotnet Booth.DockerVolumeBackup.WebApi.dll");
                if (thisContainer == null)
                    return new List<string>();

                _DependentVolumes = thisContainer.Mounts.Where(x => x.Type == "volume").Select(x => x.Name).ToList();
            }

            return _DependentVolumes;
        }

        public async Task<Volume?> CreateVolumeAsync(string name)
        {
            var volume = await dockerClient.Volumes.CreateAsync(name);
            if (volume == null)
                return null;

            return new Volume { Name = volume.Name, MountPoint = volume.Mountpoint, Size = volume.UsageData != null ? volume.UsageData.Size : 0 };
        }
    }
}
