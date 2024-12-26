using System.Globalization;
using Dapper;
using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Volumes.Dtos;

namespace Booth.DockerVolumeBackup.Application.Volumes.Queries
{
    public record GetAllVolumesQuery : IRequest<ErrorOr<IReadOnlyList<VolumeDto>>>;

    internal class GetAllVolumesQueryHandler(IDockerService dockerService, IDataContext dataContext) : IRequestHandler<GetAllVolumesQuery, ErrorOr<IReadOnlyList<VolumeDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<VolumeDto>>> Handle(GetAllVolumesQuery request, CancellationToken cancellationToken)
        {
            var volumes = await dockerService.GetVolumesAsync();
            var queryResult = volumes.Select(x => new VolumeDto { Name = x.Name, Size = x.Size }).ToList();

            // Get the last backup date from the database
            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT Volume, Max(EndTime) AS BackupTime
                    FROM BackupVolume
                    GROUP BY Volume
                    HAVING Status = 2 AND EndTime IS NOT NULL
                """;
                var backupDates = (await connection.QueryAsync(sql)).AsList();

                foreach (var volume in queryResult)
                {
                    var backupRecord = backupDates.Find(x => x.Volume == volume.Name);
                    if (backupRecord != null)
                    {
                        volume.LastBackup = DateTimeOffset.Parse(backupRecord.BackupTime, null, DateTimeStyles.AssumeUniversal);
                    }
                }
            }

            return queryResult;
        }
    }

}
