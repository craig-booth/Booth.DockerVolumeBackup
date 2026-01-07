using Xunit;
using FluentAssertions;

using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using Booth.DockerVolumeBackup.Infrastructure.Services;
using Booth.DockerVolumeBackup.Application.Interfaces;

namespace Booth.DockerVolumeBackup.Test.Services
{
    public class DockerServiceTests
    {


        [Fact]
        public async Task GetVolumesAsyncReturnsVolumes()
        {
            // Arrange
            var dockerFactoryMock = new DockerFactoryMock();
            var dockerService = new DockerService(dockerFactoryMock.CreateClient());

            // Act
            var volumes = await dockerService.GetVolumesAsync();

            // Assert
            volumes.Should().NotBeNull();
            volumes.Should().HaveCount(7);
        }

        [Fact]
        public async Task GetDependentServicesReturnsServices()
        {
            // Arrange
            var dockerFactoryMock = new DockerFactoryMock();
            var dockerService = new DockerService(dockerFactoryMock.CreateClient());

            // Act
            var volumes = new Volume[]
            {
                new () { Name = "service2_volume1", MountPoint = "" },
                new () { Name = "shared_volume1", MountPoint = "" }
            };
            var services = await dockerService.GetDependentServicesAsync(volumes);

            // Assert
            services.Should().NotBeNull();
            services.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetDependentVolumesAsyncReturnsVolume()
        {
            // Arrange
            var dockerFactoryMock = new DockerFactoryMock();
            var dockerService = new DockerService(dockerFactoryMock.CreateClient());

            // Act
            var volumes = await dockerService.GetDependentVolumesAsync();

            // Assert
            volumes.Should().NotBeNull();
            volumes.Should().HaveCount(1);
            volumes[0].Should().Be("dockervolumebackup_database");
        }

        [Fact]
        public async Task StopAndStartServicesAsync()
        {
            List<string> messages = new List<string>();

            // Arrange
            var dockerFactoryMock = new DockerFactoryMock();
            var dockerService = new DockerService(dockerFactoryMock.CreateClient());

            dockerFactoryMock.OnMessageHandlerEvent += (s, e) =>
            {
                messages.Add(e.Message);
            };

            // Act
            var services = new Service[]
            {
                new () { Id = "1ol06jd41lyrl8auilb947gn7", Replicas = 1},
                new () { Id = "9auv05nnkq0avccax9q1kxhrh", Replicas = 2 }
            };
            await dockerService.StopServicesAsync(services, CancellationToken.None);
            messages.Should().Contain(new[]
            {
                "Service 1ol06jd41lyrl8auilb947gn7 scaled to 0 replicas.",
                "Service 9auv05nnkq0avccax9q1kxhrh scaled to 0 replicas."
            });

            messages.Clear();
            await dockerService.StartServicesAsync(services, CancellationToken.None);
            messages.Should().Contain(new[]
            {
                "Service 1ol06jd41lyrl8auilb947gn7 scaled to 1 replicas.",
                "Service 9auv05nnkq0avccax9q1kxhrh scaled to 2 replicas."
            });
        }

        [Fact]
        public async Task CreateVolume()
        {
            // Arrange
            var dockerFactoryMock = new DockerFactoryMock();
            var dockerService = new DockerService(dockerFactoryMock.CreateClient());

            // Act
            var volume = await dockerService.CreateVolumeAsync("NewVolume");

            volume.Should().NotBeNull();
            volume.Name.Should().Be("NewVolume");
        }
    }
}
