using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SvgBillBoard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceHeartbeatStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartbeatAt",
                table: "Devices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOnlineAt",
                table: "Devices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatAt",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastOnlineAt",
                table: "Devices");
        }
    }
}
