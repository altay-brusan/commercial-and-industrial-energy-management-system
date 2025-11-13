using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeviceConnectedDeviceAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId",
                unique: true,
                filter: "[SourceDeviceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId");
        }
    }
}
