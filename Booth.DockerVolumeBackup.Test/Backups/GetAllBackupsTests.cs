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
using Booth.DockerVolumeBackup.Application.Backups.Dtos;
using FluentAssertions.Execution;


namespace Booth.DockerVolumeBackup.Test.Volumes
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class GetAllBackupsTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task GellAllBackups()
        {
            var httpClient = fixture.CreateClient();

            var backups = await httpClient.GetFromJsonAsync<IReadOnlyList<BackupDto>>("api/backups", TestContext.Current.CancellationToken);

            using (var scope = new AssertionScope())
            {
                backups.Should().NotBeNull();
                backups.Should().HaveCount(10);
                backups[0].BackupId.Should().Be(1);
                backups[0].ScheduleId.Should().Be(1);
                backups[0].Status.Should().Be(Status.Complete);
                backups[0].ScheduledTime.Should().BeBefore(DateTimeOffset.UtcNow);
                backups[0].StartTime.Should().BeBefore(DateTimeOffset.UtcNow);
                backups[0].EndTime.Should().BeBefore(DateTimeOffset.UtcNow);
                backups[0].Volumes.Should().HaveCount(2);
            }
        }
    }
}
