using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Backups.Commands
{
    public record RunScheduledBackupCommand(int BackupId) : IRequest<bool>;

    internal class RunScheduledBackupCommandHandler : IRequestHandler<RunScheduledBackupCommand, bool>
    {
        public Task<bool> Handle(RunScheduledBackupCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
