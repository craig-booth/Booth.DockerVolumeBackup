using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Infrastructure.Docker
{
    public interface IDockerClient
    {
        IVolumeResource Volumes { get; }
        IServiceResource Services { get; }
    }

    public interface IVolumeResource
    {
        Task<IList<Volume>> ListAsync();
    }

    public interface IServiceResource
    {
        Task<IList<Service>> ListAsync();
        Task ScaleAsync(string id, int scale);
    }
}
