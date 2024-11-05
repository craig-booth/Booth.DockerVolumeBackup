
using System.Text.Json;
using Microsoft.Extensions.Options;

using Booth.DockerVolumeBackup.WebApi.Models;
using Booth.DockerVolumeBackup.WebApi.Services;
using Booth.DockerVolumeBackup.WebApi.Backup;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class BackupEndPoints
    {

        public static void AddBackupEndPoints(this WebApplication app)
        {
            app.MapPost("api/volumes/backup", async (HttpContext context, BackupService backupService) =>
            {
                if (!context.Request.HasJsonContentType())
                {
                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    return -1;
                }

                var backupRequest = await context.Request.ReadFromJsonAsync<VolumeBackupRequest>();
                if (backupRequest == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return -1;
                }

                var backupId = await backupService.BackupVolumesAsync(backupRequest.Volumes);

                return backupId;
            });

            app.MapGet("api/backup/{id:int}", async (int id, HttpContext context, CancellationToken cancellationToken, BackupService backupService, IBackupNotificationService notificationService, IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> serializeOptions) =>
            {
                var backupUpdateEvent = new AutoResetEvent(false);
                var onBackupUpdate = (int backupId) => { backupUpdateEvent.Set(); };
                notificationService.SubscribeToUpdates(id, onBackupUpdate);

                context.Response.Headers.Append("Content-Type", "text/event-stream");

                while (!cancellationToken.IsCancellationRequested)
                {
                    backupUpdateEvent.WaitOne();

                    var status = await backupService.GetBackupStatusAsync(id);
                    if (status == null)
                        break;

                    await context.Response.WriteAsync($"data: ");
                    await JsonSerializer.SerializeAsync(context.Response.Body, status, serializeOptions.Value.SerializerOptions);
                    await context.Response.WriteAsync($"\n\n");
                    await context.Response.Body.FlushAsync();
                }

                notificationService.UnsubscribeToUpdates(id, onBackupUpdate);
            });

        }


    }
}
