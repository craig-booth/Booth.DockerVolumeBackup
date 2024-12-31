using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetNextBakupToRun
{
    public record GetNextBackupToRunQuery() : IRequest<ErrorOr<int>>;

    internal class GetNextBackupToRunQueryHandler(IDataContext dataContext) : IRequestHandler<GetNextBackupToRunQuery, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(GetNextBackupToRunQuery request, CancellationToken cancellationToken)
        {
            int backupId = 0;

     /*       dataContext.Backups.AsNoTracking()
                .Where(x => x.Status == Status.Queued && x.scj)

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
            } */

            return backupId > 0 ? backupId : Error.NotFound();
        }
    }
}
