
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

using MediatR;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.Application.Services;


namespace Booth.DockerVolumeBackup.Application.Backups.Queries.GetBackupStatusEvents
{
    public record GetBackupStatusEventsQuery(int BackupId) : IStreamRequest<BackupStatusDto?>;


    public class BackupStatusDto
    {
        public int BackupId { get; set; }
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<BackupVolumeStatusDto> Volumes { get; set; } = [];
    }

    public class BackupVolumeStatusDto
    {
        public int BackupVolumeId { get; set; }
        public StatusDto Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }

    internal class GetBackupStatusEventsQueryHandler(IDataContext dataContext, IBackupNotificationService notificationService) : IStreamRequestHandler<GetBackupStatusEventsQuery, BackupStatusDto?>
    {

        public async IAsyncEnumerable<BackupStatusDto?> Handle(GetBackupStatusEventsQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var query = dataContext.Backups
                .Where(x => x.BackupId == request.BackupId)
                .Select(x => new BackupStatusDto()
                {
                    BackupId = x.BackupId,
                    Status = (StatusDto)x.Status,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Volumes = x.Volumes.Select(v => new BackupVolumeStatusDto()
                    {
                        BackupVolumeId = v.BackupVolumeId,
                        Status = (StatusDto)v.Status,
                        StartTime = v.StartTime,
                        EndTime = v.EndTime
                    }).ToList()
                });

            var backupStatus = await query.FirstOrDefaultAsync(cancellationToken);
            if ((backupStatus != null) && ((backupStatus.Status == StatusDto.Queued) || (backupStatus.Status == StatusDto.Active)))
            {
                var backupUpdateEvent = new AutoResetEvent(false);
                var onBackupUpdate = (int backupId) => { backupUpdateEvent.Set(); };
                notificationService.SubscribeToUpdates(onBackupUpdate);

                while (!cancellationToken.IsCancellationRequested)
                {
                    backupUpdateEvent.WaitOne();

                    backupStatus = await query.FirstOrDefaultAsync(cancellationToken);
                    if (backupStatus == null)
                        break;

                    if ((backupStatus.Status == StatusDto.Complete) || (backupStatus.Status == StatusDto.Error))
                        break;

                    yield return backupStatus;
                }

                notificationService.UnsubscribeToUpdates(onBackupUpdate);
            }
            yield return backupStatus;
        }
    }
}
