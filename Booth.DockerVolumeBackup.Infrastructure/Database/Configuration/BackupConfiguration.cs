using Microsoft.EntityFrameworkCore;
using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Booth.DockerVolumeBackup.Infrastructure.Database.Configuration
{
    internal class BackupConfiguration : IEntityTypeConfiguration<Backup>
    {
        public void Configure(EntityTypeBuilder<Backup> builder)
        {
            builder.ToTable("Backup");
            builder.HasKey(x => x.BackupId);
            builder.HasMany(x => x.Volumes);
        }
    }

    internal class BackupVolumeConfiguration : IEntityTypeConfiguration<BackupVolume>
    {
        public void Configure(EntityTypeBuilder<BackupVolume> builder)
        {
            builder.ToTable("BackupVolume");
            builder.HasKey(x => x.BackupVolumeId);
        }
    }
}
