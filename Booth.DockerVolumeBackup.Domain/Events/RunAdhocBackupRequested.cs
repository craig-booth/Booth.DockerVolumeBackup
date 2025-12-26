using MediatR;

namespace Booth.DockerVolumeBackup.Domain.Events
{
    public record RunAdhocBackupRequested(int BackupId) : INotification;
}
