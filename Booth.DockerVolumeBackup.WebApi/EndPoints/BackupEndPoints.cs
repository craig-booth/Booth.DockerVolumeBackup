using System.Text.Json;
using Microsoft.Extensions.Options;

using MediatR;

using Booth.DockerVolumeBackup.WebApi.Dtos;
using Booth.DockerVolumeBackup.Application.Backups.Queries;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using Booth.DockerVolumeBackup.Application.Backups.Commands;
using Booth.DockerVolumeBackup.Application.Services;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class BackupEndPoints
    {

        public static void AddBackupEndPoints(this WebApplication app)
        {

            app.MapGet("api/backups", async (IMediator mediator) =>
            {   
                var backups = await mediator.Send(new GetAllBackupsQuery());
                return Results.Ok(backups);
            })
            .WithName("GetAllBackups")
            .Produces<IReadOnlyList<BackupDto>>(StatusCodes.Status200OK);


            app.MapGet("api/backups/{id:int}", async (int id, IMediator mediator) =>
            {
                var backup = await mediator.Send(new GetBackupQuery(id));
                if (backup == null)
                    return Results.NotFound();

                return Results.Ok(backup);
            })
            .WithName("GetBackup")
            .Produces<BackupDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            app.MapGet("api/backups/{id:int}/status", async(int id, HttpContext context, CancellationToken cancellationToken, IMediator mediator, IBackupNotificationService notificationService, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> serializeOptions) =>
            {
                context.Response.Headers.Append("Content-Type", "text/event-stream");

                var backupStatusUpdates = mediator.CreateStream(new GetBackupStatusQuery(id), cancellationToken);
                await foreach (var backupStatus in backupStatusUpdates)
                {
                    await context.Response.WriteAsync($"data: ");
                    await JsonSerializer.SerializeAsync(context.Response.Body, backupStatus, serializeOptions.Value.SerializerOptions);
                    await context.Response.WriteAsync($"\n\n");
                    await context.Response.Body.FlushAsync();

                }
            });

            app.MapGet("api/backups/{id:int}/log", (int id) =>
            {
                throw new NotSupportedException();
            });

            app.MapPost("api/backups/{id:int}/run", (int id) =>
            {
                throw new NotSupportedException();
            });


            app.MapPost("api/backups/run", async (VolumeBackupRequestDto request, IMediator mediator) =>
            {
                var backupId = await mediator.Send(new RunAdhocBackupCommand(request.Volumes));

                return backupId;
            });



        }


    }
}
