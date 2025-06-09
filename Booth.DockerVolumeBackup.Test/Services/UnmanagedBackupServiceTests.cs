using Xunit;
using NSubstitute;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.Services;
using Booth.DockerVolumeBackup.Domain.Models;

namespace Booth.DockerVolumeBackup.Test.Services
{
    public class UnmanagedBackupServiceTests
    {

        [Fact]
        public async Task GetBackupsAsync_ReturnsEmptyList_WhenNoBackupsExist()
        {
            // Arrange
            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(new List<BackupDirectoryInfo>());

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups = await service.GetBackupsAsync();

            // Assert
            using (new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().BeEmpty();
            }
        }

        [Fact] 
        public async Task GetBackupsAsync_ReturnsListOfBackups_WhenBackupsExist()
        {
            // Arrange
            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("backup2", new DateTime(2005, 01, 10))
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

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups = await service.GetBackupsAsync();

           
            // Assert
            using (new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCount(2);
                backups[0].BackupDirectory.Should().Be("backup1");
                backups[0].StartTime.Should().Be(new DateTime(2005, 01, 01));
                backups[0].EndTime.Should().Be(new DateTime(2005, 01, 01));
                backups[0].Status.Should().Be(Status.Complete);
                backups[0].BackupId.Should().Be(-1);

                backups[1].BackupDirectory.Should().Be("backup2");
                backups[1].StartTime.Should().Be(new DateTime(2005, 01, 10));
                backups[1].EndTime.Should().Be(new DateTime(2005, 01, 10));
                backups[1].Status.Should().Be(Status.Complete);
                backups[1].BackupId.Should().Be(-2);
            }
        }

        [Fact]
        public async Task GetBackupsAsync_ReturnsCorrectId_WhenNewDirectoryFound()
        {
            // Arrange
            var backupDirectories1 = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("backup2", new DateTime(2005, 01, 10))
            };
            var backupDirectories2 = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("backup3", new DateTime(2005, 01, 20)),
                new BackupDirectoryInfo("backup2", new DateTime(2005, 01, 10))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories1, backupDirectories2);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>
                {
                    new BackupFileInfo("volume1.tar.gz", 2000, new DateTime(2010, 01, 02)),
                    new BackupFileInfo("volume2.tar.gz", 3000, new DateTime(2010, 01, 03))
                });

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups1 = await service.GetBackupsAsync(); 
            var backups2 = await service.GetBackupsAsync();

            // Assert
            using (new AssertionScope())
            {
                backups1.Should().HaveCount(2);
                backups2.Should().HaveCount(3);

                backups2.Should().Contain(x => x.BackupDirectory == "backup1" && x.BackupId == -1);
                backups2.Should().Contain(x => x.BackupDirectory == "backup2" && x.BackupId == -2);
                backups2.Should().Contain(x => x.BackupDirectory == "backup3" && x.BackupId == -3);
            }
        }

        [Fact]
        public async Task GetBackupsAsync_ReturnsCorrectId_WhenDirectoryDeleted()
        {
            // Arrange
            var backupDirectories1 = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("backup2", new DateTime(2005, 01, 10)),
                new BackupDirectoryInfo("backup3", new DateTime(2005, 01, 20))
            };
            var backupDirectories2 = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01)),
                new BackupDirectoryInfo("backup3", new DateTime(2005, 01, 20))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories1, backupDirectories2);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>
                {
                    new BackupFileInfo("volume1.tar.gz", 2000, new DateTime(2010, 01, 02)),
                    new BackupFileInfo("volume2.tar.gz", 3000, new DateTime(2010, 01, 03))
                });

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups1 = await service.GetBackupsAsync();
            var backups2 = await service.GetBackupsAsync();

            // Assert
            using (new AssertionScope())
            {
                backups1.Should().HaveCount(3);
                backups2.Should().HaveCount(2);

                backups2.Should().Contain(x => x.BackupDirectory == "backup1" && x.BackupId == -1);
                backups2.Should().Contain(x => x.BackupDirectory == "backup3" && x.BackupId == -3);
            }
        }

        [Fact]
        public async Task GetBackupsAsync_ReturnsVolumes_WhenBackupsExist()
        {
            // Arrange
            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01))
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

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups = await service.GetBackupsAsync();


            // Assert
            using (new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCount(1);
                backups[0].Volumes.Should().HaveCount(2);

                backups[0].Volumes[0].Volume.Should().Be("volume1");
                backups[0].Volumes[0].StartTime.Should().Be(new DateTime(2010, 01, 02));
                backups[0].Volumes[0].Status.Should().Be(Status.Complete);
                backups[0].Volumes[0].BackupFile.Should().Be("volume1.tar.gz");
                backups[0].Volumes[0].BackupSize.Should().Be(2000);

                backups[0].Volumes[1].Volume.Should().Be("volume2");
                backups[0].Volumes[1].StartTime.Should().Be(new DateTime(2010, 01, 03));
                backups[0].Volumes[1].Status.Should().Be(Status.Complete);
                backups[0].Volumes[1].BackupFile.Should().Be("volume2.tar.gz");
                backups[0].Volumes[1].BackupSize.Should().Be(3000);
            }

        }

        [Fact]
        public async Task GetBackupsAsync_DoesNotReturnBackup_WhenNoBackupsExist()
        {
            // Arrange
            var backupDirectories = new List<BackupDirectoryInfo>
            {
                new BackupDirectoryInfo("backup1", new DateTime(2005, 01, 01))
            };

            var mountPointService = Substitute.For<IMountPointBackupService>();
            mountPointService.GetBackupDirectoriesAsync(Arg.Any<string>())
                .Returns(backupDirectories);
            mountPointService.GetBackupFilesAsync(Arg.Any<string>())
                .Returns(new List<BackupFileInfo>());

            var service = new UnmanagedBackupService(mountPointService);

            // Act
            var backups = await service.GetBackupsAsync();


            // Assert
            using (new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCount(0);
            }
        }

    }
}
