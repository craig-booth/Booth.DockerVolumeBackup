using MediatR;
using Dapper;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetBackupQuery(int BackupId) : IRequest<ErrorOr<BackupDto>>;

    internal class GetBackupQueryHandler(IDataContext dataContext) : IRequestHandler<GetBackupQuery, ErrorOr<BackupDto>>
    {
        public async Task<ErrorOr<BackupDto>> Handle(GetBackupQuery request, CancellationToken cancellationToken)
        {
            BackupDto? backup = null;
            
            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT b.BackupId, b.ScheduleId, bs.Name as 'ScheduleName', b.Status, b.StartTime, b.EndTime
                    FROM Backup b
                    JOIN BackupSchedule bs ON bs.ScheduleId = b.ScheduleId
                    WHERE BackupId = @BackupId;

                    SELECT BackupVolumeId, Volume, Status, StartTime, EndTime
                    FROM BackupVolume
                    WHERE BackupId = @BackupId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { BackupId = request.BackupId });

                backup = await multi.ReadSingleOrDefaultAsync<BackupDto>();
                var backupVolumes = await multi.ReadAsync<BackupVolumeDto>();

                if (backup != null)
                    backup.Volumes.AddRange(backupVolumes);
            }  

            return backup != null ? backup : Error.NotFound();
        }
    }
}
