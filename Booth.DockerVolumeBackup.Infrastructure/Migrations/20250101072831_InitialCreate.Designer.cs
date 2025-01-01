﻿// <auto-generated />
using System;
using Booth.DockerVolumeBackup.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250101072831_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.Backup", b =>
                {
                    b.Property<int>("BackupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("BackupId");

                    b.HasIndex("ScheduleId");

                    b.ToTable("Backup", (string)null);
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupSchedule", b =>
                {
                    b.Property<int>("ScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Enabled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Friday")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Monday")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Saturday")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Sunday")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Thursday")
                        .HasColumnType("INTEGER");

                    b.Property<TimeOnly>("Time")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Tuesday")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Wednesday")
                        .HasColumnType("INTEGER");

                    b.HasKey("ScheduleId");

                    b.ToTable("BackupSchedule", (string)null);
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupScheduleVolume", b =>
                {
                    b.Property<int>("BackupScheduleVolumeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Volume")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("BackupScheduleVolumeId");

                    b.HasIndex("ScheduleId");

                    b.ToTable("BackupScheduleVolume", (string)null);
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupVolume", b =>
                {
                    b.Property<int>("BackupVolumeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BackupId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Volume")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("BackupVolumeId");

                    b.HasIndex("BackupId");

                    b.ToTable("BackupVolume", (string)null);
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.Backup", b =>
                {
                    b.HasOne("Booth.DockerVolumeBackup.Domain.Models.BackupSchedule", "Schedule")
                        .WithMany("Backups")
                        .HasForeignKey("ScheduleId");

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupScheduleVolume", b =>
                {
                    b.HasOne("Booth.DockerVolumeBackup.Domain.Models.BackupSchedule", null)
                        .WithMany("Volumes")
                        .HasForeignKey("ScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupVolume", b =>
                {
                    b.HasOne("Booth.DockerVolumeBackup.Domain.Models.Backup", null)
                        .WithMany("Volumes")
                        .HasForeignKey("BackupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.Backup", b =>
                {
                    b.Navigation("Volumes");
                });

            modelBuilder.Entity("Booth.DockerVolumeBackup.Domain.Models.BackupSchedule", b =>
                {
                    b.Navigation("Backups");

                    b.Navigation("Volumes");
                });
#pragma warning restore 612, 618
        }
    }
}
