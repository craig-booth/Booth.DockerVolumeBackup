using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Database;

namespace Booth.DockerVolumeBackup.Application
{

    public class AppConfig
    {
        public string DatabaseConnectionString { get; set; } = "";
        public string DockerUrI { get; set; } = "";
        public bool SeedDatabase { get; set; }
    }

    public static class DependencyInjection
    {

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDockerClientFactory, DockerClientFactory>();
            services.AddSingleton<IDockerClient>((services) => services.GetRequiredService<IDockerClientFactory>().CreateClient());
            services.AddSingleton<IDataContext, DataContext>();

            return services;
        }

        public static async Task SetupDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var dockerClient = scope.ServiceProvider.GetRequiredService<IDockerClient>();
                var config = scope.ServiceProvider.GetRequiredService<IOptions<AppConfig>>();

                var databaseSetup = new DatabaseSetup(context, dockerClient);

                await databaseSetup.CreateDatabase();

                if (config.Value.SeedDatabase)
                {
                    await databaseSetup.SeedDatabase();
                }
            }
        }

    }
}
