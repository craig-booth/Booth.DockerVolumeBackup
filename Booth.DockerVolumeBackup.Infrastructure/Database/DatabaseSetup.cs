using Bogus;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using System.Data;


namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DatabaseSetup(DataContext dataContext, IDockerClient dockerClient)
    {

        private const int SEED = 34643;
        public async Task SeedDatabase()
        {
            var volumes = await dockerClient.Volumes.ListAsync();
            var volumeNames = volumes.Select(x => x.Name).ToList();
            var schedules = GenerateSchedules(volumeNames);
            var backups = GenerateBackups(schedules);

            // Clear existing data
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupVolume;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM Backup;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupDefinitionVolume;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupDefinition;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupSchedule;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("UPDATE sqlite_sequence SET seq = 0;", [], CancellationToken.None);

            //Add new data
            dataContext.Schedules.AddRange(schedules);
            dataContext.Backups.AddRange(backups);
            await dataContext.SaveChangesAsync();
        }

        private List<BackupSchedule> GenerateSchedules(List<string> volumeNames)
        {
            var backupDefinitionFaker = new Faker<BackupDefinition>()
            .UseSeed(SEED)
            .RuleFor(x => x.Volumes, f =>
            {
                var volumeCount = f.Random.Number(3, volumeNames.Count);
                var scheduleVolumes = f.PickRandom(volumeNames, volumeCount);

                var backupVolumes = scheduleVolumes.Select(x => new BackupDefinitionVolume { Volume = x });
                return backupVolumes.ToList();
            })
            .RuleFor(x => x.KeepLast, f => f.Random.Number(0, 10));

            var backupScheduleFaker = new Faker<BackupSchedule>()
                .UseSeed(SEED)
                .RuleFor(x => x.Name, f => f.Commerce.ProductAdjective() + f.Commerce.Product())
                .RuleFor(x => x.Enabled, f => f.Random.Bool(0.90f))
                .RuleFor(x => x.Sunday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Monday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Tuesday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Wednesday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Thursday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Friday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Saturday, f => f.Random.Bool(0.25f))
                .RuleFor(x => x.Time, f =>
                {
                    var time = f.Date.BetweenTimeOnly(TimeOnly.MinValue, TimeOnly.MaxValue);
                    return new TimeOnly(time.Hour, time.Minute);
                })
                .RuleFor(x => x.BackupDefinition, f => backupDefinitionFaker.Generate());

            var schedules = backupScheduleFaker.UseSeed(SEED).Generate(5);

            return schedules;
        }

        private List<Backup> GenerateBackups(List<BackupSchedule> schedules)
        {
            var faker = new Faker() { Random = new Randomizer(SEED) };

            var backups = new List<Backup>();

            var now = DateTimeOffset.UtcNow;
            foreach (var schedule in schedules)
            {
                var scheduledTime = new DateTimeOffset(faker.Date.PastDateOnly(1), schedule.Time, new TimeSpan(0));
                var backupCount = 0;

                while (scheduledTime < now)
                {
                    backupCount++;

                    var backup = new Backup()
                    {
                        Status = Status.Complete,
                        BackupType = BackupType.Scheduled,
                        Schedule = schedule,
                        StartTime = scheduledTime,
                        EndTime = scheduledTime.AddHours(1),
                        BackupDirectory = $"/backup/{scheduledTime:yyyy-MM-dd}_{backupCount}",
                    };

                    backup.Volumes.AddRange(schedule.BackupDefinition.Volumes.Select(x => new BackupVolume()
                    {
                        Volume = x.Volume,
                        Status = Status.Complete,
                        StartTime = backup.StartTime,
                        EndTime = backup.EndTime?.AddMinutes(12),
                        BackupFile = $"{x.Volume}.tar.gz",
                        BackupSize = faker.Random.Number(10000, 1000000)
                    }));

                    backups.Add(backup);

                    scheduledTime = schedule.GetNextRunTime(scheduledTime);
                }
            }

            return backups;
        }

    }
}
