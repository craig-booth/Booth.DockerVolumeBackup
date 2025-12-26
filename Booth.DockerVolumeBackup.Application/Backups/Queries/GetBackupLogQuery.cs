using MediatR;

namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackupLog
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
