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
        public List<BackupScheduleVolume> Volumes { get; set; } = new List<BackupScheduleVolume>();

        public DateTimeOffset GetNextRunTime(DateTimeOffset after)
        {
            return after.AddDays(7);
        }
    }

    public class BackupScheduleVolume
    {
        public int BackupScheduleVolumeId { get; set; }
        public int ScheduleId { get; set; }
        public required string Volume { get; set; }
    }
}
