using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeviceEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DeviceId",
                table: "TopologyNodes",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    GrpcConnection = table.Column<bool>(type: "bit", nullable: false),
                    GrpcDeviceStatus = table.Column<int>(type: "int", nullable: false),
                    GrpcLastDataReviceDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_DeviceId",
                table: "TopologyNodes",
                column: "DeviceId",
                unique: true,
                filter: "[DeviceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_FacilityId",
                table: "Devices",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TenantId",
                table: "Devices",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_TopologyNodes_Devices_DeviceId",
                table: "TopologyNodes",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopologyNodes_Devices_DeviceId",
                table: "TopologyNodes");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_TopologyNodes_DeviceId",
                table: "TopologyNodes");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceId",
                table: "TopologyNodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
