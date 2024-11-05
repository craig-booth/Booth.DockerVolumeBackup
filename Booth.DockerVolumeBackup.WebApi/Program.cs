using System.Text.Json;
using System.Text.Json.Serialization;
using Booth.Docker;
using Booth.DockerVolumeBackup.WebApi.Backup;
using Booth.DockerVolumeBackup.WebApi.DataProviders;
using Booth.DockerVolumeBackup.WebApi.EndPoints;
using Booth.DockerVolumeBackup.WebApi.Models;
using Booth.DockerVolumeBackup.WebApi.Services;

namespace Booth.DockerVolumeBackup.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();

            // Set JSON conversion options
            builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // Add services to the container.
            builder.Services.AddSingleton<IDockerClient>(DockerClientFactory.CreateClient("http://192.168.1.11:2375"));
            builder.Services.AddSingleton<IDataContext, DataContext>();
            builder.Services.AddSingleton<IBackupNotificationService, BackupNotificationService>();
            builder.Services.AddSingleton<IBackupDataProvider, BackupDataProvider>();
            builder.Services.AddSingleton<VolumeService>();
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddHostedService<BackupBackgroundService>();

            var app = builder.Build();

            // Setup Database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
                await SetupDatabase.CreateDatabase(context);
            }

            // Configure the HTTP request pipeline.
            app.UseAuthorization();
            app.AddVolumeEndPoints();
            app.AddBackupEndPoints();


            app.Run();
        }
    }
}
