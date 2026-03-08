using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHuaweiDeviceMappingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMappings_StationMappingId_HuaweiDeviceId",
                schema: "huawei",
                table: "DeviceMappings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_StationMappingId_HuaweiDeviceId_DeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                columns: new[] { "StationMappingId", "HuaweiDeviceId", "DeviceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMappings_StationMappingId_HuaweiDeviceId_DeviceId",
                schema: "huawei",
                table: "DeviceMappings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_StationMappingId_HuaweiDeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                columns: new[] { "StationMappingId", "HuaweiDeviceId" },
                unique: true);
        }
    }
}
