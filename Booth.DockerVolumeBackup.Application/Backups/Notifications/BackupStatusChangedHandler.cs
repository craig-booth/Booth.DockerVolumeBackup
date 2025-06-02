using Booth.DockerVolumeBackup.Domain.Events;
using Booth.DockerVolumeBackup.Application.Services;
using MediatR;

namespace Booth.DockerVolumeBackup.Application.Backups.Notifications
{
    internal class BackupStatusChangedHandler(IBackupNotificationService notificationService) : INotificationHandler<BackupStatusChangedEvent>
    {
        public Task Handle(BackupStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            notificationService.PublishUpdate(notification.BackupId);

            return Task.CompletedTask;
        }
    }
}
