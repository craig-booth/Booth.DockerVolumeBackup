
using MediatR;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Events;


namespace Booth.DockerVolumeBackup.Infrastructure.Services
{
    public interface IBackupNotificationService
    {
        void PublishUpdate(int backupId);
        void SubscribeToUpdates(Action<int> handler);
        void UnsubscribeToUpdates(Action<int> handler);
    }
    internal class BackupNotificationService : IBackupNotificationService
    {
        private readonly List<Action<int>> _Subscribers;
        private readonly ReaderWriterLockSlim _Lock;

        public BackupNotificationService()
        {
            _Subscribers = new List<Action<int>>();
            _Lock = new ReaderWriterLockSlim();
        }
        public void PublishUpdate(int backupId)
        {
            // Get list of subscribers for the backup 
            _Lock.EnterReadLock();
            var subscribers = new List<Action<int>>(_Subscribers);
            _Lock.ExitReadLock();

            // Invoke Handle for each subscriber
            subscribers.ForEach(x => x.Invoke(backupId));
        }

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
    }
}
