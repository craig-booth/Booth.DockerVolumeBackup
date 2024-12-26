using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Domain.Events;


namespace Booth.DockerVolumeBackup.Application.Backups.Events
{
    public class BackupStatusChangedEventHandler(IDataContext dataContext) : INotificationHandler<BackupStatusChangedEvent>
    {
        public async Task Handle(BackupStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            using (var connection = dataContext.CreateConnection())
            {
                var sql = "UPDATE Backup SET Status = @Status";

                if (notification.Status == Status.Active)
                    sql += ", StartTime = CURRENT_TIMESTAMP";
                else if (notification.Status == Status.Complete)
                    sql += ", EndTime = CURRENT_TIMESTAMP";

                sql += " WHERE BackupId = @BackupId";

                await connection.ExecuteAsync(sql, new { BackupId = notification.BackupId, Status = notification.Status });
            }
        }
    }
}
