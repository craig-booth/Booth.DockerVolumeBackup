
using Dapper;
using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;


namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetBackupStatusQuery(int BackupId) : IRequest<ErrorOr<BackupStatusDto>>;

    internal class GetBackupStatusQueryHandler(IDataContext dataContext) : IRequestHandler<GetBackupStatusQuery, ErrorOr<BackupStatusDto>>
    {

        public async Task<ErrorOr<BackupStatusDto>> Handle(GetBackupStatusQuery request, CancellationToken cancellationToken)
        {
            BackupStatusDto? backupStatus = null;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT BackupId, Status
                    FROM Backup
                    WHERE BackupId = @BackupId;

                    SELECT Volume, Status, EndTime
                    FROM BackupVolume
                    WHERE BackupId = @BackupId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { BackupId = request.BackupId });

                backupStatus = await multi.ReadSingleOrDefaultAsync<BackupStatusDto>();
                var backupVolumes = await multi.ReadAsync<VolumeBackupStatusDto>();

                if (backupStatus != null)
                    backupStatus.Volumes.AddRange(backupVolumes);
            }

            return backupStatus != null ? backupStatus : Error.NotFound();
        }
    }
}
