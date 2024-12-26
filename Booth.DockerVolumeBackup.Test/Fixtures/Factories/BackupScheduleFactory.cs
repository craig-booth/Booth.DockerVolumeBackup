using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Test.Fixtures.Factories
{
    internal static class BackupScheduleFactory
    {
        private static readonly Faker<BackupSchedule> BackupScheduleFaker = new Faker<BackupSchedule>().CustomInstantiator(f =>
        {
            var schedule = new BackupSchedule()
            {
                ScheduleId = f.Random.Number(1, 100),
                Enabled = f.Random.Bool(0.9f),
                Sunday = f.Random.Bool(0.25f),
                Monday = f.Random.Bool(0.25f),
                Tuesday = f.Random.Bool(0.25f),
                Wednesday = f.Random.Bool(0.25f),
                Thursday = f.Random.Bool(0.25f),
                Friday = f.Random.Bool(0.25f),
                Saturday = f.Random.Bool(0.25f),
                Time = f.Date.BetweenTimeOnly(TimeOnly.MinValue, TimeOnly.MaxValue),
            };

            return schedule;
        });

        public static BackupSchedule Generate() => BackupScheduleFaker.Generate();

        public static List<BackupSchedule> Generate(int count) => BackupScheduleFaker.Generate(count);

    }
}
