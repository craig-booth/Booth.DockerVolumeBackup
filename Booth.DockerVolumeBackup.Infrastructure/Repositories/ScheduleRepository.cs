using Dapper;

using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Application.Interfaces;


namespace Booth.DockerVolumeBackup.Infrastructure.Repositories
{

    internal class ScheduleRepository(IDataContext dataContext) : IScheduleRepository
    {
        public async Task<int> Add(BackupSchedule schedule)
        {
            schedule.ScheduleId = 0;

            using (var connection = dataContext.CreateConnection())
            {
                var sql = """
                    INSERT INTO BackupSchedule (Enabled, Name, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Time)
                    VALUES (@Enabled, @Name, @Sunday, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday, @Time) RETURNING RowId;
                """;

                schedule.ScheduleId = await connection.ExecuteScalarAsync<int>(sql, schedule);

                sql = """
                    INSERT INTO BackupScheduleVolume (ScheduleId, Volume)
                    VALUES (@ScheduleId, @Volume) RETURNING RowId;
                """;
                foreach (var volume in schedule.Volumes)
                {
                    volume.ScheduleId = schedule.ScheduleId;
                    volume.BackupScheduleVolumeId = await connection.ExecuteScalarAsync<int>(sql, volume);
                }
            }

            return schedule.ScheduleId;
        }

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<BackupSchedule> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(BackupSchedule schedule)
        {
            throw new NotImplementedException();
        }
    }
}
