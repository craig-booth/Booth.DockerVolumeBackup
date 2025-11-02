using Booth.DockerVolumeBackup.Application.Interfaces;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace Booth.DockerVolumeBackup.Infrastructure.Services
{
    internal class MountPointBackupService : IMountPointBackupService
    {
        private readonly ILogger _Logger;

        public MountPointBackupService(ILogger<MountPointBackupService> logger)
        {
            _Logger = logger;
        }

        public Task<bool> CreateDirectoryAsync(string directoryName)
        {
            try
            {
                Directory.CreateDirectory(directoryName);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to create directory '{directoryName}'", directoryName);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public async Task<long> BackupDirectoryAsync(string directoryName, string destinationPath)
        {
            var result = await Cli.Wrap("tar")
                .WithArguments(["-czpf", destinationPath, "-C", directoryName, "./"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            if (result.IsSuccess)
            {
                var fileInfo = new FileInfo(destinationPath);
                return fileInfo.Length;
            }
            else
            {
                _Logger.LogError(result.StandardError);
                return 0;
            }
        }

        public Task<bool> DeleteDirectoryAsync(string directoryName)
        {
            try
            {
                Directory.Delete(directoryName, true);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to delete directory '{directoryName}'", directoryName);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<List<BackupDirectoryInfo>> GetBackupDirectoriesAsync(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo == null || !directoryInfo.Exists)
                return Task.FromResult(new List<BackupDirectoryInfo>());

            var backupDirectories = directoryInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                .Select(x => new BackupDirectoryInfo(x.Name, x.CreationTime))
                .ToList();

            return Task.FromResult(backupDirectories);
        }

        public Task<List<BackupFileInfo>> GetBackupFilesAsync(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (directoryInfo == null || !directoryInfo.Exists)
                return Task.FromResult(new List<BackupFileInfo>());

            var backupFiles = directoryInfo.EnumerateFiles("*.tar.gz", SearchOption.TopDirectoryOnly)
                .Select(x => new BackupFileInfo(x.Name, x.Length, x.CreationTime))
                .ToList();

            return Task.FromResult(backupFiles);
        }

        public Stream GetBackupFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo == null || !fileInfo.Exists)
                throw new FileNotFoundException("Backup file not found", filePath);

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public async Task<bool> RestoreDirectoryAsync(string directoryName, string backupFile)
        {
            var result = await Cli.Wrap("tar")
                .WithArguments(["-xpf", backupFile, "-C", directoryName])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return result.IsSuccess;
        }
    }
}
