using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAlarms_Facilities_FacilityId",
                table: "DeviceAlarms");

            migrationBuilder.DropForeignKey(
                name: "FK_DevicePcsData_Facilities_FacilityId",
                table: "DevicePcsData");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceModels_DeviceModelId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Facilities_FacilityId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceSmartMeterData_Facilities_FacilityId",
                table: "DeviceSmartMeterData");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceSolarData_Facilities_FacilityId",
                table: "DeviceSolarData");

            migrationBuilder.DropTable(
                name: "DeviceBessData");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropTable(
                name: "FacilityConnectivities");

            migrationBuilder.DropIndex(
                name: "IX_DeviceSolarData_FacilityId_DeviceLocalTime",
                table: "DeviceSolarData");

            migrationBuilder.DropIndex(
                name: "IX_DeviceSmartMeterData_FacilityId_DeviceLocalTime",
                table: "DeviceSmartMeterData");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_DevicePcsData_FacilityId_DeviceLocalTime",
                table: "DevicePcsData");

            migrationBuilder.DropIndex(
                name: "IX_DeviceAlarms_FacilityId_OccurredAt",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "DeviceSolarData");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "DeviceSmartMeterData");

            migrationBuilder.DropColumn(
                name: "DeviceModelId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "DevicePcsData");

            migrationBuilder.DropColumn(
                name: "FacilityId",
                table: "DeviceAlarms");

            migrationBuilder.RenameColumn(
                name: "FacilityId",
                table: "Devices",
                newName: "RtuId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_FacilityId_Status",
                table: "Devices",
                newName: "IX_Devices_RtuId_Status");

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Devices",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceBatteryData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceBatteryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceBatteryData_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FacilityRemoteTerminalUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RtuSerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RtuModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VpnIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    PublicIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    Protocol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastSeenUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityRemoteTerminalUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityRemoteTerminalUnits_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_DeviceId_OccurredAt",
                table: "DeviceAlarms",
                columns: new[] { "DeviceId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBatteryData_DeviceId_DeviceLocalTime",
                table: "DeviceBatteryData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBatteryData_DeviceId_Kind_DeviceLocalTime",
                table: "DeviceBatteryData",
                columns: new[] { "DeviceId", "Kind", "DeviceLocalTime" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FacilityRemoteTerminalUnits_FacilityId",
                table: "FacilityRemoteTerminalUnits",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityRemoteTerminalUnits_RtuSerialNumber",
                table: "FacilityRemoteTerminalUnits",
                column: "RtuSerialNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_FacilityRemoteTerminalUnits_RtuId",
                table: "Devices",
                column: "RtuId",
                principalTable: "FacilityRemoteTerminalUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_FacilityRemoteTerminalUnits_RtuId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "DeviceBatteryData");

            migrationBuilder.DropTable(
                name: "FacilityRemoteTerminalUnits");

            migrationBuilder.DropIndex(
                name: "IX_DeviceAlarms_DeviceId_OccurredAt",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "RtuId",
                table: "Devices",
                newName: "FacilityId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_RtuId_Status",
                table: "Devices",
                newName: "IX_Devices_FacilityId_Status");

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityId",
                table: "DeviceSolarData",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityId",
                table: "DeviceSmartMeterData",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceModelId",
                table: "Devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityId",
                table: "DevicePcsData",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "FacilityId",
                table: "DeviceAlarms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "DeviceBessData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceBessData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceBessData_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceBessData_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PortTemplate = table.Column<string>(type: "jsonb", nullable: true),
                    Specifications = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceModels_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FacilityConnectivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    LastSeenUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: true),
                    Protocol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    RtuModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RtuSerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VpnIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityConnectivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacilityConnectivities_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSolarData_FacilityId_DeviceLocalTime",
                table: "DeviceSolarData",
                columns: new[] { "FacilityId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSmartMeterData_FacilityId_DeviceLocalTime",
                table: "DeviceSmartMeterData",
                columns: new[] { "FacilityId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DevicePcsData_FacilityId_DeviceLocalTime",
                table: "DevicePcsData",
                columns: new[] { "FacilityId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_FacilityId_OccurredAt",
                table: "DeviceAlarms",
                columns: new[] { "FacilityId", "OccurredAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBessData_DeviceId_DeviceLocalTime",
                table: "DeviceBessData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBessData_DeviceId_Kind_DeviceLocalTime",
                table: "DeviceBessData",
                columns: new[] { "DeviceId", "Kind", "DeviceLocalTime" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBessData_FacilityId_DeviceLocalTime",
                table: "DeviceBessData",
                columns: new[] { "FacilityId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceModels_DeviceTypeId_Name_Manufacturer",
                table: "DeviceModels",
                columns: new[] { "DeviceTypeId", "Name", "Manufacturer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacilityConnectivities_FacilityId",
                table: "FacilityConnectivities",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityConnectivities_RtuSerialNumber",
                table: "FacilityConnectivities",
                column: "RtuSerialNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAlarms_Facilities_FacilityId",
                table: "DeviceAlarms",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DevicePcsData_Facilities_FacilityId",
                table: "DevicePcsData",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceModels_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId",
                principalTable: "DeviceModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Facilities_FacilityId",
                table: "Devices",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSmartMeterData_Facilities_FacilityId",
                table: "DeviceSmartMeterData",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSolarData_Facilities_FacilityId",
                table: "DeviceSolarData",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
