using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRtuSerialNumberAndDeviceNameToDeviceAlarm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "DeviceAlarms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RtuSerialNumber",
                table: "DeviceAlarms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "RtuSerialNumber",
                table: "DeviceAlarms");
        }
    }
}
