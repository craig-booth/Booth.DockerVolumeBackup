using Dapper;

namespace Booth.DockerVolumeBackup.WebApi.DataProviders
{
    public static class SetupDatabase
    {
        public static async Task CreateDatabase(IDataContext context)
        {
            using (var connection = context.CreateConnection())
            {

                var sql = """
                    CREATE TABLE IF NOT EXISTS Backup (
                        BackupId        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Status          INTEGER NOT NULL,
                        ScheduledTime   TIMESTAMP,
                        StartTime       TIMESTAMP,
                        EndTime         TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS BackupVolume (
                        BackupVolumeId  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        BackupId        INTEGER NOT NULL,
                        Volume          TEXT,
                        Status          INTEGER NOT NULL,
                        StartTime       TIMESTAMP,
                        EndTime         TIMESTAMP
                    );
                    """;

                await connection.ExecuteAsync(sql);
            }
        }

    }
}
