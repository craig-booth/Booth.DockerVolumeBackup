using Booth.DockerVolumeBackup.Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands.DeleteBackup
{
    public record DeleteBackupCommand(int BackupId) : IRequest<ErrorOr<bool>>;

    internal class DeleteBackupCommandHandler(IDataContext dataContext, IMountPointBackupService mountPointBackupService) : IRequestHandler<DeleteBackupCommand, ErrorOr<bool>>
    {
        public async Task<ErrorOr<bool>> Handle(DeleteBackupCommand request, CancellationToken cancellationToken)
        {
            var backup = await dataContext.Backups.AsTracking().FirstOrDefaultAsync(x => x.BackupId == request.BackupId);
            if (backup == null)
                return Error.NotFound();

            // Delete the backup
            if (backup.BackupDirectory != null)
                await mountPointBackupService.DeleteDirectoryAsync(backup.BackupDirectory);

            dataContext.Backups.Remove(backup);

            await dataContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
