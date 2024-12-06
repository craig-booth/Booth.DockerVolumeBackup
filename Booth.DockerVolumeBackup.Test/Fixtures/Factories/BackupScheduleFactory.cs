using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;

using Booth.DockerVolumeBackup.Domain;

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
                Days = new BitArray([true, false, false, false, false, true, false]),
                Time = f.Date.BetweenTimeOnly(TimeOnly.MinValue, TimeOnly.MaxValue),
                FirstRunTime = f.Date.PastOffset(1)
            };

            return schedule;
        });

        public static BackupSchedule Generate() => BackupScheduleFaker.Generate();

        public static List<BackupSchedule> Generate(int count) => BackupScheduleFaker.Generate(count);

    }
}
