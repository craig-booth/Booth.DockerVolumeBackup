using System;

using MediatR;
using ErrorOr;
using FluentValidation;
using Dapper;

using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;


namespace Booth.DockerVolumeBackup.Application.Schedules.Commands
{

    public record UpdateScheduleCommand : IRequest<ErrorOr<bool>>
    {
        public int Id { get; set; }
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
            
            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    UPDATE BackupSchedule
                        SET Enabled = @Enabled,
                            Name = @Name,
                            Sunday = @Sunday,
                            Monday = @Monday,
                            Tuesday = @Tuesday,
                            Wednesday = @Wednesday,
                            Thursday = @Thursday,
                            Friday = @Friday,
                            Saturday = @Saturday,
                            Time = @Time
                    WHERE ScheduleId = @ScheduleId;
                """;

                var schedule = new BackupSchedule()
                {
                    ScheduleId = request.Id,
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

                var recordsAffected = await connection.ExecuteAsync(sql, schedule);
                if (recordsAffected == 0)
                    return Error.NotFound();

                sql = """
                    DELETE FROM BackupScheduleVolume WHERE ScheduleId = @ScheduleId;
                """;
                await connection.ExecuteAsync(sql, new { ScheduleId = request.Id });

                sql = """
                    INSERT INTO BackupScheduleVolume (ScheduleId, Volume)
                    VALUES (@ScheduleId, @Volume) RETURNING RowId;
                """;
                foreach (var volume in request.Volumes)
                {
                    await connection.ExecuteAsync(sql, new { ScheduleId = request.Id, Volume = volume });
                }

                return true;
            }

        }
    }
}
