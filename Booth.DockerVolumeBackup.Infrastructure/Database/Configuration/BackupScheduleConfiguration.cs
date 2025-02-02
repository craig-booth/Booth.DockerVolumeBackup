using System;

using Microsoft.EntityFrameworkCore;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Booth.DockerVolumeBackup.Infrastructure.Database.Configuration
{
    internal class BackupScheduleConfiguration : IEntityTypeConfiguration<BackupSchedule>
    {
        public void Configure(EntityTypeBuilder<BackupSchedule> builder)
        {
            builder.ToTable("BackupSchedule");
            builder.HasKey(x => x.ScheduleId);

            builder.HasMany(x => x.Backups)
                .WithOne(x => x.Schedule)
                .HasForeignKey(x => x.ScheduleId);

            builder.HasOne(x => x.BackupDefinition)
                .WithOne()
                .HasForeignKey<BackupDefinition>(x => x.ScheduleId)
                .IsRequired();
        }
    }

}
