using MediatR;
using Booth.DockerVolumeBackup.Application.Volumes.Dtos;
using Booth.DockerVolumeBackup.Application.Volumes.Queries;


namespace Booth.DockerVolumeBackup.WebApi.EndPoints
{
    public static class VolumeEndPoints
    {

        public static void AddVolumeEndPoints(this WebApplication app) 
        {
            app.MapGet("api/volumes", async (IMediator mediator) =>
            {
                var volumes = await mediator.Send(new GetAllVolumesQuery());

                return volumes;
            })
            .WithName("GetAllVolumes")
            .Produces<IReadOnlyList<VolumeDto>>(StatusCodes.Status200OK);
        }


    }
}
