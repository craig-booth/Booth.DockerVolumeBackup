using Dapper;
using MediatR;

using Booth.DockerVolumeBackup.Infrastructure.Database;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands
{
    public record RunAdhocBackupCommand(IEnumerable<string> Volumes) : IRequest<int>;

    internal class RunAdhocBackupCommandHandler(IDataContext dataContext) : IRequestHandler<RunAdhocBackupCommand, int>
    {
        public async Task<int> Handle(RunAdhocBackupCommand request, CancellationToken cancellationToken)
        {
            var backupId = 0;

            using (var connection = dataContext.CreateConnection())
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

                await connection.ExecuteAsync(sql, request.Volumes.Select(x => new { BackupId = backupId, Volume = x }));
            }

            return backupId;
        }
    }
}
