using Booth.DockerVolumeBackup.Application.Volumes.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IDockerService
    {
        Task<List<Volume>> GetVolumesAsync();
        Task<List<Service>> GetDependentServices(IEnumerable<Volume> volumes);
        Task StopServices(IEnumerable<Service> services, CancellationToken stoppingToken);
        Task StartServices(IEnumerable<Service> services, CancellationToken stoppingToken);
    }

    public class Volume
    {
        public string Name { get; set; }
        public string MountPoint { get; set; }
        public long Size { get; set; }
    }

    public class Service
    {
        public string Id { get; set; }
        public int Replicas { get; set; }
    }
}
