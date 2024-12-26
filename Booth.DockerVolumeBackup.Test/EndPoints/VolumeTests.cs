using System;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;

using Booth.DockerVolumeBackup.Test.Fixtures;
using Booth.DockerVolumeBackup.Application.Volumes.Dtos;

namespace Booth.DockerVolumeBackup.Test.EndPoints
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class VolumeTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task GellAllVolumes()
        {
            var httpClient = fixture.CreateClient();

            var volumes = await httpClient.GetFromJsonAsync<IReadOnlyList<VolumeDto>>("api/volumes", TestContext.Current.CancellationToken);

            volumes.Should().NotBeNull();
            volumes.Should().HaveCount(10);
            using (var scope = new AssertionScope())
            {
                volumes[0].Name.Should().NotBeEmpty();
                volumes[0].Size.Should().BeGreaterThan(0);
                volumes[0].LastBackup.Should().BeBefore(DateTimeOffset.UtcNow);
            }
        }
    }
}
