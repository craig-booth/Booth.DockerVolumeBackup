using MediatR;
using ErrorOr;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
using Booth.DockerVolumeBackup.Application.Services;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands.RunScheduledBackup
{
    public record RunScheduledBackupCommand(int ScheduleId) : IRequest<ErrorOr<int>>;

    internal class RunScheduledBackupCommandHandler(IScheduleUtils scheduleUtils, IServiceScopeFactory serviceScopeFactory, IBackupScheduler scheduler) : IRequestHandler<RunScheduledBackupCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(RunScheduledBackupCommand request, CancellationToken cancellationToken)
        {
            var backup = await scheduleUtils.CreateBackupFromScheduleAsync(request.ScheduleId, cancellationToken);
            if (backup == null)
            {
                return Error.NotFound();
            }

            var backupJob = new BackupJob(backup.BackupId, serviceScopeFactory);

            scheduler.QueueJob(backupJob);

            return backupJob.Id;
        }
    }
}
