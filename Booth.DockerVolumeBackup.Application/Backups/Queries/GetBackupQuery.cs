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
        public BackupTypeDto BackupType { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string BackupDirectory { get; set; } = string.Empty;
        public List<BackupVolumeDto> Volumes { get; set; } = new List<BackupVolumeDto>();
    }

    public class BackupVolumeDto
    {
        public int BackupVolumeId { get; set; }
        public string Volume { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string BackupFile { get; set; } = string.Empty;
        public long? BackupSize { get; set; }
    }

    internal class GetBackupQueryHandler(IDataContext dataContext, IUnmanagedBackupService unmanagedBackupService) : IRequestHandler<GetBackupQuery, ErrorOr<BackupDto>>
    {
        public async Task<ErrorOr<BackupDto>> Handle(GetBackupQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Where(x => x.BackupId == request.BackupId)
                .Select(x => new BackupDto()
                {
                    BackupId = x.BackupId,
                    BackupType = (x.Schedule != null) ? BackupTypeDto.Scheduled : BackupTypeDto.Adhoc,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = (x.Schedule != null) ? x.Schedule.Name : "",
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Status = (StatusDto)x.Status,
                    BackupDirectory = x.BackupDirectory ?? string.Empty,
                    Volumes = x.Volumes.Select(x => new BackupVolumeDto() {
                        BackupVolumeId = x.BackupVolumeId,
                        Volume = x.Volume,
                        Status = (StatusDto)x.Status,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        BackupFile = x.BackupFile ?? string.Empty,
                        BackupSize = x.BackupSize
                    }).ToList()
                });

            var backup = await query.FirstOrDefaultAsync(cancellationToken);

            // If backup is not found the check if it is an unmanaged backup
            if (backup == null)
            {
                var unmanagedbackup = unmanagedBackupService.GetBackup(request.BackupId);
                if (unmanagedbackup != null)
                {
                    backup = new BackupDto
                    {
                        BackupId = unmanagedbackup.BackupId,
                        BackupType = BackupTypeDto.Unmanaged,
                        ScheduleId = null,
                        ScheduleName = string.Empty,
                        StartTime = unmanagedbackup.StartTime,
                        EndTime = unmanagedbackup.EndTime,
                        Status = (StatusDto)unmanagedbackup.Status, 
                        BackupDirectory = unmanagedbackup.BackupDirectory ?? string.Empty,
                        Volumes = unmanagedbackup.Volumes.Select(x => new BackupVolumeDto
                        {
                            BackupVolumeId = x.BackupVolumeId,
                            Volume = x.Volume,
                            Status = (StatusDto)x.Status,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime,
                            BackupFile = x.BackupFile ?? string.Empty,
                            BackupSize = x.BackupSize
                        }).ToList()
                    };
                }

            }

            return backup != null ? backup : Error.NotFound();
        }
    }
}
