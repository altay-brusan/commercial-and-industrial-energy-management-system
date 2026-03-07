using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnvelopeDeviceNameAndPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DeviceSolarData",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DeviceSmartMeterData",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DevicePcsData",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DeviceBatteryData",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DeviceAlarms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "DeviceSolarData");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DeviceSmartMeterData");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DevicePcsData");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DeviceBatteryData");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DeviceAlarms");
        }
    }
}
