using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Domain.Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using Coravel.Scheduling.Schedule;

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

            var backupDataSet = Substitute.For<DbSet<Backup>>();
            backupDataSet.FindAsync(Arg.Any<object[]>(), Arg.Any<CancellationToken>()).Returns(backup);

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
        }

    }
}
