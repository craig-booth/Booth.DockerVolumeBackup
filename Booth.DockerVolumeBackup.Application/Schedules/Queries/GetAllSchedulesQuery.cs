using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using ErrorOr;
using Dapper;

using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Schedules.Queries
{
    public record GetAllSchedulesQuery() : IRequest<ErrorOr<IReadOnlyList<ScheduleDto>>>;

    internal class GetAllSchedulesQueryHandler(IDataContext dataContext) : IRequestHandler<GetAllSchedulesQuery, ErrorOr<IReadOnlyList<ScheduleDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<ScheduleDto>>> Handle(GetAllSchedulesQuery request, CancellationToken cancellationToken)
        {
            var scheduleDtos = new List<ScheduleDto>();

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT ScheduleId, Name, Enabled, Sunday, Monday, Tuesday, Wednesday, Friday, Saturday, Time
                    FROM BackupSchedule;

                    SELECT BackupScheduleVolumeId, ScheduleId, Volume
                    FROM BackupScheduleVolume;
                """;
                var multi = await connection.QueryMultipleAsync(sql);

                var schedules = await multi.ReadAsync<BackupSchedule>();
                var scheduleVolumes = await multi.ReadAsync<BackupScheduleVolume>();

                foreach (var schedule in schedules)
                {
                    var scheduleDto = ScheduleDto.FromDomain(schedule);
                    scheduleDto.Volumes.AddRange(scheduleVolumes.Where(x => x.ScheduleId == schedule.ScheduleId).Select(x => x.Volume));

                    scheduleDtos.Add(scheduleDto);
                }

            }

            return scheduleDtos;

        }

    }
}
