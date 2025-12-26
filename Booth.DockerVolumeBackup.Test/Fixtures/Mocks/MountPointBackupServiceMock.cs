using Booth.DockerVolumeBackup.Application.Interfaces;
using System.Text;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class MountPointBackupServiceMock : IMountPointBackupService
    {
        public Task<long> BackupDirectoryAsync(string directoryName, string destinationPath)
        {
            return Task.FromResult(1000L);
        }

        public Task<bool> CreateDirectoryAsync(string directoryName)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteDirectoryAsync(string directoryName)
        {
            return Task.FromResult(true);
        }

        public Task<List<BackupDirectoryInfo>> GetBackupDirectoriesAsync(string path)
        {
            return Task.FromResult(new List<BackupDirectoryInfo>());
        }

        public Task<List<BackupFileInfo>> GetBackupFilesAsync(string path)
        {
            return Task.FromResult(new List<BackupFileInfo>());
        }

        public Stream GetBackupFile(string filePath)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("Test Data"));
        }

        public Task<bool> RestoreDirectoryAsync(string directoryName, string backupFile)
        {
            throw new NotImplementedException();
        }
    }
}
