using System.Data;

using Bogus;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Domain.Models;


namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DatabaseSetup(DataContext dataContext, IDockerClient dockerClient)
    {
     
        public async Task SeedDatabase()
        {
            Randomizer.Seed = new Random(1312);
            var faker = new Faker();

            var volumes = await dockerClient.Volumes.ListAsync();
            var volumeNames = volumes.Select(x => x.Name).ToList(); 
            var schedules = GenerateSchedules(volumeNames, faker);
            var backups = GenerateBackups(schedules, faker);

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

        private List<BackupSchedule> GenerateSchedules(List<string> volumeNames, Faker faker)
        {
            // Add schedules
            var backupScheduleFaker = new Faker<BackupSchedule>()
                .CustomInstantiator(f =>
                {
                    var time = f.Date.BetweenTimeOnly(TimeOnly.MinValue, TimeOnly.MaxValue);

                    var schedule = new BackupSchedule()
                    {
                        Name = f.Name.FirstName(),
                        Enabled = f.Random.Bool(0.75f),
                        Sunday = f.Random.Bool(0.25f),
                        Monday = f.Random.Bool(0.25f),
                        Tuesday = f.Random.Bool(0.25f),
                        Wednesday = f.Random.Bool(0.25f),
                        Thursday = f.Random.Bool(0.25f),
                        Friday = f.Random.Bool(0.25f),
                        Saturday = f.Random.Bool(0.25f),
                        Time = new TimeOnly(time.Hour, time.Minute)
                    };


                    return schedule;
                });

            var schedules = backupScheduleFaker.Generate(5);
            foreach (var schedule in schedules)
            {
                var volumeCount = faker.Random.Number(volumeNames.Count);
                var scheduleVolumes = faker.PickRandom(volumeNames, volumeCount);
                schedule.BackupDefinition.Volumes.AddRange(scheduleVolumes.Select(x => new BackupDefinitionVolume { Volume = x }));
            }

            return schedules;
        }

        private List<Backup> GenerateBackups(List<BackupSchedule> schedules, Faker faker)
        {
            var backups = new List<Backup>();

            var now = DateTimeOffset.UtcNow;
            foreach (var schedule in schedules)
            {
                var scheduledTime = new DateTimeOffset(faker.Date.PastDateOnly(1), schedule.Time, new TimeSpan(0));

                while (scheduledTime < now)
                {
                    var backup = new Backup()
                    {
                        Status = Status.Complete,
                        Schedule = schedule,
                        StartTime = scheduledTime,
                        EndTime = scheduledTime.AddHours(1)
                    };

                    backup.Volumes.AddRange(schedule.BackupDefinition.Volumes.Select(x => new BackupVolume()
                    {
                        Volume = x.Volume,
                        Status = Status.Complete,
                        StartTime = backup.StartTime,
                        EndTime = backup.EndTime?.AddMinutes(12)
                    }));

                    backups.Add(backup);

                    scheduledTime = schedule.GetNextRunTime(scheduledTime);
                }    
            }

            return backups;
        }

    }
}
