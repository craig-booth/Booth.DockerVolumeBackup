using System;
using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands.RunScheduledBackup
{
    public record RunScheduledBackupCommand(int ScheduleId) : IRequest<ErrorOr<int>>;

    internal class RunScheduledBackupCommandHandler(IDataContext dataContext) : IRequestHandler<RunScheduledBackupCommand, ErrorOr<int>>
    {
        public async  Task<ErrorOr<int>> Handle(RunScheduledBackupCommand request, CancellationToken cancellationToken)
        {
            var schedule = await dataContext.Schedules.AsNoTracking()
                .Where(x => x.ScheduleId == request.ScheduleId)
                .Include(x => x.Volumes)
                .SingleOrDefaultAsync();
            if (schedule == null)
                return Error.NotFound();


            var backup = new Backup
            {
                ScheduleId = request.ScheduleId,
                Status = Status.Queued
            };
            backup.Volumes.AddRange(schedule.Volumes.Select(x => new BackupVolume { Volume = x.Volume, Status = Status.Queued }));

            dataContext.Backups.Add(backup);

            await dataContext.SaveChangesAsync(cancellationToken);

            return backup.BackupId;
        }
    }
}
