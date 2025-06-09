
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using System.Collections.Concurrent;

namespace Booth.DockerVolumeBackup.Application.Services
{
    internal class UnmanagedBackupService(IMountPointBackupService mountPointService): IUnmanagedBackupService
    {
        private static int _NextBackupId = -1;
        private static readonly Dictionary<string, Backup> _Backups = [];
        private static readonly ReaderWriterLockSlim _Lock = new();

        public async Task<List<Backup>> GetBackupsAsync()
        {
            var backups = new List<Backup>();

            var backupDirectories = await mountPointService.GetBackupDirectoriesAsync("/backup");

            foreach (var backupDirectory in backupDirectories) 
            {
                _Lock.EnterReadLock();
                var found = _Backups.TryGetValue(backupDirectory.Name, out var existingBackup);
                _Lock.ExitReadLock();

                if (found)
                {
                    if (existingBackup != null)
                        backups.Add(existingBackup);
                }
                else
                {
                    _Lock.EnterWriteLock();

                    // Check again in case another thread added it
                    if (!_Backups.ContainsKey(backupDirectory.Name))
                    {
                        var newBackup = new Backup
                        {
                            BackupId = _NextBackupId--,
                            BackupDirectory = backupDirectory.Name,
                            StartTime = backupDirectory.CreationDate,
                            EndTime = backupDirectory.CreationDate,
                            Status = Status.Complete
                        };
                        _Backups.Add(backupDirectory.Name, newBackup);
                        backups.Add(newBackup);
                    }
                    _Lock.ExitWriteLock();
                }
            }

            return backups;
        }
    }
}
