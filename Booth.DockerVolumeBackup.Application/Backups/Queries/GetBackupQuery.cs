using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using Booth.DockerVolumeBackup.Infrastructure.Database;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetBackupQuery(int BackupId) : IRequest<BackupDto?>;

    internal class GetBackupQueryHandler(IDataContext dataContext) : IRequestHandler<GetBackupQuery, BackupDto?>
    {
        public async Task<BackupDto?> Handle(GetBackupQuery request, CancellationToken cancellationToken)
        {
            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT BackupId, ScheduleId, '', Status, ScheduledTime, StartTime, EndTime
                    FROM Backup
                    WHERE BackupId = @BackupId;

                    SELECT BackupVolumeId, Volume, Status, StartTime, EndTime
                    FROM BackupVolume
                    WHERE BackupId = @BackupId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { BackupId = request.BackupId });

                var backup = await multi.ReadSingleAsync<BackupDto>();
                if (backup != null)
                {
                    var backupVolumes = await multi.ReadAsync<BackupVolumeDto>();
                    backup.Volumes.AddRange(backupVolumes);
                }

                return backup;
            }
        }
    }
}
