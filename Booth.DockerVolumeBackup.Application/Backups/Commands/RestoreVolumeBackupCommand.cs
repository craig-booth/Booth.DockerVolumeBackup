using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace Booth.DockerVolumeBackup.Application.Backups.Commands.RestoreVolumeBackup
{
    public record RestoreVolumeBackupCommand(int VolumeBackupId, string VolumeName) : IRequest<ErrorOr<bool>>;

    internal class RestoreVolumeBackupCommandHandler(IDataContext dataContext, IServiceScopeFactory serviceScopeFactory, IBackupScheduler scheduler) : IRequestHandler<RestoreVolumeBackupCommand, ErrorOr<bool>>
    {
        public async Task<ErrorOr<bool>> Handle(RestoreVolumeBackupCommand request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Include(x => x.Volumes)
                .Where(x => x.Volumes.Any(v => v.BackupVolumeId == request.VolumeBackupId));

            var backup = await query.FirstOrDefaultAsync(cancellationToken);
            if (backup == null)
                return Error.NotFound();

            var volume = backup.Volumes.FirstOrDefault(x => x.BackupVolumeId == request.VolumeBackupId);
            if (volume == null)
                return Error.NotFound();

            if (backup.BackupDirectory == null || volume.BackupFile == null)
                return Error.Failure("BackupNotCompleted", "Cannot restore, the backup was not completed successfully");

            // Check that the backup was successful
            if (backup.Status != Status.Complete)
                return Error.Failure("BackupNotCompleted", "Cannot restore, the backup was not completed successfully");

            var restoreJob = new RestoreJob(request.VolumeBackupId, request.VolumeName, serviceScopeFactory);
            scheduler.QueueJob(restoreJob);

            return true;
        }
    }
}
