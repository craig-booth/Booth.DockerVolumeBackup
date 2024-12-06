using System.Collections.Concurrent;
using Booth.DockerVolumeBackup.Application.Backups.Events;
using MediatR;

namespace Booth.DockerVolumeBackup.Application.Services
{

    public interface IBackupNotificationService
    {
        void SubscribeToUpdates(Action<int> handler);
        void UnsubscribeToUpdates(Action<int> handler);
    }

    public class BackupNotificationService  : IBackupNotificationService, INotificationHandler<BackupStatusChangedEvent>, INotificationHandler<BackupVolumeStatusChangedEvent>
    {
        private readonly List<Action<int>> _Subscribers = new List<Action<int>>();
        private readonly ReaderWriterLockSlim _Lock = new ReaderWriterLockSlim();

        public void SubscribeToUpdates(Action<int> handler)
        {
            _Lock.EnterWriteLock();
            _Subscribers.Add(handler);
            _Lock.ExitWriteLock();
        }

        public void UnsubscribeToUpdates(Action<int> handler)
        {
            _Lock.EnterWriteLock();
            _Subscribers.Remove(handler);
            _Lock.ExitWriteLock();
        }

        Task INotificationHandler<BackupStatusChangedEvent>.Handle(BackupStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            SignalStatusChanged(notification.BackupId);

            return Task.CompletedTask;
        }

        Task INotificationHandler<BackupVolumeStatusChangedEvent>.Handle(BackupVolumeStatusChangedEvent notification, CancellationToken cancellationToken)
        {
            SignalStatusChanged(notification.BackupId);

            return Task.CompletedTask;
        }

        private void SignalStatusChanged(int backupId)
        {
            // Get list of subscribers for the backup 
            _Lock.EnterReadLock();
            var subscribers = new List<Action<int>>(_Subscribers);
            _Lock.ExitReadLock();

            // Invoke Handle for each subscriber
            subscribers.ForEach(x => x.Invoke(backupId));
        }
    }
}
