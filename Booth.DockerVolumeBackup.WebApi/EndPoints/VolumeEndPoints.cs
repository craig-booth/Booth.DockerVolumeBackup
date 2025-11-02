using MediatR;
using ErrorOr;


using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolume;
using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetAllVolumes;
using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolumeBackups;
using Booth.DockerVolumeBackup.WebApi.Extensions;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class VolumeEndPoints
    {

        public static void AddVolumeEndPoints(this WebApplication app) 
        {
            app.MapGet("api/volumes", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllVolumesQuery());
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetAllVolumes");

            app.MapGet("api/volumes/{name}", async (string name, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetVolumeQuery(name));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));
            })
            .WithName("GetVolume");

            app.MapGet("api/volumes/{name}/backups", async (string name, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetVolumeBackupsQuery(name));
                return result.Match(value => Results.Ok(value), errors => ErrorResult.Error(errors));

            })
            .WithName("GetVolumeBackups");
        }

    }
}
