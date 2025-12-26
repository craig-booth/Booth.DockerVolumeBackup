
using Microsoft.EntityFrameworkCore;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Booth.DockerVolumeBackup.Application.Services;


namespace Booth.DockerVolumeBackup.Application.BackgroundJobs
{
    internal class ScheduledBackupJob : IBackgroundJob
    {
        private readonly int _ScheduleId;
        private readonly IServiceScopeFactory _ScopeFactory;

        public int Id => _ScheduleId;

        public ScheduledBackupJob(int scheduleId, IServiceScopeFactory scopeFactory)
        {
            _ScheduleId = scheduleId;
            _ScopeFactory = scopeFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            using (var scope = _ScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var scheduleUtils = scope.ServiceProvider.GetRequiredService<IScheduleUtils>();
                var scheduler = scope.ServiceProvider.GetRequiredService<IBackupScheduler>();
                var serviceScopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

                var backup = await scheduleUtils.CreateBackupFromScheduleAsync(_ScheduleId, cancellationToken);
                if (backup == null)
                    return;

                var backupJob = new BackupJob(backup.BackupId, serviceScopeFactory);
                if (backupJob != null)
                    scheduler.QueueJob(backupJob);
            }

        }


    }
}
