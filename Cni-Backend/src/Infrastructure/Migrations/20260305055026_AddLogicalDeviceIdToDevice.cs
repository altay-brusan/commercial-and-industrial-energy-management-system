using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogicalDeviceIdToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices",
                column: "LogicalDeviceId",
                principalTable: "LogicalDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices",
                column: "LogicalDeviceId",
                principalTable: "LogicalDevices",
                principalColumn: "Id");
        }
    }
}
