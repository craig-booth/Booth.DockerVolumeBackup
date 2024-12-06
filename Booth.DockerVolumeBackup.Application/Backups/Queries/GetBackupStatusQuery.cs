
using Dapper;
using MediatR;

using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using Booth.DockerVolumeBackup.Application.Services;
using System.Runtime.CompilerServices;


namespace Booth.DockerVolumeBackup.Application.Backups.Queries
{
    public record GetBackupStatusQuery(int BackupId) : IStreamRequest<BackupStatusDto?>;

    internal class GetBackupStatusQueryHandler(IDataContext dataContext, IBackupNotificationService notificationService) : IStreamRequestHandler<GetBackupStatusQuery, BackupStatusDto?>
    {

        public async IAsyncEnumerable<BackupStatusDto?> Handle(GetBackupStatusQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            BackupStatusDto? backupStatus = await GetBackupStatus(request.BackupId);
            if ((backupStatus != null) && ((backupStatus.Status == Status.Queued) || (backupStatus.Status == Status.Active)))
            {
                var backupUpdateEvent = new AutoResetEvent(false);
                var onBackupUpdate = (int backupId) => { backupUpdateEvent.Set(); };
                notificationService.SubscribeToUpdates(onBackupUpdate);

                while (!cancellationToken.IsCancellationRequested)
                {
                    backupUpdateEvent.WaitOne();

                    backupStatus = await GetBackupStatus(request.BackupId);
                    if (backupStatus == null) 
                        break;

                    if ((backupStatus.Status == Status.Complete) || (backupStatus.Status == Status.Error))
                        break;

                    yield return backupStatus;
                }

                notificationService.UnsubscribeToUpdates(onBackupUpdate);
            }
            yield return backupStatus;           
        }

        private async Task<BackupStatusDto?> GetBackupStatus(int backupId)
        {
            BackupStatusDto? backupStatus = null;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    SELECT BackupId, Status
                    FROM Backup
                    WHERE BackupId = @BackupId;

                    SELECT Volume, Status, EndTime
                    FROM BackupVolume
                    WHERE BackupId = @BackupId;
                """;
                var multi = await connection.QueryMultipleAsync(sql, new { BackupId = backupId });

                backupStatus = await multi.ReadSingleAsync<BackupStatusDto>();
                var backupVolumes = await multi.ReadAsync<VolumeBackupStatusDto>();

                backupStatus.Volumes.AddRange(backupVolumes);
            }

            return backupStatus;
        }
    }
}
