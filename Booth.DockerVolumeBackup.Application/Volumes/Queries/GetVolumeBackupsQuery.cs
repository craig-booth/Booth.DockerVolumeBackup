using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolumeBackups
{

    public class VolumeBackupDto
    {
        public string VolumeName { get; set; } = string.Empty;
        public int BackupId { get; set; }
        public int BackupVolumeId { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? BackupTime { get; set; }
        public long? Size { get; set; }
    }

    public record GetVolumeBackupsQuery(string VolumeName) : IRequest<ErrorOr<IReadOnlyList<VolumeBackupDto>>>;

    internal class GetVolumeBackupsQueryHandler(IDataContext dataContext) : IRequestHandler<GetVolumeBackupsQuery, ErrorOr<IReadOnlyList<VolumeBackupDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<VolumeBackupDto>>> Handle(GetVolumeBackupsQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Where(x => x.Volumes.Any(v => v.Volume == request.VolumeName))
                .Select(x => new VolumeBackupDto()
                {
                    VolumeName = request.VolumeName,
                    BackupId = x.BackupId,
                    BackupVolumeId = x.Volumes.First(v => v.Volume == request.VolumeName).BackupVolumeId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = (x.Schedule != null) ? x.Schedule.Name : "",
                    Status = (StatusDto)x.Volumes.First(v => v.Volume == request.VolumeName).Status,
                    BackupTime = x.Volumes.First(v => v.Volume == request.VolumeName).EndTime,
                    Size = x.Volumes.First(v => v.Volume == request.VolumeName).BackupSize
                });

            var backups = await query.ToListAsync(cancellationToken);

            return backups;
        }
    }
}
