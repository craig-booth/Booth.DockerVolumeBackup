using System;
using Microsoft.Extensions.DependencyInjection;


using MediatR;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Services;
using Booth.DockerVolumeBackup.Application.Behavoirs;

namespace Booth.DockerVolumeBackup.Application
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IBackupNotificationService, BackupNotificationService>();
            services.AddTransient<IBackupService, BackupService>();
            services.AddHostedService<BackupBackgroundService>();

            var applicationAssembly = typeof(DependencyInjection).Assembly;
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(applicationAssembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Transient, includeInternalTypes: true);


            return services;
        }
    }
}
