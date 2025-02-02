using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IBackgroundJob
    {
        int Id { get; }
        Task Execute(CancellationToken cancellationToken);
    }
}
