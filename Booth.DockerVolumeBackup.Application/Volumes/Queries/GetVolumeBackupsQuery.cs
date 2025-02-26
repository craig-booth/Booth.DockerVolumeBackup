using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetAllVolumes;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolumeBackups
{

    public class VolumeBackupDto
    {
        public int BackupId { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? BackupTime { get; set; }
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
                    BackupId = x.BackupId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = (x.Schedule != null) ? x.Schedule.Name : "",
                    Status = (StatusDto)x.Volumes.First(v => v.Volume == request.VolumeName).Status,
                    BackupTime = x.Volumes.First(v => v.Volume == request.VolumeName).EndTime,
                });

            var backups = await query.ToListAsync(cancellationToken);

            return backups;
        }
    }
}
