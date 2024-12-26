using MediatR;

using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Domain.Events
{
    public class BackupStatusChangedEvent : EventArgs, INotification
    {
        public int BackupId { get; }
        public Status Status { get; }

        public BackupStatusChangedEvent(int backupId, Status status)
        {
            BackupId = backupId;
            Status = status;
        }
    }
}
