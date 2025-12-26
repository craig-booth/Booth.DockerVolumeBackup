using MediatR;

using Booth.DockerVolumeBackup.WebApi.Extensions;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetVolumeBackupContents;
using Booth.DockerVolumeBackup.Application.Backups.Commands.RestoreVolumeBackup;
using Booth.DockerVolumeBackup.WebApi.Dtos;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class VolumeBackupEndPoints
    {

        public static void AddVolumeBackupEndPoints(this WebApplication app)
        {

            app.MapPost("api/volumebackups/{id:int}/restore", async (int id, VolumeRestoreRequestDto request, IMediator mediator) =>
            {
                var result = await mediator.Send(new RestoreVolumeBackupCommand(id, request.VolumeName));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("RestoreVolumeBackup");

            app.MapGet("api/volumebackups/{id:int}/download", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetVolumeBackupContentsQuery(id));
                return result.Match(
                    value => Results.File(value.Content, "application/octet-stream", value.FileName),
                    errors => ErrorResult.Error(errors));
            })
            .WithName("DownloadVolumeBackup");




        }


    }
}
