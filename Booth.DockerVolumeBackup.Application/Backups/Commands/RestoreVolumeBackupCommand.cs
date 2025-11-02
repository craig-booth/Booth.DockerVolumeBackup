using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Booth.DockerVolumeBackup.Application.Backups.Commands.RestoreVolumeBackup
{
    public record RestoreVolumeBackupCommand(int VolumeBackupId, string VolumeName) : IRequest<ErrorOr<bool>>;

    internal class RestoreVolumeBackupCommandHandler(IDataContext dataContext, IDockerService dockerService, IMountPointBackupService mountPointBackupService, ILogger<RestoreVolumeBackupCommandHandler> logger) : IRequestHandler<RestoreVolumeBackupCommand, ErrorOr<bool>>
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

            // If the volume does not exist, create it
            var volumes = await dockerService.GetVolumesAsync();
            var dockerVolume = volumes.FirstOrDefault(x => x.Name == request.VolumeName);
            if (dockerVolume == null)
            {
                dockerVolume = await dockerService.CreateVolumeAsync(request.VolumeName);
                if (dockerVolume == null)
                    return Error.Failure("VolumeNotCreated", "An error occurred created volume");
            }

            // Restore from backup
            logger.LogInformation("Restoring volume '{VolumeName}' ({MountPoint}) from backup file '{BackupFile}'", request.VolumeName, dockerVolume.MountPoint, volume.BackupFile);
            await mountPointBackupService.RestoreDirectoryAsync(dockerVolume.MountPoint, volume.BackupFile);

            return true;
        }
    }
}
