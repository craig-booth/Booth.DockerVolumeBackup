using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Domain.Events
{
    public record RunAdhocBackupRequested(int BackupId) : INotification;
}
