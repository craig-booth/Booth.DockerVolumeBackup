using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKeepLastField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KeepLast",
                table: "BackupDefinition",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeepLast",
                table: "BackupDefinition");
        }
    }
}
