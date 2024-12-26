using MediatR;
using Dapper;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetNextBackupToRunQuery() : IRequest<ErrorOr<int>>;

    internal class GetNextBackupToRunQueryHandler(IDataContext dataContext) : IRequestHandler<GetNextBackupToRunQuery, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(GetNextBackupToRunQuery request, CancellationToken cancellationToken)
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

            return backupId > 0 ? backupId : Error.NotFound();
        }
    }
}
