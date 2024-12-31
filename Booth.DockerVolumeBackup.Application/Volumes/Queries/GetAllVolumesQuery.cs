using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Application.Volumes.Queries.GetAllVolumes
{
    public class VolumeDto
    {
        public required string Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? LastBackup { get; set; }
    }

    public record GetAllVolumesQuery : IRequest<ErrorOr<IReadOnlyList<VolumeDto>>>;

    internal class GetAllVolumesQueryHandler(IDockerService dockerService, IDataContext dataContext) : IRequestHandler<GetAllVolumesQuery, ErrorOr<IReadOnlyList<VolumeDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<VolumeDto>>> Handle(GetAllVolumesQuery request, CancellationToken cancellationToken)
        {
            var volumes = await dockerService.GetVolumesAsync();
            var queryResult = volumes.Select(x => new VolumeDto { Name = x.Name, Size = x.Size }).ToList();

            // Get the last backup date from the database
            var sql = """
                    SELECT Volume AS Name, 0 AS Size, Max(EndTime) AS LastBackup
                    FROM BackupVolume
                    GROUP BY Volume
                    HAVING Status = 2 AND EndTime IS NOT NULL
                """;
            var volumeLatestBackup = await dataContext.ExecuteSqlQueryAsync<VolumeDto>(sql, [])
                .ToListAsync();

            foreach (var volume in queryResult)
            {
                var backupRecord = volumeLatestBackup.Find(x => x.Name == volume.Name);
                if (backupRecord != null)
                {
                    volume.LastBackup = backupRecord.LastBackup;
                }
            }

            return queryResult;
        }

        
    }

}
