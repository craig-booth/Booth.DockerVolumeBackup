using System.Data;
using Microsoft.EntityFrameworkCore;

using Bogus;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Domain.Models;


namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DatabaseSetup(DataContext dataContext, IDockerClient dockerClient)
    {
        private Faker? _Faker;
      
        public async Task SeedDatabase()
        {       
            var volumes = await dockerClient.Volumes.ListAsync();
            var volumeNames = volumes.Select(x => x.Name).ToList(); 
            var schedules = GenerateSchedules(volumeNames);
            var backups = GenerateBackups(schedules);

            // Clear existing data
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupSchedule;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupScheduleVolume;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM Backup;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("DELETE FROM BackupVolume;", [], CancellationToken.None);
            await dataContext.ExecuteSqlCommandAsync("UPDATE sqlite_sequence SET seq = 0;", [], CancellationToken.None);

            //Add new data
            dataContext.Schedules.AddRange(schedules);
            dataContext.Backups.AddRange(backups);
            await dataContext.SaveChangesAsync();
        }

        private List<BackupSchedule> GenerateSchedules(List<string> volumeNames)
        {
            Randomizer.Seed = new Random(1312);
            _Faker = new Faker();

            // Add schedules
            var faker = new Faker<BackupSchedule>()
                .CustomInstantiator(f =>
                {
                    var schedule = new BackupSchedule()
                    {
                        Name = f.Name.FirstName(),
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

            var schedules = faker.Generate(5);
            foreach (var schedule in schedules)
            {
                var volumeCount = _Faker.Random.Number(volumeNames.Count);
                var scheduleVolumes = _Faker.PickRandom(volumeNames, volumeCount);
                schedule.Volumes.AddRange(scheduleVolumes.Select(x => new BackupScheduleVolume { Volume = x }));
            }

            return schedules;
        }

        private List<Backup> GenerateBackups(List<BackupSchedule> schedules)
        {
            var backups = new List<Backup>();

            var now = DateTimeOffset.UtcNow;
            foreach (var schedule in schedules)
            {
                var scheduledTime = new DateTimeOffset(_Faker.Date.PastDateOnly(1), schedule.Time, new TimeSpan(0));

                while (scheduledTime < now)
                {
                    var backup = new Backup()
                    {
                        Status = Status.Complete,
                        Schedule = schedule,
                        StartTime = scheduledTime,
                        EndTime = scheduledTime.AddHours(1)
                    };

                    backup.Volumes.AddRange(schedule.Volumes.Select(x => new BackupVolume()
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
