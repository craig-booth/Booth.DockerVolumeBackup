using System.Net;

using Xunit;
using FluentAssertions;

using Booth.DockerVolumeBackup.Test.Fixtures;


namespace Booth.DockerVolumeBackup.Test.EndPoints
{

    [Collection(nameof(WebApiFixtureCollection))]
    public class VolumeBackupTests(WebApiFixture fixture)
    {

        [Fact]
        public async Task DownloadVolumeBackup_VolumeDoesNotExist()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/volumebackups/999999/download", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DownloadVolumeBackup_Success()
        {
            var httpClient = fixture.CreateClient();

            var response = await httpClient.GetAsync("api/volumebackups/1/download", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/octet-stream");
            response.Content.Headers.ContentDisposition?.FileName.Should().Be("Jakob_Schuppe.tar.gz");

            var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            var textReader = new StreamReader(stream);
            var content = await textReader.ReadToEndAsync(TestContext.Current.CancellationToken);
            content.Should().Be("Test Data");
        }

    }
}
