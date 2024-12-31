using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using NSubstitute;

using Booth.DockerVolumeBackup.Application;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Mocks
{
    internal class DataContextMock: IDataContext, IDisposable
    {
        private readonly IDataContext _Context;
        private readonly IDbConnection _Connection;

        public DbSet<Backup> Backups => _Context.Backups;
        public DbSet<BackupSchedule> Schedules => _Context.Schedules;

        public DataContextMock()
        {
            var appConfig = new AppConfig()
            {
                DatabaseConnectionString = "DataSource=\"file::memory:?cache=shared\"",
                SeedDatabase = true
            };
            var config = Substitute.For<IOptions<AppConfig>>();
            config.Value.Returns(appConfig);

            _Context = new DataContext(config);         

            // Create connection to keep in memory database open to prevent it being cleaned up
            _Connection = _Context.CreateConnection();
        }

        public IDbConnection CreateConnection()
        {
            return _Context.CreateConnection();
        }

        public void Dispose()
        {
            if (_Connection != null)
                _Connection.Dispose();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _Context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<T> ExecuteSqlQueryAsync<T>(string sql, object[] parameters)
        {
            return _Context.ExecuteSqlQueryAsync<T>(sql, parameters);   
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, object[] parameters, CancellationToken cancellationToken)
        {
            return _Context.ExecuteSqlCommandAsync(sql , parameters, cancellationToken);
        }
    }
}
