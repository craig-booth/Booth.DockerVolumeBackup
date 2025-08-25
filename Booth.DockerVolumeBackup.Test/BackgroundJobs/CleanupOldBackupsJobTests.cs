using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using FluentAssertions;
using FluentAssertions.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using System.Security.Cryptography.Xml;
using Xunit;


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

            var today = new DateTimeOffset(DateTime.UtcNow.Date);   
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-6), BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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

            var today = new DateTimeOffset(DateTime.UtcNow.Date);
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Active, StartTime = today.AddDays(-6), BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Queued, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Error, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 3));      
            await mountPointBackupService.Received(1).DeleteDirectoryAsync("/backups/backup3");
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

            var today = new DateTimeOffset(DateTime.UtcNow.Date);
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-6), BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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
            var schedules = new[] { 
                new BackupSchedule { ScheduleId = 1, BackupDefinition = new BackupDefinition { KeepLast = 2 } },
                new BackupSchedule { ScheduleId = 2, BackupDefinition = new BackupDefinition { KeepLast = 0 } },
                new BackupSchedule { ScheduleId = 3, BackupDefinition = new BackupDefinition { KeepLast = 1 } },
            };
            var scheduleDataSet = schedules.AsQueryable().BuildMockDbSet();

            var today = new DateTimeOffset(DateTime.UtcNow.Date);
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-6), BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 2, Status = Status.Complete, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 3, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 3, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 1));
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 5));
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

            var today = new DateTimeOffset(DateTime.UtcNow.Date);
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-6), BackupDirectory = "/backups/backup1"},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 1));
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 2));
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

            var today = new DateTimeOffset(DateTime.UtcNow.Date);
            var backups = new[] {
                new Backup {BackupId = 1, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-6), BackupDirectory = null},
                new Backup {BackupId = 2, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-5), BackupDirectory = "/backups/backup2"},
                new Backup {BackupId = 3, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-4), BackupDirectory = "/backups/backup3"},
                new Backup {BackupId = 4, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-3), BackupDirectory = "/backups/backup4"},
                new Backup {BackupId = 5, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-2), BackupDirectory = "/backups/backup5"},
                new Backup {BackupId = 6, ScheduleId = 1, Status = Status.Complete, StartTime = today.AddDays(-1), BackupDirectory = "/backups/backup6"}
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
            backupDataSet.Received().Remove(Arg.Is<Backup>(x => x.BackupId == 1));
            await mountPointBackupService.DidNotReceive().DeleteDirectoryAsync(Arg.Any<string>());
        }
    }
}
