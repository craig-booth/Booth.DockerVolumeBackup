using Microsoft.EntityFrameworkCore;

using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Interfaces
{
    public interface IDataContext
    {
        DbSet<Backup> Backups { get; }
        DbSet<BackupSchedule> Schedules { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<ITransaction> BeginTransactionAsync();
        Task<int> ExecuteSqlCommandAsync(string sql, object[] parameters, CancellationToken cancellationToken);
        IQueryable<T> ExecuteSqlQueryAsync<T>(string sql, object[] parameters);

    }

    public interface ITransaction : IDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken);
        Task RollbackAsync(CancellationToken cancellationToken);
    }

}
