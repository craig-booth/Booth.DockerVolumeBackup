using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands.DeleteMultipleBackups
{
    public record DeleteMultipleBackupsCommand(IEnumerable<int> BackupIds) : IRequest<ErrorOr<int>>;

    public class DeleteMultipleBackupsCommandValidator : AbstractValidator<DeleteMultipleBackupsCommand>
    {
        public DeleteMultipleBackupsCommandValidator()
        {
            RuleFor(x => x.BackupIds).NotEmpty().WithMessage("Atleast one backup must be selected.");
        }
    }

    internal class DeleteMultipleBackupsCommandHandler(IDataContext dataContext, IMountPointBackupService mountPointBackupService) : IRequestHandler<DeleteMultipleBackupsCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(DeleteMultipleBackupsCommand request, CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .AsTracking()
                .Where(x => request.BackupIds.Contains(x.BackupId));

            var backups = await query.ToListAsync(cancellationToken);
            if (backups == null)
                return Error.NotFound();

            // Delete the backups
            foreach (var backup in backups)
            {
                if (backup.BackupDirectory != null)
                    await mountPointBackupService.DeleteDirectoryAsync(backup.BackupDirectory);

                dataContext.Backups.Remove(backup);
            }

            await dataContext.SaveChangesAsync(cancellationToken);

            return backups.Count;
        }
    }
}
