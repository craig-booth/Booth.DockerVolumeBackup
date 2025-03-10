
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using Coravel;

using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Infrastructure.Docker;
using Booth.DockerVolumeBackup.Infrastructure.Database;
using Booth.DockerVolumeBackup.Infrastructure.Services;
using Booth.DockerVolumeBackup.Infrastructure.Scheduler;
using System.Runtime.CompilerServices;
using Coravel.Scheduling.Schedule.Interfaces;


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
            builder.Services.AddScoped<IBackupScheduler, BackupScheduler>();
            builder.Services.AddTransient<IMountPointBackupService,MountPointBackupService>();

            builder.Services.AddQueue();
            builder.Services.AddScheduler();

            return builder;
        }

        public static void SetupScheduler(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var schedulerConfiguration = scope.ServiceProvider.GetRequiredService<ISchedulerConfiguration>();

                schedulerConfiguration.LogScheduledTaskProgress();
            }
        }

        public static async Task SetupDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.Migrate();

                var config = scope.ServiceProvider.GetRequiredService<IOptions<AppConfig>>();
                if (config.Value.SeedDatabase)
                {
                    var dockerClient = scope.ServiceProvider.GetRequiredService<IDockerClient>();

                    var databaseSetup = new DatabaseSetup(context, dockerClient);
                    await databaseSetup.SeedDatabase();
                }
            }
        }

    }
}
