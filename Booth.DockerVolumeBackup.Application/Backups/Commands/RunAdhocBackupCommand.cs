using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;

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

    internal class RunAdhocBackupCommandHandler(IDataContext dataContext) : IRequestHandler<RunAdhocBackupCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(RunAdhocBackupCommand request, CancellationToken cancellationToken)
        {
            var backup = new Backup
            {
                ScheduleId = null,
                Status = Status.Queued
            };
            backup.Volumes.AddRange(request.Volumes.Select(x => new BackupVolume { Volume = x, Status = Status.Queued }));

            dataContext.Backups.Add(backup);

            await dataContext.SaveChangesAsync(cancellationToken);

            return backup.BackupId; 
        }
    }
}
