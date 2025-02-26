using Booth.DockerVolumeBackup.Domain.Events;
using Booth.DockerVolumeBackup.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
