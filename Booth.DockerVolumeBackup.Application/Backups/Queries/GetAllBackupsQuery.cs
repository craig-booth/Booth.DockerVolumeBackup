using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Backups.Common;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.Contracts;


namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetAllBackups
{
    public record GetAllBackupsQuery(int? ScheduleId = null) : IRequest<ErrorOr<IReadOnlyList<BackupDto>>>;

    public class BackupDto
    {
        public int BackupId { get; set; }
        public BackupTypeDto BackupType { get; set; }
        public int? ScheduleId { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public StatusDto Status { get; set; }
        public DateTimeOffset? BackupTime { get; set;}
    }

    internal class GetAllBackupsQueryHandler(IDataContext dataContext, IUnmanagedBackupService unmanagedBackupService) : IRequestHandler<GetAllBackupsQuery, ErrorOr<IReadOnlyList<BackupDto>>>
    {
        public async Task<ErrorOr<IReadOnlyList<BackupDto>>> Handle(GetAllBackupsQuery request, CancellationToken cancellationToken)
        {
            var backups = new List<BackupDto>();

            // Get managed backups from the data context
            var query = dataContext.Backups.AsQueryable();

            if (request.ScheduleId != null)
            {
                query = query.Where(x => x.ScheduleId == request.ScheduleId)
                    .Include(x => x.Schedule);
            }
            else
            {
                query = query.Include(x => x.Schedule);
            }

            var managedBackups = await query.ToListAsync(cancellationToken);
            foreach (var managedBackup in managedBackups)
            {
                backups.Add(new BackupDto
                {
                    BackupId = managedBackup.BackupId,
                    BackupType = (managedBackup.Schedule != null) ? BackupTypeDto.Scheduled : BackupTypeDto.Adhoc,
                    ScheduleId = managedBackup.ScheduleId,
                    ScheduleName = (managedBackup.Schedule != null) ? managedBackup.Schedule.Name : string.Empty,
                    Status = (StatusDto)managedBackup.Status,
                    BackupTime = managedBackup.StartTime
                });
            }

            // Get unmanaged backups
            var unmanagedBackups = await unmanagedBackupService.GetBackupsAsync(cancellationToken);
            foreach (var unmanagedBackup in unmanagedBackups)
            {
                // Only include unmanaged backups that do not have the same name as a managed backup
                if (!managedBackups.Any(x => x.BackupDirectory == unmanagedBackup.BackupDirectory))
                {
                    backups.Add(new BackupDto
                    {
                        BackupId = unmanagedBackup.BackupId,
                        BackupType = BackupTypeDto.Unmanaged,
                        ScheduleId = null,
                        ScheduleName = string.Empty,
                        Status = StatusDto.Complete,
                        BackupTime = unmanagedBackup.StartTime
                    });
                }
            }

            return backups;
        }
    }
}
