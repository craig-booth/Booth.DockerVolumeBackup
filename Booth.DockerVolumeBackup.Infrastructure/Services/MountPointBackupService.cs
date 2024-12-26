using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Infrastructure.Services
{
    internal class MountPointBackupService : IMountPointBackupService, IDisposable
    {

        private Process? _Process = null;     
        private readonly ILogger _Logger;

        public MountPointBackupService(ILogger<MountPointBackupService> logger) 
        {
            _Logger = logger;
        }

        public async Task CreateDirectoryAsync(string directoryName)
        {
            await RunShellCommand($"mkdir {directoryName}");
        }

        public async Task BackupDirectoryAsync(string directoryName, string destinationPath)
        {
            await RunShellCommand($"tar -czf {destinationPath} -C {directoryName} ./");
        }

        private async Task RunShellCommand(string command)
        {
            if (_Process == null)
            {
                _Process = new Process();
                _Process.StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/sh",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                _Process.Start();
            }

            var completedText = "--@!@Completed@!@--";

            _Logger.LogDebug($"Executing shell command '{command}'");
            await _Process.StandardInput.WriteLineAsync(command);
            await _Process.StandardInput.WriteLineAsync($"echo {completedText}");


            _Logger.LogDebug($"Command executed, waiting for completion");
            while (true)
            {
                var output = await _Process.StandardOutput.ReadLineAsync();
                if ((output != null) && (output == completedText))
                    break;
            }

            _Logger.LogDebug("Command completed");
        }

        public void Dispose()
        {
            if (_Process != null)
            {
                _Process.Kill();
                _Process.Dispose();
            }
        }
    }
}
