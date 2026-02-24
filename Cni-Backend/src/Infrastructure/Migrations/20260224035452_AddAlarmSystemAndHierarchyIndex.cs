using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmSystemAndHierarchyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_ParentDeviceId",
                table: "Devices");

            migrationBuilder.AddColumn<int>(
                name: "HierarchyIndex",
                table: "Devices",
                type: "integer",
                nullable: true);

            // Drop and recreate DeviceAlarms with correct column ordering
            // (table is always empty — no data loss)
            migrationBuilder.DropTable(
                name: "DeviceAlarms");

            migrationBuilder.CreateTable(
                name: "DeviceAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceAlarmTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Details = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAlarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAlarms_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAlarmTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FaultKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DetectionStrategy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "BooleanFlag"),
                    MatchPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAlarmTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAlarmTypes_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ParentDeviceId_HierarchyIndex",
                table: "Devices",
                columns: new[] { "ParentDeviceId", "HierarchyIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceAlarmTypeId",
                table: "DeviceAlarms",
                column: "DeviceAlarmTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_IsActive",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_OccurredAt",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_DeviceAlarmTypeId_IsActive",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "DeviceAlarmTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarmTypes_DeviceTypeId_Model_Manufacturer",
                table: "DeviceAlarmTypes",
                columns: new[] { "DeviceTypeId", "Model", "Manufacturer" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarmTypes_DeviceTypeId_Model_Manufacturer_FaultKey",
                table: "DeviceAlarmTypes",
                columns: new[] { "DeviceTypeId", "Model", "Manufacturer", "FaultKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarmTypes_Kind",
                table: "DeviceAlarmTypes",
                column: "Kind");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAlarms_DeviceAlarmTypes_DeviceAlarmTypeId",
                table: "DeviceAlarms",
                column: "DeviceAlarmTypeId",
                principalTable: "DeviceAlarmTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAlarmTypes");

            // Recreate original DeviceAlarms table
            migrationBuilder.DropTable(
                name: "DeviceAlarms");

            migrationBuilder.CreateTable(
                name: "DeviceAlarms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FaultKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecoveredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAlarms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAlarms_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_IsActive",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_OccurredAt",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.DropColumn(
                name: "HierarchyIndex",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ParentDeviceId",
                table: "Devices",
                column: "ParentDeviceId");
        }
    }
}
