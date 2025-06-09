﻿using Booth.DockerVolumeBackup.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
