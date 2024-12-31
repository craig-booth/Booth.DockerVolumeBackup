using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Application.Schedules.Queries.GetAllSchedules
{
    public record GetAllSchedulesQuery() : IRequest<ErrorOr<IReadOnlyList<ScheduleDto>>>;

    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
    }

    internal class GetAllSchedulesQueryHandler(IDataContext dataContext) : IRequestHandler<GetAllSchedulesQuery, ErrorOr<IReadOnlyList<ScheduleDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<ScheduleDto>>> Handle(GetAllSchedulesQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Schedules.AsNoTracking()
                .Select(x => new ScheduleDto
                {
                    ScheduleId = x.ScheduleId,
                    Name = x.Name,
                    Enabled = x.Enabled
                });

            var schedules = await query.ToListAsync();        

            return schedules;

        }

    }
}
