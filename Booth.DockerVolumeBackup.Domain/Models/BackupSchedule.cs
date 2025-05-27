using System;
using System.Collections;

namespace Booth.DockerVolumeBackup.Domain.Models
{
    public class BackupSchedule
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public TimeOnly Time { get; set; }

        public int BackupDefinitionId { get; set; }
        public BackupDefinition BackupDefinition { get; set; } = new BackupDefinition();

        public ICollection<Backup> Backups { get; } = [];

        public DateTimeOffset GetNextRunTime(DateTimeOffset after)
        {
            return after.AddDays(7);
        }
    }

}
