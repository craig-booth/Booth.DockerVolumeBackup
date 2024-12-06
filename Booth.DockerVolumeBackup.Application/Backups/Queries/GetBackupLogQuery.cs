using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetBackupLogQuery : IRequest<string>;

    internal class GetBackupLogQueryHandler() : IRequestHandler<GetBackupLogQuery, string>
    {
        public Task<string> Handle(GetBackupLogQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
