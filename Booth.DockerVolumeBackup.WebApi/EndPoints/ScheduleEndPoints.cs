
using MediatR;

using Booth.DockerVolumeBackup.Application.Schedules.Commands;
using Booth.DockerVolumeBackup.Application.Schedules.Queries.GetSchedule;
using Booth.DockerVolumeBackup.Application.Schedules.Queries.GetAllSchedules;
using Booth.DockerVolumeBackup.WebApi.Extensions;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class ScheduleEndPoints
    {
        public static void AddScheduleEndPoints(this WebApplication app)
        {

            app.MapGet("api/schedules", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllSchedulesQuery());

                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetAllSchedules");

            app.MapGet("api/schedules/{id:int}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetScheduleQuery(id));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetSchedule");


            app.MapPost("api/schedules", async (Application.Schedules.Dtos.ScheduleDto schedule, IMediator mediator) =>
            {
                var result = await mediator.Send(new CreateScheduleCommand()
                {
                    Name = schedule.Name,
                    Enabled = schedule.Enabled,
                    Days = schedule.Days,
                    Time = schedule.Time,
                    KeepLast = schedule.KeepLast,
                    Volumes = schedule.Volumes,
                });
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("CreateSchedule");


            app.MapPut("api/schedules/{id:int}", async (int id, Application.Schedules.Dtos.ScheduleDto schedule, IMediator mediator) =>
            {
                var result = await mediator.Send(new UpdateScheduleCommand()
                {
                    ScheduleId = id,
                    Name = schedule.Name,
                    Enabled = schedule.Enabled,
                    Days = schedule.Days,
                    Time = schedule.Time,
                    KeepLast = schedule.KeepLast,
                    Volumes = schedule.Volumes
                });
                return result.Match(x => Results.NoContent(), errors => ErrorResult.Error(errors));
            })
            .WithName("UpdateSchedule");

            app.MapDelete("api/schedules/{id:int}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteScheduleCommand(id));
                return result.Match(_ => Results.NoContent(), errors => ErrorResult.Error(errors));
            })
            .WithName("DeleteSchedule");
        }
    }
}
