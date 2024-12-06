using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using Booth.DockerVolumeBackup.Application.Services;

namespace Booth.DockerVolumeBackup.Application
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IBackupNotificationService, BackupNotificationService>();
            services.AddHostedService<BackupBackgroundService>();

            var applicationAssembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(applicationAssembly);
            });

            return services;
        }
    }
}
