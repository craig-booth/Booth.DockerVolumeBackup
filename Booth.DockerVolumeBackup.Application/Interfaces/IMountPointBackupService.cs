using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{

    public record BackupDirectoryInfo(string Name, DateTime CreationDate);    
    public record BackupFileInfo(string Name, long Size, DateTime CreationDate);

    public interface IMountPointBackupService
    {
        Task<bool> CreateDirectoryAsync(string directoryName);
        Task<bool> DeleteDirectoryAsync(string directoryName);
        Task<List<BackupDirectoryInfo>> GetBackupDirectoriesAsync(string path);
        Task<List<BackupFileInfo>> GetBackupFilesAsync(string path);
        Task<long> BackupDirectoryAsync(string directoryName, string backupFile);
        Task<bool> RestoreDirectoryAsync(string directoryName, string backupFile);
        Stream GetBackupFile(string filePath);
    }
}
