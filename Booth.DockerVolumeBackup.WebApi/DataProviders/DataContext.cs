using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Booth.DockerVolumeBackup.WebApi.DataProviders
{
    public interface IDataContext
    {
        IDbConnection CreateConnection();
    }

    public class DataContext : IDataContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;

            SqlMapper.AddTypeHandler<DateTimeOffset>(new DateTimeOffsetHandler());
        }

        public IDbConnection CreateConnection()
        {
            return new SqliteConnection(Configuration.GetConnectionString("WebApiDatabase"));
        }
    }
}
