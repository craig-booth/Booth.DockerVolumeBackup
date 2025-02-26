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
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<BackupVolumeStatusDto> Volumes { get; set; } = [];
    }

    public class BackupVolumeStatusDto
    {
        public int BackupVolumeId { get; set; }
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
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
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Volumes = x.Volumes.Select(v => new BackupVolumeStatusDto()
                    {
                        BackupVolumeId = v.BackupVolumeId,
                        Status = (StatusDto)v.Status,
                        StartTime = v.StartTime,
                        EndTime = v.EndTime
                    }).ToList()
                });

            var backupStatus = await query.FirstOrDefaultAsync(cancellationToken);

            return backupStatus != null ? backupStatus : Error.NotFound();
        }
    }
}
