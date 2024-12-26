using MediatR;
using Dapper;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetAllBackupsQuery(int? ScheduleId = null) : IRequest<ErrorOr<IReadOnlyList<BackupDto>>>;

    internal class GetAllBackupsQueryHandler(IDataContext dataContext) : IRequestHandler<GetAllBackupsQuery, ErrorOr<IReadOnlyList<BackupDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<BackupDto>>> Handle(GetAllBackupsQuery request, CancellationToken cancellationToken)
        {
            var backups = new List<BackupDto>();

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT b.BackupId, b.ScheduleId, bs.Name as 'ScheduleName', b.Status, b.StartTime, b.EndTime,
                           bv.BackupVolumeId, bv.Volume, bv.Status, bv.StartTime, bv.EndTime
                    FROM Backup b 
                    JOIN BackupSchedule bs ON bs.ScheduleId = b.ScheduleId
                    JOIN BackupVolume bv ON bv.BackupId = b.BackupId
                """;
                if (request.ScheduleId.HasValue)
                {
                    sql += " WHERE b.ScheduleId = @ScheduleId";
                }
                var queryResult = await connection.QueryAsync<BackupDto, BackupVolumeDto, BackupDto>(sql, (backup, volume) =>
                {
                    backup.Volumes.Add(volume);
                    return backup;
                }, new { ScheduleId = request.ScheduleId }, splitOn: "BackupVolumeId");


                backups = queryResult.GroupBy(x => x.BackupId).Select(group =>
                    {
                        var backup = group.First();
                        backup.Volumes = group.Select(x => x.Volumes.Single()).ToList();
                        return backup;
                    }).ToList();
            } 

            return backups;
        }
    }
}
