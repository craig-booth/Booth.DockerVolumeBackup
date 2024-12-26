
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Infrastructure.Repositories
{
    internal class BackupRepository : IBackupRepository
    {
        public Task Add(Backup backup)
        {
            throw new NotImplementedException();
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Backup> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(Backup backup)
        {
            throw new NotImplementedException();
        }
    }
}
