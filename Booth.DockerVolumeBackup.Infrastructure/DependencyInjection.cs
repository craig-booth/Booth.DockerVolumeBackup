using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Sqlite;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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

        public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
        {      
            builder.Services.AddDbContext<DataContext>();
            builder.Services.AddScoped<IDataContext>(x => x.GetRequiredService<DataContext>());
            builder.Services.AddSingleton<IDockerClientFactory, DockerClientFactory>();
            builder.Services.AddSingleton<IDockerClient>((services) => services.GetRequiredService<IDockerClientFactory>().CreateClient());
            builder.Services.AddSingleton<IDockerService, DockerService>();
            builder.Services.AddTransient<IMountPointBackupService,MountPointBackupService>();


            return builder;
        }

        public static async Task SetupDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
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
