using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvgBillBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePlaybackVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaybackVersion",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaybackVersion",
                table: "Devices");
        }
    }
}
