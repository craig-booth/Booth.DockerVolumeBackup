using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Infrastructure.Docker.Models
{
    public class Service
    {
        public required string Id { get; set; }
        public ServiceVersion? Version { get; set; }
        public ServiceSpec? Spec { get; set; }

    }

    public class ServiceSpec
    {
        public required string Name { get; set; }

        public ServiceTaskTemplate? TaskTemplate { get; set; }
        public ServiceMode? Mode { get; set; }

    }

    public class ServiceVersion
    {
        public int Index { get; set; }
    }
    public class ServiceMode
    {
        public ServiceModeReplicated? Replicated { get; set; }
    }

    public class ServiceModeReplicated
    {
        public int Replicas { get; set; }
    }

    public class ServiceTaskTemplate
    {
        public ServiceContainerSpec? ContainerSpec { get; set; }
    }

    public class ServiceContainerSpec
    {
        public List<ServiceMount> Mounts { get; set; } = new List<ServiceMount>();
    }

    public class ServiceMount
    {
        public required string Source { get; set; }
        public required string Target { get; set; }
        public required string Type { get; set; }
    }
}
