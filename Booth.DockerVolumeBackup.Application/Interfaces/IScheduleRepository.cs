using Booth.DockerVolumeBackup.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IScheduleRepository
    {
        Task<BackupSchedule?> Get(int id);
        Task<int> Add(BackupSchedule schedule);
        Task Update(BackupSchedule schedule);
        Task Delete(int id);
    }
}
