using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackup
{
    public record GetBackupQuery(int BackupId) : IRequest<ErrorOr<BackupDto>>;

    public class BackupDto
    {
        public int BackupId { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<BackupVolumeDto> Volumes { get; set; } = new List<BackupVolumeDto>();
    }

    public class BackupVolumeDto
    {
        public int BackupVolumeId { get; set; }
        public string Volume { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }

    internal class GetBackupQueryHandler(IDataContext dataContext) : IRequestHandler<GetBackupQuery, ErrorOr<BackupDto>>
    {
        public async Task<ErrorOr<BackupDto>> Handle(GetBackupQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .AsNoTracking()
                .Where(x => x.BackupId == request.BackupId)
                .Select(x => new BackupDto()
                {
                    BackupId = x.BackupId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = (x.Schedule != null) ? x.Schedule.Name : "",
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Status = (StatusDto)x.Status,
                    Volumes = x.Volumes.Select(x => new BackupVolumeDto() {
                        BackupVolumeId = x.BackupVolumeId,
                        Volume = x.Volume,
                        Status = (StatusDto)x.Status,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime
                    }).ToList()
                });

            var backup = await query.FirstOrDefaultAsync(cancellationToken);

            return backup != null ? backup : Error.NotFound();
        }
    }
}
