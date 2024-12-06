using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using Xunit;
using FluentAssertions;

using Booth.DockerVolumeBackup.Test.Fixtures;
using System.Net.Http.Json;
using Booth.DockerVolumeBackup.Application.Volumes.Dtos;
using FluentAssertions.Execution;


namespace Booth.DockerVolumeBackup.Test.Volumes
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class GetAllVolumesTests(WebApiFixture fixture)
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
