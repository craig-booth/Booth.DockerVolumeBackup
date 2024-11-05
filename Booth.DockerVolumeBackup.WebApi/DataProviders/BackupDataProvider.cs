using System.Globalization;
using Dapper;

using Booth.DockerVolumeBackup.WebApi.DataProviders.Models;

namespace Booth.DockerVolumeBackup.WebApi.DataProviders
{

    public interface IBackupDataProvider
    {
        Task<int> CreateBackupAsync(IEnumerable<string> volumes);
        Task UpdateBackupStatusAsync(int backupId, Status status);
        Task UpdateVolumeStatusAsync(int backupVolumeId, Status status);
        Task<Models.Backup?> GetBackupAsync(int backupId);
        Task<int> GetNextBackupIdAsync();
        Task<int> GetLastBackupDates(Dictionary<string, DateTimeOffset?> volumes);
    }

    public class BackupDataProvider : IBackupDataProvider
    {
        private readonly IDataContext _DataContext;

        public BackupDataProvider(IDataContext context)
        {
            _DataContext = context;
        }

        public async Task<int> CreateBackupAsync(IEnumerable<string> volumes)
        {
            var backupId = 0;

            using (var connection = _DataContext.CreateConnection())
            {
                var sql = """
                    INSERT INTO Backup (Status, ScheduledTime)
                        VALUES (0, CURRENT_TIMESTAMP) RETURNING RowId;
                """;
                backupId = await connection.ExecuteScalarAsync<int>(sql);

                sql = """
                        INSERT INTO BackupVolume(BackupId, Volume, Status)
                        VALUES(@BackupId, @Volume, 0);

                    """;

                await connection.ExecuteAsync(sql, volumes.Select(x => new { BackupId = backupId, Volume = x }));
            }

            return backupId;
        }

        public async Task<Models.Backup?> GetBackupAsync(int backupId)
        {
            Models.Backup? backup = null;

            using (var connection = _DataContext.CreateConnection())
            {
                var sql = """
                    SELECT BackupId, Status, ScheduledTime, StartTime, EndTime
                    FROM Backup
                    WHERE BackupId = @BackupId;

                    SELECT BackupVolumeId, Volume, Status, StartTime, EndTime
                    FROM BackupVolume
                    WHERE BackupId = @BackupId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { BackupId = backupId});

                backup = multi.ReadSingle<Models.Backup>();
                var backupVolumes = multi.Read<Models.BackupVolume>();

                backup.Volumes.AddRange(backupVolumes);
            }

            return backup;
        }

        public async Task<int> GetNextBackupIdAsync()
        {
            int backupId = 0;

            using (var connection = _DataContext.CreateConnection())
            {
                var sql = """
                    SELECT BackupId
                    FROM Backup
                    WHERE Backup.Status = 0 AND Backup.ScheduledTime <= CURRENT_TIMESTAMP
                    ORDER BY Backup.ScheduledTime ASC
                    LIMIT 1
                """;
                backupId = await connection.ExecuteScalarAsync<int>(sql);
            }

            return backupId; 
        }

        public async Task UpdateBackupStatusAsync(int backupId, Status status)
        {
            using (var connection = _DataContext.CreateConnection())
            {
                var sql = """
                    UPDATE Backup
                    SET Status = @Status 
                    WHERE BackupId = @BackupId
                """;
                await connection.ExecuteAsync(sql, new {BackupId = backupId, Status = status});
            }
        }

        public async Task UpdateVolumeStatusAsync(int backupVolumeId, Status status)
        {
            using (var connection = _DataContext.CreateConnection())
            {
                var sql = "UPDATE BackupVolume SET Status = @Status";

                if (status == Status.Active)
                    sql += ", StartTime = CURRENT_TIMESTAMP";
                else if (status == Status.Complete)
                    sql += ", EndTime = CURRENT_TIMESTAMP";

                sql += " WHERE BackupVolumeId = @BackupVolumeId";
                
                await connection.ExecuteAsync(sql, new { BackupVolumeId = backupVolumeId, Status = status });
            }
        }

        public async Task<int> GetLastBackupDates(Dictionary<string, DateTimeOffset?> volumes)
        {
            int volumesUpdated = 0;

            using (var connection = _DataContext.CreateConnection())
            {
                var sql = """
                    SELECT Volume, Max(EndTime) AS BackupTime
                    FROM BackupVolume
                    GROUP BY Volume, EndTime
                    HAVING Status = 2 AND EndTime IS NOT NULL
                """;
                var backupDates = (await connection.QueryAsync(sql)).AsList();
     
                foreach (var volumeName in volumes.Keys)
                {
                    var backupRecord = backupDates.Find(x => x.Volume == volumeName);
                    if (backupRecord != null)
                    {
                        volumes[volumeName] = DateTimeOffset.Parse(backupRecord.BackupTime, null, DateTimeStyles.AssumeUniversal);
                        volumesUpdated++;
                    }
                }
            }

            return volumesUpdated;
        }
    }
}
