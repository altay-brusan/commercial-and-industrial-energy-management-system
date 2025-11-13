using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeviceSourceStreamDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceDeviceId",
                table: "Devices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Devices_SourceDeviceId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "SourceDeviceId",
                table: "Devices");
        }
    }
}
