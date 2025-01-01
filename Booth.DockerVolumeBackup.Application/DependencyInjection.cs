using System;
using Microsoft.Extensions.DependencyInjection;


using MediatR;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Services;
using Booth.DockerVolumeBackup.Application.Behavoirs;
using Microsoft.Extensions.Hosting;

namespace Booth.DockerVolumeBackup.Application
{
    public static class DependencyInjection
    {

        public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IBackupNotificationService, BackupNotificationService>();
            builder.Services.AddTransient<IBackupService, BackupService>();
          //  builder.Services.AddHostedService<BackupBackgroundService>();

            var applicationAssembly = typeof(DependencyInjection).Assembly;
            builder.Services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(applicationAssembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Transient, includeInternalTypes: true);


            return builder;
        }
    }
}
