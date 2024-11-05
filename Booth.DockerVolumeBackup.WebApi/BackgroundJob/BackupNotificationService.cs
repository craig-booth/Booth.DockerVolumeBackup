using System.Collections.Concurrent;

namespace Booth.DockerVolumeBackup.WebApi.Backup
{

    public interface IBackupNotificationService
    {
        void SignalStatusChanged(int backupId);
        void SubscribeToUpdates(int backupId, Action<int> handler);
        void UnsubscribeToUpdates(int backupId, Action<int> handler);
    }

    public class BackupNotificationService  : IBackupNotificationService
    {
        private readonly Dictionary<int, List<Action<int>>> _Subscribers = new Dictionary<int, List<Action<int>>>();
        private readonly ReaderWriterLockSlim _Lock = new ReaderWriterLockSlim();

        public void SignalStatusChanged(int backupId)
        {
            // Get list of subscribers for the backup
            var subscribersForBackup = new List<Action<int>>();
            _Lock.EnterReadLock();
            if (_Subscribers.TryGetValue(backupId, out var subscribers))
                subscribersForBackup.AddRange(subscribers);
            _Lock.ExitReadLock();

            // Invoke Handle for each subscriber
            subscribersForBackup.ForEach(x => x.Invoke(backupId));
        }

        public void SubscribeToUpdates(int backupId, Action<int> handler)
        {
            _Lock.EnterWriteLock();

            if (!_Subscribers.TryGetValue(backupId, out var subscribers))
            {
                subscribers = new List<Action<int>>();
                _Subscribers.Add(backupId, subscribers);
            }
            subscribers.Add(handler);

            _Lock.ExitWriteLock();
        }

        public void UnsubscribeToUpdates(int backupId, Action<int> handler)
        {
            _Lock.EnterWriteLock();

            if (_Subscribers.TryGetValue(backupId, out var subscribers))
            {
                subscribers.Remove(handler);
            }

            _Lock.ExitWriteLock();
        }
    }
}
