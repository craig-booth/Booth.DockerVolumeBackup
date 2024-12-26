using System;

using MediatR;
using Dapper;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands
{
    public record RunScheduledBackupCommand(int ScheduleId) : IRequest<ErrorOr<int>>;

    internal class RunScheduledBackupCommandHandler(IDataContext dataContext) : IRequestHandler<RunScheduledBackupCommand, ErrorOr<int>>
    {
        public async  Task<ErrorOr<int>> Handle(RunScheduledBackupCommand request, CancellationToken cancellationToken)
        {
            var backupId = 0;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT Count(ScheduleId)
                    FROM BackupSchedule 
                    WHERE ScheduleId = @ScheduleId;
                """;
                var count = await connection.ExecuteScalarAsync<int>(sql, new { ScheduleId = request.ScheduleId });
                if (count == 0)
                    return Error.NotFound();

                sql = """
                    INSERT INTO Backup (ScheduleId, Status)
                        VALUES (@ScheduleId, @Status) RETURNING RowId;
                """;
                backupId = await connection.ExecuteScalarAsync<int>(sql, new { ScheduleId = request.ScheduleId, Status = Status.Queued });

                sql = """
                    INSERT INTO BackupVolume(BackupId, Volume, Status)
                        SELECT @BackupId, Volume, @Status 
                        FROM BackupScheduleVolume
                        WHERE ScheduleId = @ScheduleId

                    """;

                await connection.ExecuteAsync(sql, new { BackupId = backupId, ScheduleId = request.ScheduleId, Status = Status.Queued });
            }

            return backupId;
        }
    }
}
