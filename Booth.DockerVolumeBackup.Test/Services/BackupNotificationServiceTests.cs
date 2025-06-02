using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using MediatR;

using Booth.DockerVolumeBackup.Domain.Events;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Backups.Notifications;
using Booth.DockerVolumeBackup.Application.Services;

namespace Booth.DockerVolumeBackup.Test.Services
{
    public  class BackupNotificationServiceTests
    {


        [Fact]
        public void ReceiveNotificationsForBackupStatusChangedEvent()
        {
            var notificationService = new BackupNotificationService();
            var handler = new BackupStatusChangedHandler(notificationService);

            int? backupId = null;
            Action<int> action = (id) => { backupId = id; };
            notificationService.SubscribeToUpdates(action);

            var event_ = new BackupStatusChangedEvent(12, Status.Complete);
            handler.Handle(event_, CancellationToken.None);

            backupId.Should().Be(12);
        }

        [Fact]
        public void ReceiveNotificationsForBackupVolumeStatusChangedEvent()
        {
            var notificationService = new BackupNotificationService();
            var handler = new BackupVolumeStatusChangedHandler(notificationService);

            int? backupId = null;
            Action<int> action = (id) => { backupId = id; };
            notificationService.SubscribeToUpdates(action);

            var event_ = new BackupVolumeStatusChangedEvent(6, 4, Status.Complete);
            handler.Handle(event_, CancellationToken.None);

            backupId.Should().Be(6);
        }

        [Fact]
        public void ReceiveNotificationsMultipleSubscribers()
        {
            var notificationService = new BackupNotificationService();
            var handler = new BackupStatusChangedHandler(notificationService);

            int? backupId1 = null;
            Action<int> action1 = (id) => { backupId1 = id; };
            notificationService.SubscribeToUpdates(action1);

            int? backupId2 = null;
            Action<int> action2 = (id) => { backupId2 = id; };
            notificationService.SubscribeToUpdates(action2);

            var event_ = new BackupStatusChangedEvent(45, Status.Complete);
            handler.Handle(event_, CancellationToken.None);

            backupId1.Should().Be(45);
            backupId2.Should().Be(45);
        }

        [Fact]
        public void NoNotificationsIfNotSubscribed()
        {      
            var notificationService = new BackupNotificationService();
            var handler = new BackupStatusChangedHandler(notificationService);

            int? backupId = null;

            var event_ = new BackupStatusChangedEvent(12, Status.Complete);
            handler.Handle(event_, CancellationToken.None);

            backupId.Should().BeNull();
        }

        [Fact]
        public void NoNotificationsIfUnsubscribe()
        {
            var notificationService = new BackupNotificationService();
            var handler = new BackupStatusChangedHandler(notificationService);

            int? backupId = null;
            Action<int> action = (id) => { backupId = id; };
            notificationService.SubscribeToUpdates(action);

            var event_ = new BackupStatusChangedEvent(12, Status.Complete);
            handler.Handle(event_, CancellationToken.None);
            backupId.Should().Be(12);


            notificationService.UnsubscribeToUpdates(action);

            var event2_ = new BackupStatusChangedEvent(14, Status.Complete);
            handler.Handle(event2_, CancellationToken.None);
            backupId.Should().Be(12);

        }

    }
}
