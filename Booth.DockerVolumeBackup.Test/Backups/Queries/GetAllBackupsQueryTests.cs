using Booth.DockerVolumeBackup.Application.Backups.Common;
using Booth.DockerVolumeBackup.Application.Backups.Queries.GetAllBackups;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Booth.DockerVolumeBackup.Test.Backups.Queries
{
    public class GetAllBackupsQueryTests
    {

        [Fact]
        public async Task GetAllBackupsQuery_ReturnsUnmanagedBackups()
        {
            var managedBackups = new List<Backup>();
            var managedBackupsMock = managedBackups.BuildMock();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.AsQueryable().Returns(managedBackupsMock);

            var unmanagedBackups = new List<Backup>
            {
                new Backup
                {
                    BackupId = -1,
                    Status = Status.Complete,
                    BackupDirectory = "unmanaged-backup-1",
                },
                new Backup
                {
                    BackupId = -2,
                    Status = Status.Complete,
                    BackupDirectory = "unmanaged-backup-2",
                }
            };
            var unmanagedBackupService = Substitute.For<IUnmanagedBackupService>();
            unmanagedBackupService.GetBackupsAsync(Arg.Any<CancellationToken>()).Returns(unmanagedBackups);

            var queryHandler = new GetAllBackupsQueryHandler(dataContext, unmanagedBackupService);

            var backups = await queryHandler.Handle(new GetAllBackupsQuery(), CancellationToken.None);

            {
                backups.IsError.Should().BeFalse();
                backups.Value.Should().HaveCount(2);
                backups.Value.Should().Contain(x => x.BackupId == -1 && x.BackupType == BackupTypeDto.Unmanaged);
                backups.Value.Should().Contain(x => x.BackupId == -2 && x.BackupType == BackupTypeDto.Unmanaged);
            }
        }

        [Fact]
        public async Task GetAllBackupsQuery_IgnoresUnmanagedBackupsWithSameNameAsManagedBackup()
        {
            var managedBackups = new List<Backup>()
                 {
                new Backup
                {
                    BackupId = 1,
                    Status = Status.Complete,
                    ScheduleId = 1,
                    Schedule = new BackupSchedule { Name = "Test Schedule" },
                    BackupDirectory = "managed-backup-1",
                },
                new Backup
                {
                    BackupId = 2,
                    Status = Status.Complete,
                    ScheduleId = 1,
                    Schedule = new BackupSchedule { Name = "Test Schedule" },
                    BackupDirectory = "managed-backup-2",
                }
            };
            var managedBackupsMock = managedBackups.BuildMock();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.AsQueryable().Returns(managedBackupsMock);

            var unmanagedBackups = new List<Backup>
            {
                new Backup
                {
                    BackupId = -1,
                    Status = Status.Complete,
                    BackupDirectory = "managed-backup-1",
                },
                new Backup
                {
                    BackupId = -2,
                    Status = Status.Complete,
                    BackupDirectory = "managed-backup-2",
                },
                new Backup
                {
                    BackupId = -3,
                    Status = Status.Complete,
                    BackupDirectory = "unmanaged-backup-1",
                }
            };
            var unmanagedBackupService = Substitute.For<IUnmanagedBackupService>();
            unmanagedBackupService.GetBackupsAsync(Arg.Any<CancellationToken>()).Returns(unmanagedBackups);

            var queryHandler = new GetAllBackupsQueryHandler(dataContext, unmanagedBackupService);

            var backups = await queryHandler.Handle(new GetAllBackupsQuery(), CancellationToken.None);

            using (var scope = new AssertionScope())
            {
                backups.IsError.Should().BeFalse();
                backups.Value.Should().HaveCount(3);
                backups.Value.Should().Contain(x => x.BackupId == 1 && x.BackupType == BackupTypeDto.Scheduled);
                backups.Value.Should().Contain(x => x.BackupId == 2 && x.BackupType == BackupTypeDto.Scheduled);
                backups.Value.Should().Contain(x => x.BackupId == -3 && x.BackupType == BackupTypeDto.Unmanaged);
            }
        }

    }
}
