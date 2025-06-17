using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BackupType",
                table: "Backup",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Backup SET BackupType = CASE WHEN ScheduleId ISNULL THEN 1 ELSE 0 END;"); // Set initial value for existing records
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackupType",
                table: "Backup");
        }
    }
}
