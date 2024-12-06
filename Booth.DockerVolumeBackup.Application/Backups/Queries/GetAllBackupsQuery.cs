using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetAllBackupsQuery(int? ScheduleId = null) : IRequest<IReadOnlyList<BackupDto>>;

    internal class GetAllBackupsQueryHandler(IDataContext dataContext) : IRequestHandler<GetAllBackupsQuery, IReadOnlyList<BackupDto>>
    {
        public async Task<IReadOnlyList<BackupDto>> Handle(GetAllBackupsQuery request, CancellationToken cancellationToken)
        {
            var backups = new List<BackupDto>();

        /*    using (var connection = dataContext.CreateConnection())
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
            } */

            return backups;

        }
    }
}
