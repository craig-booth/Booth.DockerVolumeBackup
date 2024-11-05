using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.Docker.Models
{
    public class Service
    {
        public string Id { get; set; }
        public ServiceVersion Version { get; set; }
        public ServiceSpec Spec { get; set; }

    }

    public class ServiceSpec
    {
        public string Name { get; set; }

        public ServiceTaskTemplate TaskTemplate { get; set; }
        public ServiceMode Mode { get; set; }

    }

    public class ServiceVersion
    {
        public int Index { get; set; }
    }
    public class ServiceMode
    {
        public ServiceModeReplicated Replicated { get; set; }
    }

    public class ServiceModeReplicated
    {
        public int Replicas { get; set; }
    }

    public class ServiceTaskTemplate
    {
        public ServiceContainerSpec ContainerSpec { get; set; }
    }

    public class ServiceContainerSpec
    {
        public List<ServiceMount> Mounts { get; set; }
    }

    public class ServiceMount
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
    }
}
