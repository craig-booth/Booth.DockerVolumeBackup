using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands.RunAdhocBackup
{
    public record RunAdhocBackupCommand(IEnumerable<string> Volumes) : IRequest<ErrorOr<int>>;

    public class RunAdhocBackupCommandValidator : AbstractValidator<RunAdhocBackupCommand>
    {
        public RunAdhocBackupCommandValidator()
        {
            RuleFor(x => x.Volumes).NotEmpty().WithMessage("Atleast one volume must be selected.");
        }
    }

    internal class RunAdhocBackupCommandHandler(IDataContext dataContext, IServiceScopeFactory serviceScopeFactory, IBackupScheduler scheduler) : IRequestHandler<RunAdhocBackupCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(RunAdhocBackupCommand request, CancellationToken cancellationToken)
        {
            var backup = new Backup
            {
                Status = Status.Queued,
                BackupType = BackupType.Adhoc
            };
            backup.Volumes.AddRange(request.Volumes.Select(x => new BackupVolume { Volume = x, Status = Status.Queued }));
            dataContext.Backups.Add(backup);
            await dataContext.SaveChangesAsync(cancellationToken);

            var backupJob = new BackupJob(backup.BackupId, serviceScopeFactory);
            scheduler.QueueJob(backupJob);

            return backup.BackupId;
        }
    }
}
