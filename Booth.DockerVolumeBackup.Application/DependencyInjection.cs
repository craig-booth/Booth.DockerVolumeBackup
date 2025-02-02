using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MediatR;
using FluentValidation;

using Booth.DockerVolumeBackup.Application.Behavoirs;
using Booth.DockerVolumeBackup.Infrastructure.Services;
using Booth.DockerVolumeBackup.Application.Interfaces;
using Booth.DockerVolumeBackup.Application.BackgroundJobs;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Booth.DockerVolumeBackup.Application.Services;

namespace Booth.DockerVolumeBackup.Application
{
    public static class DependencyInjection
    {

        public static IHostApplicationBuilder AddApplication(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IBackupNotificationService, BackupNotificationService>();
            builder.Services.AddScoped<IScheduleUtils, ScheduleUtils>();

            var applicationAssembly = typeof(DependencyInjection).Assembly;
            builder.Services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(applicationAssembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            builder.Services.AddValidatorsFromAssembly(applicationAssembly, ServiceLifetime.Transient, includeInternalTypes: true);


            return builder;
        }

        public static async Task AddScheduledBackups(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<IDataContext>();
                var scheduler = scope.ServiceProvider.GetRequiredService<IBackupScheduler>();
                var scopeFactory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

                var schedules = await dataContext.Schedules
                    .Where(x => x.Enabled)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    var scheduledJob = new ScheduledBackupJob(schedule.ScheduleId, scopeFactory);
                    scheduler.ScheduleBackup(schedule, scheduledJob);
                } 
            }
        }
    }
}
