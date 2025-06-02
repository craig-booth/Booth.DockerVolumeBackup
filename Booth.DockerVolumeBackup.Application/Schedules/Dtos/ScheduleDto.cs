using Booth.DockerVolumeBackup.Domain.Models;
using MediatR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Application.Schedules.Dtos
{
    public class ScheduleDto
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; } = "";
        public bool Enabled { get; set; }
        public ScheduleDaysDto Days { get; set; } = new ScheduleDaysDto();
        public TimeOnly Time { get; set; }
        public int KeepLast { get; set; }
        public List<string> Volumes { get; set; } = new List<string>();


        internal static ScheduleDto FromDomain(BackupSchedule schedule)
        {
            var scheduleDto = new ScheduleDto()
            {
                ScheduleId = schedule.ScheduleId,
                Name = schedule.Name,
                Enabled = schedule.Enabled,
                Days = new ScheduleDaysDto()
                {
                    Sunday = schedule.Sunday,
                    Monday = schedule.Monday,
                    Tuesday = schedule.Tuesday,
                    Wednesday = schedule.Wednesday,
                    Thursday = schedule.Thursday,
                    Friday = schedule.Friday,
                    Saturday = schedule.Saturday
                },
                Time = schedule.Time,
            };

         //   scheduleDto.Volumes.AddRange(schedule.BackupDefinition.Volumes.Select(x => x.Volume));

            return scheduleDto;
        }

        internal BackupSchedule ToDomain()
        {
            var schedule = new BackupSchedule()
            {
                Name = Name,
                Enabled = Enabled,
                Sunday = Days.Sunday,
                Monday = Days.Monday,
                Tuesday = Days.Tuesday,
                Wednesday = Days.Wednesday,
                Thursday = Days.Thursday,
                Friday = Days.Friday,
                Saturday = Days.Saturday,
                Time = Time
            };

            return schedule;
        }
    }

    public class ScheduleDaysDto
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
    }
}
