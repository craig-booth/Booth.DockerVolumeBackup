using System.Text.Json;
using System.Text.Json.Serialization;

using Booth.DockerVolumeBackup.Application;

using Booth.DockerVolumeBackup.WebApi.EndPoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
            builder.Services.AddInfrastructure();
            builder.Services.AddApplication();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthorization();

            app.AddVolumeEndPoints();
            app.AddBackupEndPoints();

            app.MapFallbackToFile("/index.html");

            // Setup database
            await app.SetupDatabase();

            app.Run();
        }
    }
}
