using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Infrastructure.Database;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetNextBackupToRunQuery() : IRequest<int>;

    internal class GetNextBackupToRunQueryHandler(IDataContext dataContext) : IRequestHandler<GetNextBackupToRunQuery, int>
    {
        public async Task<int> Handle(GetNextBackupToRunQuery request, CancellationToken cancellationToken)
        {
            int backupId = 0;

            using (var connection = dataContext.CreateConnection())
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
    }
}
