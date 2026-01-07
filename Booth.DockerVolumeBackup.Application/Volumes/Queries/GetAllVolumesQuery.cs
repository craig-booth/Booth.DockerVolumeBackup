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
        public bool Active { get; set; }
    }

    public record GetAllVolumesQuery : IRequest<ErrorOr<IReadOnlyList<VolumeDto>>>;

    internal class GetAllVolumesQueryHandler(IDockerService dockerService, IDataContext dataContext) : IRequestHandler<GetAllVolumesQuery, ErrorOr<IReadOnlyList<VolumeDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<VolumeDto>>> Handle(GetAllVolumesQuery request, CancellationToken cancellationToken)
        {
            var volumes = await dockerService.GetVolumesAsync();

            // Exclude volumes used by this container
            var dependentVolumes = await dockerService.GetDependentVolumesAsync();
            volumes.RemoveAll(x => dependentVolumes.Contains(x.Name));

            var queryResult = volumes.Select(x => new VolumeDto { Name = x.Name, Size = x.Size, Active = true }).ToList();

            // Get the last backup date from the database
            var sql = """
                    SELECT Volume AS Name, 0 AS Size, Max(EndTime) AS LastBackup, 0 AS Active
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

            // Add volumes that are backed up but not on the server
            var missingVolumes = volumeLatestBackup.Where(x => !volumes.Any(y => y.Name == x.Name));
            queryResult.AddRange(missingVolumes.Select(x => new VolumeDto { Name = x.Name, Size = x.Size, LastBackup = x.LastBackup, Active = false }));

            return queryResult;
        }


    }

}
