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
    public class BackupJobTests
    {
        [Fact]
        public async Task BackupJobPerformsBackup()
        {
            var backup = new Backup
            {
                BackupId = 45,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1" },
                        new BackupVolume { Volume = "Volume2" },
                    }
            };
            var backups = new[] { backup };
                
            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();  
            dataContext.Backups.Returns(backupDataSet);
                
            var dockerService = Substitute.For<IDockerService>();

            var volumes = new List<Volume> 
            {
                new Volume { Name = "Volume1", MountPoint = "/data/volume1" },
                new Volume { Name = "Volume2", MountPoint = "/data/volume2" },
            };
            dockerService.GetVolumesAsync().Returns(volumes);

            var services = new List<Service>
            { 
                new Service { Id = "Service1", Replicas = 1 },
                new Service { Id = "Service2", Replicas = 1 },
                new Service { Id = "Service3", Replicas = 1 },
            };
            dockerService.GetDependentServices(Arg.Any<IEnumerable<Volume>>()).Returns(services); 

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.CreateDirectoryAsync(Arg.Any<string>()).Returns(true);
            mountPointBackupService.BackupDirectoryAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(1000); 

            var publisher = Substitute.For<IPublisher>();
            var logger = Substitute.For<ILogger<BackupJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<IPublisher>(publisher);
            scopeFactory.RegisterService<ILogger<BackupJob>>(logger);

            var backupJob = new BackupJob(backup.BackupId, scopeFactory);
            await backupJob.Execute(CancellationToken.None);

            // Check services are called to perform backup
            await dockerService.Received().StopServices(Arg.Is<IEnumerable<Service>>(x => x.Count() == 3), Arg.Any<CancellationToken>());

            await mountPointBackupService.Received().CreateDirectoryAsync($"/backup/{DateTime.Now.ToString("yyyy-MM-dd")}_{backup.BackupId}");
            await mountPointBackupService.Received().BackupDirectoryAsync(volumes[0].MountPoint, Arg.Any<string>());
            await mountPointBackupService.Received().BackupDirectoryAsync(volumes[1].MountPoint, Arg.Any<string>());

            await dockerService.Received().StartServices(Arg.Is<IEnumerable<Service>>(x => x.Count() == 3), Arg.Any<CancellationToken>());
        
            backup.Status.Should().Be(Status.Complete);
            backup.Volumes.Should().AllSatisfy(x => x.Status.Should().Be(Status.Complete));
            backup.Volumes.Should().AllSatisfy(x => x.BackupSize.Should().Be(1000));
        }


        [Fact]
        public async Task BackupFailsIfDirectoryNotCreated()
        {
            var backup = new Backup
            {
                BackupId = 45,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1" },
                        new BackupVolume { Volume = "Volume2" },
                    }
            };
            var backups = new[] { backup };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();

            var volumes = new List<Volume>
            {
                new Volume { Name = "Volume1", MountPoint = "/data/volume1" },
                new Volume { Name = "Volume2", MountPoint = "/data/volume2" },
            };
            dockerService.GetVolumesAsync().Returns(volumes);

            var services = new List<Service>
            {
                new Service { Id = "Service1", Replicas = 1 },
                new Service { Id = "Service2", Replicas = 1 },
                new Service { Id = "Service3", Replicas = 1 },
            };
            dockerService.GetDependentServices(Arg.Any<IEnumerable<Volume>>()).Returns(services);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.CreateDirectoryAsync(Arg.Any<string>()).Returns(false); // Simulate failure to create directory
            var publisher = Substitute.For<IPublisher>();
            var logger = Substitute.For<ILogger<BackupJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<IPublisher>(publisher);
            scopeFactory.RegisterService<ILogger<BackupJob>>(logger);

            var backupJob = new BackupJob(backup.BackupId, scopeFactory);
            await backupJob.Execute(CancellationToken.None);

            backup.Status.Should().Be(Status.Error);
            backup.Volumes.Should().AllSatisfy(x => x.Status.Should().Be(Status.Queued));
        }

        [Fact]
        public async Task BackupFailsIfOneVolumeFails()
        {
            var backup = new Backup
            {
                BackupId = 45,
                Volumes = new List<BackupVolume>
                    {
                        new BackupVolume { Volume = "Volume1" },
                        new BackupVolume { Volume = "Volume2" },
                    }
            };
            var backups = new[] { backup };

            var backupDataSet = backups.BuildMockDbSet();

            var dataContext = Substitute.For<IDataContext>();
            dataContext.Backups.Returns(backupDataSet);

            var dockerService = Substitute.For<IDockerService>();

            var volumes = new List<Volume>
            {
                new Volume { Name = "Volume1", MountPoint = "/data/volume1" },
                new Volume { Name = "Volume2", MountPoint = "/data/volume2" },
            };
            dockerService.GetVolumesAsync().Returns(volumes);

            var services = new List<Service>
            {
                new Service { Id = "Service1", Replicas = 1 },
                new Service { Id = "Service2", Replicas = 1 },
                new Service { Id = "Service3", Replicas = 1 },
            };
            dockerService.GetDependentServices(Arg.Any<IEnumerable<Volume>>()).Returns(services);

            var mountPointBackupService = Substitute.For<IMountPointBackupService>();
            mountPointBackupService.CreateDirectoryAsync(Arg.Any<string>()).Returns(true);
            mountPointBackupService.BackupDirectoryAsync(volumes[0].MountPoint, Arg.Any<string>()).Returns(1000);
            mountPointBackupService.BackupDirectoryAsync(volumes[1].MountPoint, Arg.Any<string>()).Returns(0);

            var publisher = Substitute.For<IPublisher>();
            var logger = Substitute.For<ILogger<BackupJob>>();

            var scopeFactory = new ServiceScopeFactoryMock();
            scopeFactory.RegisterService<IDataContext>(dataContext);
            scopeFactory.RegisterService<IDockerService>(dockerService);
            scopeFactory.RegisterService<IMountPointBackupService>(mountPointBackupService);
            scopeFactory.RegisterService<IPublisher>(publisher);
            scopeFactory.RegisterService<ILogger<BackupJob>>(logger);

            var backupJob = new BackupJob(backup.BackupId, scopeFactory);
            await backupJob.Execute(CancellationToken.None);

            backup.Status.Should().Be(Status.Error);
            backup.Volumes[0].Status.Should().Be(Status.Complete);
            backup.Volumes[0].BackupSize.Should().Be(1000);
            backup.Volumes[1].Status.Should().Be(Status.Error);
            backup.Volumes[1].BackupSize.Should().Be(0);

            // Check services are restarted
            await dockerService.Received().StartServices(Arg.Is<IEnumerable<Service>>(x => x.Count() == 3), Arg.Any<CancellationToken>());
        }

    }
}
