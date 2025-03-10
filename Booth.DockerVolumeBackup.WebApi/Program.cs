using System.Text.Json;
using System.Text.Json.Serialization;

using Booth.DockerVolumeBackup.Application;

using Booth.DockerVolumeBackup.WebApi.EndPoints;

namespace Booth.DockerVolumeBackup.WebApi
{
    public interface IApiMarker { }

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
            builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));
            builder.AddInfrastructure();
            builder.AddApplication();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthorization();

            app.AddVolumeEndPoints();
            app.AddBackupEndPoints();
            app.AddScheduleEndPoints();

            app.MapFallbackToFile("/index.html");

            // Configure application
            app.SetupScheduler();
            await app.SetupDatabase();
            await app.AddScheduledBackups();

            app.Run();
        }
    }
}
