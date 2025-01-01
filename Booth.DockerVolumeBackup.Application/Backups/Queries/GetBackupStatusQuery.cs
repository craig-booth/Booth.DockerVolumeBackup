using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Backups.Common;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackupStatus
{
    public record GetBackupStatusQuery(int BackupId) : IRequest<ErrorOr<BackupStatusDto>>;

    public class BackupStatusDto
    {
        public int BackupId { get; set; }
        public StatusDto Status { get; set; }
        public List<BackupVolumeStatusDto> Volumes { get; set; } = [];
    }

    public class BackupVolumeStatusDto
    {
        public string Volume { get; set; } = "";
        public StatusDto Status { get; set; }
        public DateTimeOffset? BackupTime { get; set; }
    }


    internal class GetBackupStatusQueryHandler(IDataContext dataContext) : IRequestHandler<GetBackupStatusQuery, ErrorOr<BackupStatusDto>>
    {

        public async Task<ErrorOr<BackupStatusDto>> Handle(GetBackupStatusQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Where(x => x.BackupId == request.BackupId)
                .Select(x => new BackupStatusDto()
                {
                    BackupId = x.BackupId,
                    Status = (StatusDto)x.Status,
                    Volumes = x.Volumes.Select(x => new BackupVolumeStatusDto()
                    {
                        Volume = x.Volume,
                        Status = (StatusDto)x.Status,
                        BackupTime = x.EndTime
                    }).ToList()
                });

            var backupStatus = await query.FirstOrDefaultAsync(cancellationToken);

            return backupStatus != null ? backupStatus : Error.NotFound();
        }
    }
}
