using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupSchedule",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sunday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Monday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tuesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Wednesday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Thursday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Friday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Saturday = table.Column<bool>(type: "INTEGER", nullable: false),
                    Time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    BackupDefinitionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupSchedule", x => x.ScheduleId);
                });

            migrationBuilder.CreateTable(
                name: "Backup",
                columns: table => new
                {
                    BackupId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backup", x => x.BackupId);
                    table.ForeignKey(
                        name: "FK_Backup_BackupSchedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "BackupSchedule",
                        principalColumn: "ScheduleId");
                });

            migrationBuilder.CreateTable(
                name: "BackupDefinition",
                columns: table => new
                {
                    BackupDefinitionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupDefinition", x => x.BackupDefinitionId);
                    table.ForeignKey(
                        name: "FK_BackupDefinition_BackupSchedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "BackupSchedule",
                        principalColumn: "ScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackupVolume",
                columns: table => new
                {
                    BackupVolumeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Volume = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupVolume", x => x.BackupVolumeId);
                    table.ForeignKey(
                        name: "FK_BackupVolume_Backup_BackupId",
                        column: x => x.BackupId,
                        principalTable: "Backup",
                        principalColumn: "BackupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackupDefinitionVolume",
                columns: table => new
                {
                    BackupDefinitionVolumeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackupDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Volume = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupDefinitionVolume", x => x.BackupDefinitionVolumeId);
                    table.ForeignKey(
                        name: "FK_BackupDefinitionVolume_BackupDefinition_BackupDefinitionId",
                        column: x => x.BackupDefinitionId,
                        principalTable: "BackupDefinition",
                        principalColumn: "BackupDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Backup_ScheduleId",
                table: "Backup",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_BackupDefinition_ScheduleId",
                table: "BackupDefinition",
                column: "ScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackupDefinitionVolume_BackupDefinitionId",
                table: "BackupDefinitionVolume",
                column: "BackupDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_BackupVolume_BackupId",
                table: "BackupVolume",
                column: "BackupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupDefinitionVolume");

            migrationBuilder.DropTable(
                name: "BackupVolume");

            migrationBuilder.DropTable(
                name: "BackupDefinition");

            migrationBuilder.DropTable(
                name: "Backup");

            migrationBuilder.DropTable(
                name: "BackupSchedule");
        }
    }
}
