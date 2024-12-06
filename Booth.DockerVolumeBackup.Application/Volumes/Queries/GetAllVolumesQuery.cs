using System.Globalization;
using Dapper;
using MediatR;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Application.Volumes.Dtos;

namespace Booth.DockerVolumeBackup.Application.Volumes.Queries
{
    public record GetAllVolumesQuery : IRequest<IReadOnlyList<VolumeDto>>;

    internal class GetAllVolumesQueryHandler(IDockerClient dockerClient, IDataContext dataContext) : IRequestHandler<GetAllVolumesQuery, IReadOnlyList<VolumeDto>>
    {
        public async Task<IReadOnlyList<VolumeDto>> Handle(GetAllVolumesQuery request, CancellationToken cancellationToken)
        {
            var volumes = await dockerClient.Volumes.ListAsync();
            var queryResult = volumes.Select(x => new VolumeDto { Name = x.Name, Size = x.UsageData.Size }).ToList();

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
