using System.Net.Http.Json;

using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetAllVolumes;
using Booth.DockerVolumeBackup.Application.Volumes.Queries.GetVolumeBackups;
using Booth.DockerVolumeBackup.Test.Fixtures;
using Booth.DockerVolumeBackup.Application.Backups.Common;
using System.Net;
using Bogus.DataSets;
using System.Net.Http;


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
            volumes.Should().HaveCount(10);
            using (var scope = new AssertionScope())
            {
                volumes?[0].Name.Should().NotBeEmpty();
                volumes?[0].Size.Should().BeGreaterThan(0);
                volumes?[0].LastBackup.Should().BeBefore(DateTimeOffset.UtcNow);
            }
        }

        [Fact]
        public async Task GetVolumeBackups()
        {
            var httpClient = fixture.CreateClient();

            var volumeName = "Jakob_Schuppe";
            var volumeBackups = await httpClient.GetFromJsonAsync<IReadOnlyList<VolumeBackupDto>>($"api/volumes/{volumeName}/backups", fixture.JsonSerializerOptions, TestContext.Current.CancellationToken);

            volumeBackups.Should().NotBeNull();
            volumeBackups.Should().HaveCount(79);
            using (var scope = new AssertionScope())
            {
                volumeBackups?[0].BackupId.Should().BeGreaterThan(0);
                volumeBackups?[0].ScheduleId.Should().BeGreaterThan(0);
                volumeBackups?[0].ScheduleName.Should().Be("Caleigh");
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
