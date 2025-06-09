using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IUnmanagedBackupService
    {
        Task<List<Backup>> GetBackupsAsync(CancellationToken cancellationToken);
        Backup? GetBackup(int backupId);
    }
}
