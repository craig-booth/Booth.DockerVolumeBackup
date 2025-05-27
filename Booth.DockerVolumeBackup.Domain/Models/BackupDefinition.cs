using Booth.DockerVolumeBackup.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Domain.Models
{
    public class BackupDefinition
    {
        public int BackupDefinitionId { get; set; }
        public int ScheduleId { get; set; }
        public int? KeepLast {  get; set; }
        public List<BackupDefinitionVolume> Volumes { get; set; } = [];
    }

    public class BackupDefinitionVolume
    {
        public int BackupDefinitionVolumeId { get; set; }
        public int BackupDefinitionId { get; set; }
        public required string Volume { get; set; }
    }
}
