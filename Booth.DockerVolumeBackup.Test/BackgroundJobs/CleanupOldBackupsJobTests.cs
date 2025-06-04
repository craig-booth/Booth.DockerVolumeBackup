using Xunit;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using MockQueryable.NSubstitute;
using FluentAssertions;

using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;


namespace Booth.DockerVolumeBackup.Test.BackgroundJobs
{
    public class CleanupOldBackupsJobTests
    {
        [Fact]
        public async Task CleanupJobIgnoresSchedulesWithKeepLastZero()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 0 }
            };
            var schedules = new[] { schedule };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup6"}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backups.Should().HaveCount(6, "because the schedule has KeepLast set to 0, it should not delete any backups");
            await mountPointBackupService.DidNotReceive().DeleteDirectoryAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CleanupJobOnlyCountsCompletedBackups()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 2 }
            };
            var schedules = new[] { schedule };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Active, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Queued, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Error, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup6"}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backupDataSet.Received(1).Remove(Arg.Any<Backup>());
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 6));      
            await mountPointBackupService.Received(1).DeleteDirectoryAsync("/backups/backup6");
        }

        [Fact]
        public async Task CleanupJobDoesNothingIfNotEnoughCompletedBackups()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 10 }
            };
            var schedules = new[] { schedule };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup6"}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backupDataSet.DidNotReceive().Remove(Arg.Any<Backup>());
            await mountPointBackupService.DidNotReceive().DeleteDirectoryAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CleanupJobHandlesMulitpleSchedules()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 4 }
            };
            var schedules = new[] { 
                new BackupSchedule { ScheduleId = 1, BackupDefinition = new BackupDefinition { KeepLast = 2 } },
                new BackupSchedule { ScheduleId = 2, BackupDefinition = new BackupDefinition { KeepLast = 0 } },
                new BackupSchedule { ScheduleId = 3, BackupDefinition = new BackupDefinition { KeepLast = 1 } },
            };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 2, Status = Status.Complete, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 3, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 3, Status = Status.Complete, BackupDirectory = "/backups/backup6"}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backupDataSet.Received(2).Remove(Arg.Any<Backup>());
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 3));
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 6));
            await mountPointBackupService.Received(2).DeleteDirectoryAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CleanupJobRemovesOldBackups()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 4 }
            };
            var schedules = new[] { schedule };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup6"}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backupDataSet.Received(2).Remove(Arg.Any<Backup>());
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 5));
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 6));
            await mountPointBackupService.Received(2).DeleteDirectoryAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task CleanupJobDoesNotDeleteFoldersThatAreNull()
        {
            var schedule = new BackupSchedule
            {
                ScheduleId = 1,
                BackupDefinition = new BackupDefinition { KeepLast = 5 }
            };
            var schedules = new[] { schedule };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, BackupDirectory = null}
            };
            var backupDataSet = backups.AsQueryable().BuildMockDbSet();


            var dataContext = Substitute.For<IDataContext>();
            dataContext.Schedules.Returns(scheduleDataSet);
            dataContext.Backups.Returns(backupDataSet);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.DeleteDirectoryAsync(Arg.Any<string>()).Returns(true);
            var logger = Substitute.For<ILogger<CleanOldBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<CleanOldBackupsJob>>(logger);

            var cleanupJob = new CleanOldBackupsJob(scopeFactory);
            await cleanupJob.Execute(CancellationToken.None);

            backupDataSet.Received(1).Remove(Arg.Any<Backup>());
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 6));
            await mountPointBackupService.DidNotReceive().DeleteDirectoryAsync(Arg.Any<string>());
        }
    }
}
