using Booth.DockerVolumeBackup.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booth.DockerVolumeBackup.Infrastructure.Database.Configuration
{
    internal class BackupDefinitionConfiguration : IEntityTypeConfiguration<BackupDefinition>
    {
        public void Configure(EntityTypeBuilder<BackupDefinition> builder)
        {
            builder.ToTable("BackupDefinition");
            builder.HasKey(x => x.BackupDefinitionId);

            builder.HasMany(x => x.Volumes)
                .WithOne()
                .HasForeignKey(x => x.BackupDefinitionId);
        }
    }

    internal class BackupDefinitionVolumeConfiguration : IEntityTypeConfiguration<BackupDefinitionVolume>
    {
        public void Configure(EntityTypeBuilder<BackupDefinitionVolume> builder)
        {
            builder.ToTable("BackupDefinitionVolume");
            builder.HasKey(x => x.BackupDefinitionVolumeId);
        }
    }
}
