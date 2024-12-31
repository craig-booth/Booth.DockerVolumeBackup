using Booth.DockerVolumeBackup.Domain.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Booth.DockerVolumeBackup.Domain.Models
{
    public enum Status { Queued, Active, Complete, Error }

    public class Backup
    {
        public int BackupId { get; set; }
        public Status Status { get; set; }
        public int? ScheduleId { get; set; }
        public BackupSchedule? Schedule { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<BackupVolume> Volumes { get; set; } = new List<BackupVolume>();

        public event EventHandler<BackupStatusChangedEvent> BackupStatusChanged;
        public event EventHandler<BackupVolumeStatusChangedEvent> BackupVolumeStatusChanged;

        public void StartBackup()
        {
            Status = Status.Active;
            StartTime = DateTimeOffset.Now;
            EndTime = null;

            OnBackupStatusChanged();
        }

        public void EndBackup()
        {
            Status = Status.Complete;
            EndTime = DateTimeOffset.Now;

            OnBackupStatusChanged();
        }

        public void StartVolumeBackup(string volume)
        {
            var volumeBackup = Volumes.FirstOrDefault(x => x.Volume == volume);
            if (volumeBackup != null)
            {
                volumeBackup.Status = Status.Active;
                volumeBackup.StartTime = DateTimeOffset.Now;
                volumeBackup.EndTime = null;

                OnBackupVolumeStatusChanged(volumeBackup);
            }
        }

        public void EndVolumeBackup(string volume)
        {
            var volumeBackup = Volumes.FirstOrDefault(x => x.Volume == volume);
            if (volumeBackup != null)
            {
                volumeBackup.Status = Status.Complete;
                volumeBackup.EndTime = DateTimeOffset.Now;

                OnBackupVolumeStatusChanged(volumeBackup);
            }
        }

        protected virtual void OnBackupStatusChanged()
        {
            var e = new BackupStatusChangedEvent(BackupId, Status);

            BackupStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnBackupVolumeStatusChanged(BackupVolume volume)
        {
            var e = new BackupVolumeStatusChangedEvent(BackupId, volume.BackupVolumeId, volume.Status);

            BackupVolumeStatusChanged?.Invoke(this, e);
        }

    }

    public class BackupVolume
    {
        public int BackupVolumeId { get; set; }
        public int BackupId { get; set; }
        public required string Volume { get; set; }
        public Status Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }
}
