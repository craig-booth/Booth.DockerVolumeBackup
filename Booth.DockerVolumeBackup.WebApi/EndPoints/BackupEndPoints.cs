using System.Text.Json;
using Microsoft.Extensions.Options;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.WebApi.Dtos;
using Booth.DockerVolumeBackup.Application.Backups.Queries;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using Booth.DockerVolumeBackup.Application.Backups.Commands;
using Booth.DockerVolumeBackup.Application.Services;
using Booth.DockerVolumeBackup.Application.Schedules.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Booth.DockerVolumeBackup.WebApi.Extensions;
using System.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;



namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class BackupEndPoints
    {

        public static void AddBackupEndPoints(this WebApplication app)
        {

            app.MapGet("api/backups", async (int? scheduleId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllBackupsQuery(scheduleId));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetAllBackups");

            app.MapGet("api/backups/{id:int}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetBackupQuery(id));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetBackup");

            app.MapGet("api/backups/{id:int}/status", async (int id, HttpContext context, CancellationToken cancellationToken, IMediator mediator, IBackupNotificationService notificationService, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> serializeOptions) =>
            {
                var result = await mediator.Send(new GetBackupStatusQuery(id));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            });

            app.MapGet("api/backups/{id:int}/statusevents", async(int id, HttpContext context, CancellationToken cancellationToken, IMediator mediator, IBackupNotificationService notificationService, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> serializeOptions) =>
            {
                var result = await mediator.Send(new GetBackupStatusQuery(id));
                if (result.IsError)
                {
                    if (result.Errors[0].Type == ErrorType.NotFound)
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                    else
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
                else
                {
                    context.Response.Headers.Append("Content-Type", "text/event-stream");
                    var backupStatusUpdates = mediator.CreateStream(new GetBackupStatusEventsQuery(id), cancellationToken);
                    await foreach (var backupStatus in backupStatusUpdates)
                    {
                        await context.Response.WriteAsync($"data: ");
                        await JsonSerializer.SerializeAsync(context.Response.Body, backupStatus, serializeOptions.Value.SerializerOptions);
                        await context.Response.WriteAsync($"\n\n");
                        await context.Response.Body.FlushAsync();

                    }
                }
            });

            app.MapGet("api/backups/{id:int}/log", (int id) =>
            {
                throw new NotSupportedException();
            });

            app.MapPost("api/backups/{id:int}/run", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new RunScheduledBackupCommand(id));

                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            });


            app.MapPost("api/backups/run", async (VolumeBackupRequestDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new RunAdhocBackupCommand(request.Volumes));

                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            });



        }


    }
}
