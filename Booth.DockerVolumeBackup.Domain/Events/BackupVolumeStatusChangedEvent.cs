
using MediatR;

using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Domain.Events
{
    public class BackupVolumeStatusChangedEvent : EventArgs, INotification
    {
        public int BackupId { get; }
        public int VolumeId { get; }
        public Status Status { get; }

        public BackupVolumeStatusChangedEvent(int backupId, int volumeId, Status status)
        {
            BackupId = backupId;
            VolumeId = volumeId;
            Status = status;
        }
    }

}
