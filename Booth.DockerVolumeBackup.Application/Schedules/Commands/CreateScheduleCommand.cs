using Microsoft.EntityFrameworkCore;

using MediatR;
using FluentValidation;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Application.Schedules.Commands
{

    public record CreateScheduleCommand : IRequest<ErrorOr<int>>
    {
        public required string Name { get; set; }
        public bool Enabled { get; set; }
        public ScheduleDaysDto Days { get; set; } = new ScheduleDaysDto();
        public TimeOnly Time { get; set; }
        public List<string> Volumes { get; set; } = new List<string>();
    }

    public class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
    {
        public CreateScheduleCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Days).Must(x => x.Sunday || x.Monday || x.Tuesday || x.Wednesday || x.Thursday || x.Friday || x.Saturday).WithMessage("Atleast one day must be selected.");
            RuleFor(x => x.Volumes).NotEmpty().WithMessage("Atleast one volume must be selected.");
        }
    }


    internal class CreateScheduleCommandHandler(IDataContext dataContext) : IRequestHandler<CreateScheduleCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
        {
            var schedule = new BackupSchedule()
            {
                Name = request.Name,
                Enabled = request.Enabled,
                Sunday = request.Days.Sunday,
                Monday = request.Days.Monday,
                Tuesday = request.Days.Tuesday,
                Wednesday = request.Days.Wednesday,
                Thursday = request.Days.Thursday,
                Friday = request.Days.Friday,
                Saturday = request.Days.Saturday,
                Time = request.Time
            };
            schedule.BackupDefinition.Volumes.AddRange(request.Volumes.Select(x => new BackupDefinitionVolume() { Volume = x }));

            dataContext.Schedules.Add(schedule);
            await dataContext.SaveChangesAsync(cancellationToken);

            return schedule.ScheduleId;

        }
    }
}
