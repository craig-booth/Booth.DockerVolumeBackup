using Booth.Docker;
using Booth.DockerVolumeBackup.WebApi.DataProviders;
using Booth.DockerVolumeBackup.WebApi.Models;

namespace Booth.DockerVolumeBackup.WebApi.Services
{
    public class BackupService
    {
        private readonly IBackupDataProvider _DataProvider;
        private readonly IDockerClient _DockerClient;

        public BackupService(IBackupDataProvider dataProvider, IDockerClient dockerClient)
        {
            _DataProvider = dataProvider;
            _DockerClient = dockerClient;
        }

        public async Task<int> BackupVolumesAsync(IEnumerable<string> volumes)
        {
            var backupId = await _DataProvider.CreateBackupAsync(volumes);

            return backupId;
        }

        public async Task<BackupStatus?> GetBackupStatusAsync(int backupId)
        {
            var backup = await _DataProvider.GetBackupAsync(backupId);
            if (backup == null) 
                return null;

            var backupStatus = new BackupStatus()
            {
                BackupId = backup.BackupId,
                Status = (Status)backup.Status,
            };
            backupStatus.Volumes.AddRange(backup.Volumes.Select(x => new VolumeBackupStatus() { VolumeName = x.Volume, Status = (Status)x.Status, BackupTime = x.EndTime }));

            return backupStatus;
        }
    }
}
