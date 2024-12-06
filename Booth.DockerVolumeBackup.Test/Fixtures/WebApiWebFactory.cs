
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Booth.DockerVolumeBackup.WebApi;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;

namespace Booth.DockerVolumeBackup.Test.Fixtures
{
    public class WebApiWebFactory : WebApplicationFactory<IApiMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
 
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDockerClient>(DockerClientMock.CreateMock());
            });
        }
    }
}
