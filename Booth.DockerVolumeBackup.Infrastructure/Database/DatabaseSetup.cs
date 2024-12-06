using System.Data;

using Dapper;
using Bogus;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Domain;
using System.Collections;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using System.Net.WebSockets;
using System;

namespace Booth.DockerVolumeBackup.Infrastructure.Database
{
    internal class DatabaseSetup(IDataContext context, IDockerClient dockerClient)
    {
        private Faker _Faker;
        public async Task CreateDatabase()
        {
            using (var connection = context.CreateConnection())
            {

                var sql = """
                    CREATE TABLE IF NOT EXISTS Backup (
                        BackupId        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        Status          INTEGER NOT NULL,
                        ScheduleId      INTEGER,
                        ScheduledTime   TIMESTAMP,
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
                        Enabled         INTEGER NOT NULL,
                        Days            INTEGER NOT NULL,
                        Time            TIME NOT NULL
                    );
                    """;

                await connection.ExecuteAsync(sql);
            }
        }

        public async Task SeedDatabase()
        {
            Randomizer.Seed = new Random(1312);
            _Faker = new Faker();
       
            var volumes = await dockerClient.Volumes.ListAsync();

            using (var connection = context.CreateConnection())
            {
                // Clear existing data
                await connection.ExecuteAsync("DELETE FROM BackupSchedule");
                await connection.ExecuteAsync("DELETE FROM Backup");
                await connection.ExecuteAsync("DELETE FROM BackupVolume");

                var schedules = await SeedSchedules(connection);

                await SeedBackups(connection, schedules, volumes);
            }
        }

        private async Task<List<BackupSchedule>> SeedSchedules(IDbConnection connection)
        {
            string ADD_SCHEDULE_SQL = """
                INSERT INTO BackupSchedule (Enabled, Days, Time)
                    VALUES (@Enabled, @Days, @Time) RETURNING RowId;
                """;

            // Add schedules
            var faker = new Faker<BackupSchedule>()
                .CustomInstantiator(f =>
                {
                    var schedule = new BackupSchedule()
                    {
                        Enabled = f.Random.Bool(0.9f),
                        Days = new BitArray([true, false, false, false, false, true, false]),
                        Time = f.Date.BetweenTimeOnly(TimeOnly.MinValue, TimeOnly.MaxValue), 
                    };

                    return schedule;
                });
            var schedules = faker.Generate(5);
            foreach (var schedule in schedules)
            {
                schedule.ScheduleId = await connection.ExecuteScalarAsync<int>(ADD_SCHEDULE_SQL, schedule);
            }

            return schedules;
        }

        private async Task SeedBackups(IDbConnection connection, IList<BackupSchedule> schedules, IList<Volume> volumes)
        {
            string ADD_BACKUP_SQL = """
                INSERT INTO Backup (Status, ScheduleId, ScheduledTime, StartTime, EndTime)
                    VALUES (@Status, @ScheduleId, @ScheduledTime, @StartTime, @EndTime) RETURNING RowId;
                """;

            string ADD_BACKUPVOLUME_SQL = """
                INSERT INTO BackupVolume (BackupId, Volume, Status, StartTime, EndTime)
                    VALUES (@BackupId, @Volume, @Status, @StartTime, @EndTime) RETURNING RowId;
                """;

            var now = DateTimeOffset.UtcNow;
            foreach (var schedule in schedules)
            {
                var scheduledTime = new DateTimeOffset(_Faker.Date.PastDateOnly(1), schedule.Time, new TimeSpan(0));

                var volumeCount = _Faker.Random.Number(volumes.Count);
                var backupVolumes = _Faker.PickRandom(volumes, volumeCount);

                while (scheduledTime < now)
                {
                    var backup = new Backup()
                    {
                        Status = Status.Complete,
                        ScheduleId = schedule.ScheduleId,
                        ScheduledTime = scheduledTime,
                        StartTime = scheduledTime,
                        EndTime = scheduledTime.AddHours(1)
                    };
                    backup.BackupId = await connection.ExecuteScalarAsync<int>(ADD_BACKUP_SQL, backup);

                    foreach (var volume in backupVolumes)
                    {
                        var backupVolume = new BackupVolume()
                        {
                            BackupId = backup.BackupId,
                            Volume = volume.Name,
                            Status = Status.Complete,
                            StartTime = backup.StartTime,
                            EndTime = backup.EndTime.AddMinutes(12)
                        };

                        backupVolume.BackupVolumeId = await connection.ExecuteScalarAsync<int>(ADD_BACKUPVOLUME_SQL, backupVolume);
                    }

                    scheduledTime = schedule.GetNextRunTime(scheduledTime);
                }    
            }      
        }

    }
}
