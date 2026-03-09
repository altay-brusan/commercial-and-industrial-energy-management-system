using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHuaweiDeviceMappingDeviceIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMappings_DeviceId",
                schema: "huawei",
                table: "DeviceMappings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_DeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMappings_DeviceId",
                schema: "huawei",
                table: "DeviceMappings");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_DeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                column: "DeviceId",
                unique: true);
        }
    }
}
