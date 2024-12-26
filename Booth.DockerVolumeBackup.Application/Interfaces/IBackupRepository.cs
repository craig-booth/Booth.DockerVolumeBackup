using Booth.DockerVolumeBackup.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IBackupRepository
    {
        Task<Backup?> Get(int id);
        Task Add(Backup backup);
        Task Update(Backup backup);
        Task Delete(int id);
    }

}
