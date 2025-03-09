using Microsoft.EntityFrameworkCore;

using MediatR;
using ErrorOr;


using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Schedules.Commands
{
    public record DeleteScheduleCommand(int ScheduleId) : IRequest<ErrorOr<bool>>;

    internal class DeleteScheduleCommandHandler(IDataContext dataContext) : IRequestHandler<DeleteScheduleCommand, ErrorOr<bool>>
    {
        public async Task<ErrorOr<bool>> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
        {
            using (var transaction = await dataContext.BeginTransactionAsync())
            {


                await dataContext.Backups
                    .Where(x => x.ScheduleId == request.ScheduleId)
                    .ExecuteDeleteAsync(cancellationToken);

                var recordsAffected = await dataContext.Schedules
                    .Where(x => x.ScheduleId == request.ScheduleId)
                    .ExecuteDeleteAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                if (recordsAffected == 0)
                    return Error.NotFound();

                return true;
            }
      
        }
    }
}
