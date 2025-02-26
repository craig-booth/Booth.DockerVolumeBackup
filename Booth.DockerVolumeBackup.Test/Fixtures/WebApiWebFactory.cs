
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Booth.DockerVolumeBackup.WebApi;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Test.Fixtures.Mocks;
using Booth.DockerVolumeBackup.Application;
using Booth.DockerVolumeBackup.Application.Interfaces;
using System.Data;

namespace Booth.DockerVolumeBackup.Test.Fixtures
{
    public class WebApiWebFactory : WebApplicationFactory<IApiMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
 
            builder.ConfigureTestServices(services =>
            {
                services.Configure<AppConfig>(x =>
                {
                    x.DatabaseConnectionString = "DataSource=\"file::memory:?cache=shared\"";
                    x.SeedDatabase = true;
                });
                services.AddSingleton<IDockerClientFactory, DockerFactoryMock>();
                services.RemoveAll<IHostedService>();
            });
        }
    }
}
