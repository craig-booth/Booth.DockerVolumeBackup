using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using System.Collections;

using Dapper;

using Booth.DockerVolumeBackup.Application;
using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    public class DataContext : IDataContext
    {
        private readonly IOptions<AppConfig> _AppConfig;
        public DataContext(IOptions<AppConfig> config)
        {
            _AppConfig = config;

            SqlMapper.AddTypeHandler<DateTimeOffset>(new DateTimeOffsetHandler());
            SqlMapper.AddTypeHandler<TimeOnly>(new TimeOnlyHandler());
            SqlMapper.AddTypeHandler<bool>(new BooleanHandler());
        }

        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(_AppConfig.Value.DatabaseConnectionString);
        }
    }
}
