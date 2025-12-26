using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;
namespace Booth.DockerVolumeBackup.Test.BackgroundJobs
{
    public class LoadUnmanagedBackupsJobTest
    {
        [Fact]
        public async Task LoadUnmanagedBackupsJob_LoadsBackups()
        {
            var managedBackups = new List<Backup>();
            var managedBackupsDataSet = managedBackups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(managedBackupsDataSet);

            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("unmanaged-backup-1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("unmanaged-backup-2", new DateTime(2005, 01, 10))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>
                {
                    new BackupFileInfo("volume1.tar.gz", 2000, new DateTime(2010, 01, 02)),
                    new BackupFileInfo("volume2.tar.gz", 3000, new DateTime(2010, 01, 03))
                });

            var logger = Substitute.For<ILogger<LoadUnmanagedBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointService);
            scopeFactory.RegisterService<ILogger<LoadUnmanagedBackupsJob>>(logger);

            var loadBackupsJob = new LoadUnmanagedBackupsJob(scopeFactory);
            await loadBackupsJob.Execute(CancellationToken.None);

            dataContext.Backups.Received(2).Add(Arg.Any<Backup>());
            dataContext.Backups.Received(1).Add(Arg.Is<Backup>(x => x.BackupDirectory == "/backup/unmanaged-backup-1"));
            dataContext.Backups.Received(1).Add(Arg.Is<Backup>(x => x.BackupDirectory == "/backup/unmanaged-backup-2"));
            await dataContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        }

        [Fact]
        public async Task LoadUnmanagedBackupsJob_IgnoresEmptyFolders()
        {
            var managedBackups = new List<Backup>();
            var managedBackupsDataSet = managedBackups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(managedBackupsDataSet);

            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("unmanaged-backup-1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("unmanaged-backup-2", new DateTime(2005, 01, 10))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>());

            var logger = Substitute.For<ILogger<LoadUnmanagedBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointService);
            scopeFactory.RegisterService<ILogger<LoadUnmanagedBackupsJob>>(logger);

            var loadBackupsJob = new LoadUnmanagedBackupsJob(scopeFactory);
            await loadBackupsJob.Execute(CancellationToken.None);

            dataContext.Backups.Received(0).Add(Arg.Any<Backup>());
        }


        [Fact]
        public async Task LoadUnmanagedBackupsJob_IgnoresUnmanagedBackupsWithSameNameAsManagedBackup()
        {
            var managedBackups = new List<Backup>()
                 {
                new Backup
                {
                    BackupId = 1,
                    Status = Status.Complete,
                    ScheduleId = 1,
                    Schedule = new BackupSchedule { Name = "Test Schedule" },
                    BackupDirectory = "/backup/managed-backup-1",
                },
                new Backup
                {
                    BackupId = 2,
                    Status = Status.Complete,
                    ScheduleId = 1,
                    Schedule = new BackupSchedule { Name = "Test Schedule" },
                    BackupDirectory = "/backup/managed-backup-2",
                }
            };
            var managedBackupsDataSet = managedBackups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(managedBackupsDataSet);
            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("managed-backup-1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("managed-backup-2", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("unmanaged-backup-1", new DateTime(2005, 01, 10))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>
                {
                    new BackupFileInfo("volume1.tar.gz", 2000, new DateTime(2010, 01, 02)),
                    new BackupFileInfo("volume2.tar.gz", 3000, new DateTime(2010, 01, 03))
                });

            var logger = Substitute.For<ILogger<LoadUnmanagedBackupsJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointService);
            scopeFactory.RegisterService<ILogger<LoadUnmanagedBackupsJob>>(logger);

            var loadBackupsJob = new LoadUnmanagedBackupsJob(scopeFactory);
            await loadBackupsJob.Execute(CancellationToken.None);

            dataContext.Backups.Received(1).Add(Arg.Any<Backup>());
            dataContext.Backups.Received(1).Add(Arg.Is<Backup>(x => x.BackupDirectory == "/backup/unmanaged-backup-1"));
            await dataContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

    }
}
