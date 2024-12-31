using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;


namespace Booth.DockerVolumeBackup.Application.Schedules.Commands
{

    public record UpdateScheduleCommand : IRequest<ErrorOr<bool>>
    {
        public int ScheduleId { get; set; }
        public required string Name { get; set; }
        public bool Enabled { get; set; }
        public ScheduleDaysDto Days { get; set; } = new ScheduleDaysDto();
        public TimeOnly Time { get; set; }
        public List<string> Volumes { get; set; } = new List<string>();
    }

    public class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
    {
        public UpdateScheduleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Days).Must(x => x.Sunday || x.Monday || x.Tuesday || x.Wednesday || x.Thursday || x.Friday || x.Saturday).WithMessage("Atleast one day must be selected.");
            RuleFor(x => x.Volumes).NotEmpty().WithMessage("Atleast one volume must be selected.");
        }
    }

    internal class UpdateScheduleCommandHandler(IDataContext dataContext) : IRequestHandler<UpdateScheduleCommand, ErrorOr<bool>>
    {
        public async Task<ErrorOr<bool>> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
        {
            var schedule = await dataContext.Schedules
                .Where(x => x.ScheduleId == request.ScheduleId)
                .SingleOrDefaultAsync(cancellationToken);

            if (schedule == null)
                return Error.NotFound();

            schedule.Name = request.Name;
            schedule.Enabled = request.Enabled;
            schedule.Sunday = request.Days.Sunday;
            schedule.Monday = request.Days.Monday;
            schedule.Tuesday = request.Days.Tuesday;
            schedule.Wednesday = request.Days.Wednesday;
            schedule.Thursday = request.Days.Thursday;
            schedule.Friday = request.Days.Friday;
            schedule.Saturday = request.Days.Saturday;
            schedule.Time = request.Time;

            schedule.Volumes.RemoveAll(x => !request.Volumes.Contains(x.Volume));
            schedule.Volumes.AddRange(request.Volumes.Where(x => !schedule.Volumes.Any(v => v.Volume == x)).Select(x => new BackupScheduleVolume { ScheduleId = schedule.ScheduleId, Volume = x }));

            await dataContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
