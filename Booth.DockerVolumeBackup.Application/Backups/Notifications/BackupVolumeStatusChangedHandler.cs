using Booth.DockerVolumeBackup.Domain.Events;
using Booth.DockerVolumeBackup.Application.Services;
using MediatR;

namespace Booth.DockerVolumeBackup.Application.Backups.Notifications
{
    internal class BackupVolumeStatusChangedHandler(IBackupNotificationService notificationService) : INotificationHandler<BackupVolumeStatusChangedEvent>
    {
        public Task Handle(BackupVolumeStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            notificationService.PublishUpdate(notification.BackupId);

            return Task.CompletedTask;
        }
    }
}
