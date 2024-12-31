using System.Data;
using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using Dapper;

using Booth.DockerVolumeBackup.Application;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Infrastructure.Database.DataTypeHandlers;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    public class DataContext : DbContext, IDataContext
    {
        private readonly IOptions<AppConfig> _AppConfig;

        public DbSet<Backup> Backups => Set<Backup>();
        public DbSet<BackupSchedule> Schedules => Set<BackupSchedule>();

      //  public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DataContext(IOptions<AppConfig> config)
        {
            _AppConfig = config;

            SqlMapper.AddTypeHandler<DateTimeOffset>(new DateTimeOffsetHandler());
            SqlMapper.AddTypeHandler<TimeOnly>(new TimeOnlyHandler());
            SqlMapper.AddTypeHandler<bool>(new BooleanHandler());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite(_AppConfig.Value.DatabaseConnectionString);
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
