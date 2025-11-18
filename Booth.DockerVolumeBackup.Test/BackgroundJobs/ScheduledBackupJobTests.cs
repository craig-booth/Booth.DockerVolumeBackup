using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;

using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using NSubstitute.ReturnsExtensions;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Booth.DockerVolumeBackup.Application.Services;

namespace Booth.DockerVolumeBackup.Test.BackgroundJobs
{
    public class ScheduledBackupJobTests
    {
       
        [Fact]
        public async Task JobQueuesBackupWhenExecuted()
        {

            var scheduleId = 14;
            var backupId = 12;

            var queuedJob = Substitute.For<IBackgroundJob>();

            var scheduler = Substitute.For<IBackupScheduler>();

            var backup = new Backup { BackupId = backupId, ScheduleId = scheduleId };

            var scheduleUtils = Substitute.For<IScheduleUtils>();
            scheduleUtils.CreateBackupFromScheduleAsync(scheduleId, Arg.Any<CancellationToken>()).Returns(backup);

            var dataContext = Substitute.For<IDataContext>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IScheduleUtils>(scheduleUtils);
            scopeFactory.RegisterService<IBackupScheduler>(scheduler);       

            var job = new ScheduledBackupJob(scheduleId, scopeFactory);
            await job.Execute(CancellationToken.None);


            scheduler.Received().QueueJob(Arg.Is<BackupJob>(x => x.Id == 12));
        }

        [Fact]
        public async Task NoJobQueuedIfScheduleNotFound()
        {
            var scheduleId = 14;

            var scheduler = Substitute.For<IBackupScheduler>();

            var scheduleUtils = Substitute.For<IScheduleUtils>();
            scheduleUtils.CreateBackupFromScheduleAsync(scheduleId, Arg.Any<CancellationToken>()).ReturnsNull();

            var dataContext = Substitute.For<IDataContext>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IScheduleUtils>(scheduleUtils);
            scopeFactory.RegisterService<IBackupScheduler>(scheduler);

            var job = new ScheduledBackupJob(scheduleId, scopeFactory);
            await job.Execute(CancellationToken.None);


            scheduler.DidNotReceive().QueueJob(Arg.Any<IBackgroundJob>());
        }

    }
}
