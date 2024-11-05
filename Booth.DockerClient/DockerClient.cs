using Booth.Docker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Booth.Docker
{

    internal class DockerClient : IDockerClient
    {
        public IVolumeResource Volumes { get; }
        public IServiceResource Services { get; }

        public DockerClient(HttpClient httpClient)
        {
            Volumes = new VolumeResource(httpClient);
            Services = new ServiceResource(httpClient);
        }
    }
}
