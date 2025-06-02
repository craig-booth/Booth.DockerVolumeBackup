using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackupFile",
                table: "BackupVolume",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BackupSize",
                table: "BackupVolume",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupDirectory",
                table: "Backup",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupFile",
                table: "BackupVolume");

            migrationBuilder.DropColumn(
                name: "BackupSize",
                table: "BackupVolume");

            migrationBuilder.DropColumn(
                name: "BackupDirectory",
                table: "Backup");
        }
    }
}
