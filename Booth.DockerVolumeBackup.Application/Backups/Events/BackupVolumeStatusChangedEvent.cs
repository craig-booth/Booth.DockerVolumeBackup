using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;


namespace Booth.DockerVolumeBackup.Application.Backups.Events
{
    public record BackupVolumeStatusChangedEvent(int BackupId, int VolumeId, Status Status) : INotification;

    public class BackupVolumeStatusChangedEventHandler(IDataContext dataContext) : INotificationHandler<BackupVolumeStatusChangedEvent>
    {
        public async Task Handle(BackupVolumeStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            using (var connection = dataContext.CreateConnection())
            {
                var sql = "UPDATE BackupVolume SET Status = @Status";

                if (notification.Status == Status.Active)
                    sql += ", StartTime = CURRENT_TIMESTAMP";
                else if (notification.Status == Status.Complete)
                    sql += ", EndTime = CURRENT_TIMESTAMP";

                sql += " WHERE BackupVolumeId = @BackupVolumeId";

                await connection.ExecuteAsync(sql, new { BackupVolumeId = notification.VolumeId, Status = notification.Status });
            }
        }
    }
}
