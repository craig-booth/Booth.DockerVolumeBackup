using System.Net.Http.Json;

using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetAllVolumes;
using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolumeBackups;
using Booth.DockerVolumeBackup.Test.Fixtures;
using Booth.DockerVolumeBackup.Application.Backups.Common;


namespace Booth.DockerVolumeBackup.Test.EndPoints
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class VolumeTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task GetAllVolumes()
        {
            var httpClient = fixture.CreateClient();

            var volumes = await httpClient.GetFromJsonAsync<IReadOnlyList<VolumeDto>>("api/volumes", TestContext.Current.CancellationToken);

            volumes.Should().NotBeNull();
            volumes.Should().HaveCount(6);
            using (var scope = new AssertionScope())
            {
                volumes?[0].Name.Should().Be("service2_volume1");
                volumes?[0].Size.Should().Be(635295);
                volumes?[0].LastBackup?.Date.Should().BeWithin(TimeSpan.FromDays(7)).Before(DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task GetVolume()
        {
            var httpClient = fixture.CreateClient();

            var volume = await httpClient.GetFromJsonAsync<VolumeDto>("api/volumes/service2_volume1", TestContext.Current.CancellationToken);

            volume.Should().NotBeNull();
            using (var scope = new AssertionScope())
            {
                volume.Name.Should().Be("service2_volume1");
                volume.Size.Should().Be(635295);
                volume.LastBackup?.Date.Should().BeWithin(TimeSpan.FromDays(7)).Before(DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task GetVolumeNotFound()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/volumes/xxxxxx", TestContext.Current.CancellationToken);
            response.Should().Be404NotFound();
        }

        [Fact]
        public async Task GetVolumeBackups()
        {
            var httpClient = fixture.CreateClient();

            var volumeName = "service2_volume1";
            var volumeBackups = await httpClient.GetFromJsonAsync<IReadOnlyList<VolumeBackupDto>>($"api/volumes/{volumeName}/backups", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            volumeBackups.Should().NotBeNull();
            volumeBackups.Should().NotBeEmpty();
            using (var scope = new AssertionScope())
            {
                volumeBackups?[0].BackupId.Should().BeGreaterThan(0);
                volumeBackups?[0].ScheduleId.Should().BeGreaterThan(0);
                volumeBackups?[0].ScheduleName.Should().Be("ErgonomicFish");
                volumeBackups?[0].Status.Should().Be(StatusDto.Complete);
                volumeBackups?[0].BackupTime.Should().BeBefore(DateTimeOffset.UtcNow);
            }
        }

        [Fact]
        public async Task GetVolumeBackupsNotFound()
        {
            var httpClient = fixture.CreateClient();

            var volumeName = "abc";
            var volumeBackups = await httpClient.GetFromJsonAsync<IReadOnlyList<VolumeBackupDto>>($"api/volumes/{volumeName}/backups", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            volumeBackups.Should().NotBeNull();
            volumeBackups.Should().HaveCount(0);
        }
    }
}
