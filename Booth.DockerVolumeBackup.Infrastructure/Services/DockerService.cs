using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Docker;

namespace Booth.DockerVolumeBackup.Infrastructure.Services
{
    internal class DockerService(IDockerClient dockerClient) : IDockerService
    {

        public async Task<List<Volume>> GetVolumesAsync()
        {
            var volumes = await dockerClient.Volumes.ListAsync();

            return volumes.Select(x => new Volume { Name = x.Name, MountPoint = x.Mountpoint, Size = x.UsageData != null ? x.UsageData.Size : 0 }).ToList();
        }

        public async Task<List<Service>> GetDependentServices(IEnumerable<Volume> volumes)
        {
            var services = new List<Service>();

            var allServices = await dockerClient.Services.ListAsync();

            foreach (var service in allServices)
            {

                if (service?.Spec?.TaskTemplate?.ContainerSpec?.Mounts != null)
                {
                    foreach (var mount in service.Spec.TaskTemplate.ContainerSpec.Mounts)
                    {
                        if (volumes.Any(x => x.Name == mount.Source))
                        {
                            var replicas = service?.Spec?.Mode?.Replicated?.Replicas;
                            services.Add(new Service { Id = service.Id, Replicas = replicas ?? 0 });
                            break;
                        }
                    }
                }
            }

            return services;
        }
        public async Task StopServices(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await dockerClient.Services.ScaleAsync(service.Id, 0);
            }
        }

        public async Task StartServices(IEnumerable<Service> services, CancellationToken stoppingToken)
        {
            foreach (var service in services)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await dockerClient.Services.ScaleAsync(service.Id, service.Replicas);
            }
        }
    }
}
