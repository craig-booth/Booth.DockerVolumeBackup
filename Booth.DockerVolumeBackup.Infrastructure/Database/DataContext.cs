using System.Data;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using Booth.DockerVolumeBackup.Application;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DataContext : DbContext, IDataContext
    {
        private readonly IOptions<AppConfig> _AppConfig;

        public DbSet<Backup> Backups => Set<Backup>();
        public DbSet<BackupSchedule> Schedules => Set<BackupSchedule>();
        public DataContext(IOptions<AppConfig> config)
        {
            _AppConfig = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(_AppConfig.Value.DatabaseConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
        public IDbConnection CreateConnection()
        {
            this.Database.OpenConnection();
            return new SqliteConnection(_AppConfig.Value.DatabaseConnectionString);
        }

        public IQueryable<T> ExecuteSqlQueryAsync<T>(string sql, object[] parameters)
        {
            return this.Database.SqlQueryRaw<T>(sql, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, object[] parameters, CancellationToken cancellationToken)
        {
            return this.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

    }
}
