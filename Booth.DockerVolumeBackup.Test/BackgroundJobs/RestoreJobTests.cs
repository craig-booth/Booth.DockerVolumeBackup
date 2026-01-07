using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using Booth.DockerVolumeBackup.Infrastructure.Docker.Models;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using MediatR;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using Xunit;

namespace Booth.DockerVolumeBackup.Test.BackgroundJobs
{
    public class RestoreJobTests
    {
        [Fact]
        public async Task RestoreJobRestoresVolume()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                BackupDirectory = "/backups",
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6, BackupFile = "volume1.tar.gz", Status = Status.Complete },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7, BackupFile = "volume2.tar.gz", Status = Status.Complete },
                    }
            };
            var backups = new[] { backup };

            var volumes = new List<Application.Interfaces.Volume>
            {
                new Application.Interfaces.Volume
                {
                    Name = "Volume1",
                    MountPoint = "/data",
                    Size = 1000
                }
            };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            dockerService.GetVolumesAsync().Returns(volumes);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.RestoreDirectoryAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "Volume1", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            await mountPointBackupService.Received(1).RestoreDirectoryAsync("/data", Path.Combine("/backups", "volume2.tar.gz"));
        }

        [Fact]
        public async Task RestoreJobCreatesVolumeIfItDoesNotExist()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                BackupDirectory = "/backups",
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6, BackupFile = "volume1.tar.gz", Status = Status.Complete },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7, BackupFile = "volume2.tar.gz", Status = Status.Complete },
                    }
            };
            var backups = new[] { backup };

            var volumes = new List<Application.Interfaces.Volume>
            {
                new Application.Interfaces.Volume
                {
                    Name = "Volume1",
                    MountPoint = "/data",
                    Size = 1000
                }
            };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            dockerService.GetVolumesAsync().Returns(volumes);
            dockerService.CreateVolumeAsync(Arg.Any<string>()).Returns(new Application.Interfaces.Volume { Name = "NewVolume", MountPoint = "/data" });

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.RestoreDirectoryAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "NewVolume", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            await dockerService.Received(1).CreateVolumeAsync("NewVolume");
            await mountPointBackupService.Received(1).RestoreDirectoryAsync("/data", Path.Combine("/backups", "volume2.tar.gz"));
        }

        [Fact]
        public async Task RestoreJobLogsErrorWhenBackupNotFound()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6 },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7 },
                    }
            };
            var backups = new[] { backup };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(9999, "NewVolume", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), "Backup not found for Volume BackupId '9999'", Arg.Any<Exception>());
        }

            [Fact]
        public async Task RestoreJobLogsErrorWhenBackupDirectoryDoesNotExists()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6 },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7 },
                    }
            };
            var backups = new[] { backup };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "NewVolume", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            logger.Received(1).LogError("Cannot restore, the backup was not found");
        }

        [Fact]
        public async Task RestoreJobLogsErrorWhenBackupFailed()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                BackupDirectory = "/backups",
                Status = Status.Error,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6, BackupFile = "volume1.tar.gz", Status = Status.Complete },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7, BackupFile = "volume2.tar.gz", Status = Status.Error },
                    }
            };
            var backups = new[] { backup };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "NewVolume", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            logger.Received(1).LogError("Cannot restore, the backup was not completed successfully");
        }

        [Fact]
        public async Task RestoreJobLogsErrorWhenCreatingVolumeFails()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                BackupDirectory = "/backups",
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6, BackupFile = "volume1.tar.gz", Status = Status.Complete },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7, BackupFile = "volume2.tar.gz", Status = Status.Complete },
                    }
            };
            var backups = new[] { backup };

            var volumes = new List<Application.Interfaces.Volume>
            {
                new Application.Interfaces.Volume
                {
                    Name = "Volume1",
                    MountPoint = "/data",
                    Size = 1000
                }
            };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            dockerService.GetVolumesAsync().Returns(volumes);
            dockerService.CreateVolumeAsync(Arg.Any<string>()).Returns(default(Application.Interfaces.Volume));

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "NewVolume", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            logger.Received(1).Log(LogLevel.Error, Arg.Any<EventId>(), "An error occured when creating volume NewVolume", Arg.Any<Exception>());
        }


        [Fact]
        public async Task RestoreJobLogsErrorWhenRestoreFails()
        {
            // Arrange
            var publisher = Substitute.For<IPublisher>();

            var backup = new Backup
            {
                BackupId = 45,
                BackupDirectory = "/backups",
                Status = Status.Complete,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1", BackupVolumeId = 6, BackupFile = "volume1.tar.gz", Status = Status.Complete },
                        new BackupVolume { Volume = "Volume2", BackupVolumeId = 7, BackupFile = "volume2.tar.gz", Status = Status.Complete },
                    }
            };
            var backups = new[] { backup };

            var volumes = new List<Application.Interfaces.Volume>
            {
                new Application.Interfaces.Volume
                {
                    Name = "Volume1",
                    MountPoint = "/data",
                    Size = 1000
                }
            };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();
            dockerService.GetVolumesAsync().Returns(volumes);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.RestoreDirectoryAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var logger = Substitute.For<LoggerMock<RestoreJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<ILogger<RestoreJob>>(logger);

            // Act
            var restoreJob = new RestoreJob(7, "Volume1", scopeFactory);
            await restoreJob.Execute(CancellationToken.None);

            // Assert
            logger.Received(1).LogError("Restore failed");
        }
    }
}
