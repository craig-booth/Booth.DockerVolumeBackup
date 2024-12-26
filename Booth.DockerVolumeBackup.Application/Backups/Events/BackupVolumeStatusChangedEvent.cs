using MediatR;
using Dapper;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Domain.Events;


namespace Booth.DockerVolumeBackup.Application.Backups.Events
{
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
