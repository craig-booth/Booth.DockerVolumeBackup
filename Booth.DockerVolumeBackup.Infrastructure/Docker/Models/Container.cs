using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Infrastructure.Docker.Models
{
    public class Container
    {
        public required string Id { get; set; } 
        public required string Image { get; set; }
        public string Command { get; set; } = "";
        public List<ContainerMount> Mounts { get; set; } = new List<ContainerMount>();
    }


    public class ContainerMount
    {
        public required string Type { get; set; }
        public string Name { get; set; } = "";
        public string Destination { get; set; } = "";
    }
}
