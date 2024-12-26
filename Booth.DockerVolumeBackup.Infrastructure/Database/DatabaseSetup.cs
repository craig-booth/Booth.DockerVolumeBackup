using System.Data;

using Dapper;
using Bogus;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DatabaseSetup(IDataContext context, IDockerClient dockerClient)
    {
        private Faker? _Faker;
        public async Task CreateDatabase()
        {
            using (var connection = context.CreateConnection())
            {

                var sql = """
                    CREATE TABLE IF NOT EXISTS Backup (
                        BackupId        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Status          INTEGER NOT NULL,
                        ScheduleId      INTEGER,
                        StartTime       TIMESTAMP,
                        EndTime         TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS BackupVolume (
                        BackupVolumeId  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        BackupId        INTEGER NOT NULL,
                        Volume          TEXT NOT NULL,
                        Status          INTEGER NOT NULL,
                        StartTime       TIMESTAMP,
                        EndTime         TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS BackupSchedule (
                        ScheduleId      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Name            TEXT NOT NULL,               
                        Enabled         INTEGER NOT NULL,
                        Sunday          INTEGER NOT NULL,
                        Monday          INTEGER NOT NULL,
                        Tuesday         INTEGER NOT NULL,
                        Wednesday       INTEGER NOT NULL,
                        Thursday        INTEGER NOT NULL,
                        Friday          INTEGER NOT NULL,
                        Saturday        INTEGER NOT NULL,
                        Time            TIME NOT NULL
                    );

                    
                    CREATE TABLE IF NOT EXISTS BackupScheduleVolume (
                        BackupScheduleVolumeId      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        ScheduleId      INTEGER NOT NULL,               
                        Volume          TEXT
                    );
                    """;

                await connection.ExecuteAsync(sql);
            }
        }

        public async Task SeedDatabase()
        {       
            var volumes = await dockerClient.Volumes.ListAsync();
            var volumeNames = volumes.Select(x => x.Name).ToList(); ;

            using (var connection = context.CreateConnection())
            {
                // Clear existing data
                await connection.ExecuteAsync("DELETE FROM BackupSchedule;");
                await connection.ExecuteAsync("DELETE FROM BackupScheduleVolume;");
                await connection.ExecuteAsync("DELETE FROM Backup;");
                await connection.ExecuteAsync("DELETE FROM BackupVolume;");
                await connection.ExecuteAsync("UPDATE sqlite_sequence SET seq = 0;");

                var schedules = await SeedSchedules(connection, volumeNames);

                await SeedBackups(connection, schedules);
            }
        }

        private async Task<List<(BackupSchedule schedule, List<string> volumes)>> SeedSchedules(IDbConnection connection, List<string> volumes)
        {
            Randomizer.Seed = new Random(1312);
            _Faker = new Faker();

            string ADD_SCHEDULE_SQL = """
                INSERT INTO BackupSchedule (Enabled, Name, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Time)
                    VALUES (@Enabled, @Name, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday, @Time) RETURNING RowId;
                """;

            string ADD_SCHEDULEVOLME_SQL = """
                INSERT INTO BackupScheduleVolume (ScheduleId, Volume)
                    VALUES (@ScheduleId, @Volume) RETURNING RowId;
                """;

            var addedSchedules = new List<(BackupSchedule schedule, List<string> volumes)>();

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
                schedule.ScheduleId = await connection.ExecuteScalarAsync<int>(ADD_SCHEDULE_SQL, schedule);

                var volumeCount = _Faker.Random.Number(volumes.Count);
                var scheduleVolumes = _Faker.PickRandom(volumes, volumeCount).ToList();

                foreach (var volume in scheduleVolumes)
                {
                    await connection.ExecuteScalarAsync<int>(ADD_SCHEDULEVOLME_SQL, new { ScheduleId = schedule.ScheduleId, Volume = volume});
                }

                addedSchedules.Add((schedule, scheduleVolumes));
            }

            return addedSchedules;
        }

        private async Task SeedBackups(IDbConnection connection, List<(BackupSchedule schedule, List<string> volumes)> schedules)
        {
            string ADD_BACKUP_SQL = """
                INSERT INTO Backup (Status, ScheduleId, StartTime, EndTime)
                    VALUES (@Status, @ScheduleId, @StartTime, @EndTime) RETURNING RowId;
                """;

            string ADD_BACKUPVOLUME_SQL = """
                INSERT INTO BackupVolume (BackupId, Volume, Status, StartTime, EndTime)
                    VALUES (@BackupId, @Volume, @Status, @StartTime, @EndTime) RETURNING RowId;
                """;

            var now = DateTimeOffset.UtcNow;
            foreach (var (schedule, volumes) in schedules)
            {
                var scheduledTime = new DateTimeOffset(_Faker.Date.PastDateOnly(1), schedule.Time, new TimeSpan(0));

                while (scheduledTime < now)
                {
                    var backup = new Backup()
                    {
                        Status = Status.Complete,
                        ScheduleId = schedule.ScheduleId,
                        StartTime = scheduledTime,
                        EndTime = scheduledTime.AddHours(1)
                    };
                    backup.BackupId = await connection.ExecuteScalarAsync<int>(ADD_BACKUP_SQL, backup);

                    foreach (var volume in volumes)
                    {
                        var backupVolume = new BackupVolume()
                        {
                            BackupId = backup.BackupId,
                            Volume = volume,
                            Status = Status.Complete,
                            StartTime = backup.StartTime,
                            EndTime = backup.EndTime?.AddMinutes(12)
                        };

                        backupVolume.BackupVolumeId = await connection.ExecuteScalarAsync<int>(ADD_BACKUPVOLUME_SQL, backupVolume);
                    }

                    scheduledTime = schedule.GetNextRunTime(scheduledTime);
                }    
            }      
        }

    }
}
