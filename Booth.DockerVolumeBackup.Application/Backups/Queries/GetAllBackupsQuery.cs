using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Backups.Common;


namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetAllBackups
{
    public record GetAllBackupsQuery(int? ScheduleId = null) : IRequest<ErrorOr<IReadOnlyList<BackupDto>>>;

    public class BackupDto
    {
        public int BackupId { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
    }

    internal class GetAllBackupsQueryHandler(IDataContext dataContext) : IRequestHandler<GetAllBackupsQuery, ErrorOr<IReadOnlyList<BackupDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<BackupDto>>> Handle(GetAllBackupsQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups.AsNoTracking<Backup>();
            if (request.ScheduleId != null)
                query = query.Where(x => x.ScheduleId == request.ScheduleId);

            var queryDto = query
                .Select(x => new BackupDto()
                {
                    BackupId = x.BackupId,
                    ScheduleId = x.ScheduleId,
                    ScheduleName = (x.Schedule != null) ? x.Schedule.Name : "",
                    Status = (StatusDto)x.Status     
                });

            var backups = await queryDto.ToListAsync(cancellationToken);

            return backups;
        }
    }
}
