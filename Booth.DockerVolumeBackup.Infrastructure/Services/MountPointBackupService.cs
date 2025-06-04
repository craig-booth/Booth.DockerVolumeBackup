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
                .WithArguments(["-czf", destinationPath, "-C", directoryName, "./"])
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
    }
}
