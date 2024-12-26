using MediatR;
using ErrorOr;


using Booth.DockerVolumeBackup.Application.Volumes.Dtos;
using Booth.DockerVolumeBackup.Application.Volumes.Queries;
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
        }


    }
}
