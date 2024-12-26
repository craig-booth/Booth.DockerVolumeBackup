using Dapper;
using ErrorOr;
using MediatR;

using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Schedules.Queries
{
    public record GetScheduleQuery(int id) : IRequest<ErrorOr<ScheduleDto>>;

    internal class GetSchedulQueryHandler(IDataContext dataContext) : IRequestHandler<GetScheduleQuery, ErrorOr<ScheduleDto>>
    {
        public async Task<ErrorOr<ScheduleDto>> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
        {
            ScheduleDto? scheduleDto = null;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT ScheduleId, Name, Enabled, Sunday, Monday, Tuesday, Wednesday, Friday, Saturday, Time
                    FROM BackupSchedule
                    WHERE ScheduleId = @ScheduleId;

                    SELECT Volume
                    FROM BackupScheduleVolume
                    WHERE ScheduleId = @ScheduleId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { ScheduleId = request.id });

                var schedule = await multi.ReadSingleOrDefaultAsync<BackupSchedule>();
                var scheduleVolumes = await multi.ReadAsync<string>();

                if (schedule != null)
                {
                    scheduleDto = ScheduleDto.FromDomain(schedule);
                    scheduleDto.Volumes.AddRange(scheduleVolumes); 
                }
                    
            }

            return scheduleDto != null? scheduleDto: Error.NotFound();
        }
    }
}