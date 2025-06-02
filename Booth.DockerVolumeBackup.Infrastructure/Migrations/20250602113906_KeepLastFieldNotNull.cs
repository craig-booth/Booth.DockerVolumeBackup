using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booth.DockerVolumeBackup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KeepLastFieldNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "KeepLast",
                table: "BackupDefinition",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "KeepLast",
                table: "BackupDefinition",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
