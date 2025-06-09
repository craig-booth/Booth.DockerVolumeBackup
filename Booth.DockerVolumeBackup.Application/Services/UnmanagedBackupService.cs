
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;


namespace Booth.DockerVolumeBackup.Application.Services
{
    internal class UnmanagedBackupService(IMountPointBackupService mountPointService): IUnmanagedBackupService
    {
        private int _NextBackupId = -1;
        private readonly Dictionary<string, Backup> _Backups = [];
        private readonly ReaderWriterLockSlim _Lock = new();

        public async Task<List<Backup>> GetBackupsAsync(CancellationToken cancellationToken)
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
                    var backupFiles = await mountPointService.GetBackupFilesAsync(Path.Combine("/backup", backupDirectory.Name));

                    if (backupFiles.Count > 0)
                    {
                        var newBackup = new Backup
                        {
                            BackupId = _NextBackupId--,
                            BackupDirectory = backupDirectory.Name,
                            StartTime = backupDirectory.CreationDate,
                            EndTime = backupDirectory.CreationDate,
                            Status = Status.Complete
                        };

                        var volumeBackups = backupFiles.Select(x => new BackupVolume() {
                            Volume = GetVolumeName(x.Name),
                            BackupFile = x.Name,
                            BackupId = newBackup.BackupId,
                            BackupSize = x.Size,
                            StartTime = x.CreationDate,
                            EndTime= x.CreationDate,
                            Status = Status.Complete                           
                        });
                        newBackup.Volumes.AddRange(volumeBackups);
                        backups.Add(newBackup);


                        // Check again in case another thread added it
                        _Lock.EnterWriteLock();
                        if (!_Backups.ContainsKey(backupDirectory.Name))
                        { 
                            _Backups.Add(backupDirectory.Name, newBackup);    
                        }
                        _Lock.ExitWriteLock();

                    cancellationToken.ThrowIfCancellationRequested();
                    }
                }
            }

            return backups;
        }

        private string GetVolumeName(string fileName)
        {
            var index = fileName.IndexOf('.');
            if (index > 0)
            {
                return fileName.Substring(0, index);
            }
            else
            {
                return fileName;
            }
        }
    }
}
