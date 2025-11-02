using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetVolumeBackupContents
{
    public record GetVolumeBackupContentsQuery(int VolumeBackupId) : IRequest<ErrorOr<BackupFile>>;

    public record BackupFile (Stream Content, string FileName);

    internal class GetVolumeBackupContentsQueryHandler(IDataContext dataContext, IMountPointBackupService mountPointBackupService) : IRequestHandler<GetVolumeBackupContentsQuery, ErrorOr<BackupFile>>
    {
        public async Task<ErrorOr<BackupFile>> Handle(GetVolumeBackupContentsQuery request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Include(x => x.Volumes)
                .Where(x => x.Volumes.Any(v => v.BackupVolumeId == request.VolumeBackupId));
                
            var backup = await query.FirstOrDefaultAsync(cancellationToken);
            if (backup == null)
                return Error.NotFound();

            var backupVolume = backup.Volumes.FirstOrDefault(v => v.BackupVolumeId == request.VolumeBackupId);
            if (backupVolume == null)
                return Error.NotFound();

            // Check that the backup was successful
            if (backupVolume.Status != Status.Complete)
                return Error.Failure("BackupNotCompleted", "The backup was not completed successfully");

            if (backup.BackupDirectory == null || backupVolume.BackupFile == null)
                return Error.Failure("NoBackupFile", "The backup does not have a backup file");

            var fileName = Path.Combine(backup.BackupDirectory, backupVolume.BackupFile);
            var stream = mountPointBackupService.GetBackupFile(fileName);

            return new BackupFile(stream, backupVolume.BackupFile);
        }
    }
}
