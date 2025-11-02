using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolume
{

    public class VolumeDto
    {
        public required string Name { get; set; }
        public long Size { get; set; }
    }

    public record GetVolumeQuery(string Name) : IRequest<ErrorOr<VolumeDto>>;

    internal class GetAllVolumesQueryHandler(IDockerService dockerService) : IRequestHandler<GetVolumeQuery, ErrorOr<VolumeDto>>
    {
        public async Task<ErrorOr<VolumeDto>> Handle(GetVolumeQuery request, CancellationToken cancellationToken)
        {
            var volumes = await dockerService.GetVolumesAsync();
            
            var volume = volumes.FirstOrDefault(x => x.Name == request.Name);

            if (volume == null) 
                return Error.NotFound();

            return new VolumeDto { Name = volume.Name, Size = volume.Size };
        }

        
    }

}
