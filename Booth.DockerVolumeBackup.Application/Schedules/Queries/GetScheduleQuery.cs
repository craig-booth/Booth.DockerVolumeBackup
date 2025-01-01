using Microsoft.EntityFrameworkCore;

using ErrorOr;
using MediatR;

using Booth.DockerVolumeBackup.Application.Interfaces;



namespace Booth.DockerVolumeBackup.Application.Schedules.Queries.GetSchedule
{
    public record GetScheduleQuery(int ScheduleId) : IRequest<ErrorOr<ScheduleDto>>;


    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
        public ScheduleDaysDto Days { get; set; } = new ScheduleDaysDto();
        public TimeOnly Time { get; set; }
        public List<string> Volumes { get; set; } = [];
    }

    public class ScheduleDaysDto
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }

    internal class GetSchedulQueryHandler(IDataContext dataContext) : IRequestHandler<GetScheduleQuery, ErrorOr<ScheduleDto>>
    {
        public async Task<ErrorOr<ScheduleDto>> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Schedules
                .Where(x => x.ScheduleId == request.ScheduleId)
                .Select(x => new ScheduleDto
                {
                    ScheduleId = x.ScheduleId,
                    Name = x.Name,
                    Enabled = x.Enabled,
                    Days = new ScheduleDaysDto
                    {
                        Sunday = x.Sunday,
                        Monday = x.Monday,
                        Tuesday = x.Tuesday,
                        Wednesday = x.Wednesday,
                        Thursday = x.Thursday,
                        Friday = x.Friday,
                        Saturday = x.Saturday
                    },
                    Time = x.Time,
                    Volumes = x.Volumes.Select(x => x.Volume).ToList()
                });


            var scheduleDto = await query.SingleOrDefaultAsync();
            return scheduleDto != null? scheduleDto: Error.NotFound();
        }
    }
}