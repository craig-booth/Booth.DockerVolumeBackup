using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IMountPointBackupService
    {
        Task<bool> CreateDirectoryAsync(string directoryName);
        Task<long> BackupDirectoryAsync(string directoryName, string destinationPath);
    }
}
