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

            builder.HasMany(x => x.Volumes)
                .WithOne()
                .HasForeignKey(x => x.ScheduleId);
        }
    }


    internal class BackupScheduleVolumeConfiguration : IEntityTypeConfiguration<BackupScheduleVolume>
    {
        public void Configure(EntityTypeBuilder<BackupScheduleVolume> builder)
        {
            builder.ToTable("BackupScheduleVolume");
            builder.HasKey(x => x.BackupScheduleVolumeId);
        }
    }
}
