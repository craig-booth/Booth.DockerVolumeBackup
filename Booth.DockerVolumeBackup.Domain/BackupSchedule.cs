using System;
using System.Collections;

namespace Booth.DockerVolumeBackup.Domain
{
    public class BackupSchedule
    {
        public int ScheduleId { get; set; }
        public bool Enabled { get; set; }
        public BitArray Days { get; set; } = new BitArray(7);
        public TimeOnly Time { get; set; }

        public DateTimeOffset GetNextRunTime(DateTimeOffset after)
        {
            return after.AddDays(7);
        }
    }
}
