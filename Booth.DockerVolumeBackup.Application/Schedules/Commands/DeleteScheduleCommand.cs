using System;
using System.Threading.Tasks;

using MediatR;
using ErrorOr;
using Dapper;

using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Application.Schedules.Commands
{
    public record DeleteScheduleCommand(int Id) : IRequest<ErrorOr<bool>>;

    internal class DeleteScheduleCommandHandler(IDataContext dataContext) : IRequestHandler<DeleteScheduleCommand, ErrorOr<bool>>
    {
        public async Task<ErrorOr<bool>> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
        {
            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    DELETE FROM BackupSchedule WHERE ScheduleId = @ScheduleId;
                    DELETE FROM BackupScheduleVolume WHERE ScheduleId = @ScheduleId;
                """;
                var recordsAffected = await connection.ExecuteAsync(sql, new { ScheduleId = request.Id });

                if (recordsAffected == 0)
                    return Error.NotFound();

                return true;
            }

            
        }
    }
}
