using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeviceDataTableRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BESSDatas");

            migrationBuilder.DropTable(
                name: "GrpcFacilityDatas");

            migrationBuilder.DropTable(
                name: "PCSDatas");

            migrationBuilder.DropTable(
                name: "SmartMeterDatas");

            migrationBuilder.DropTable(
                name: "SolarDatas");

            migrationBuilder.CreateTable(
                name: "DeviceBessData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
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
                        name: "FK_DeviceBessData_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevicePcsData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePcsData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevicePcsData_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DevicePcsData_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSmartMeterData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSmartMeterData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSmartMeterData_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceSmartMeterData_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSolarData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    DeviceLocalTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSolarData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSolarData_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceSolarData_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBessData_DeviceId_DeviceLocalTime",
                table: "DeviceBessData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceBessData_FirmId_DeviceLocalTime",
                table: "DeviceBessData",
                columns: new[] { "FirmId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePcsData_DeviceId_DeviceLocalTime",
                table: "DevicePcsData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePcsData_FirmId_DeviceLocalTime",
                table: "DevicePcsData",
                columns: new[] { "FirmId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSmartMeterData_DeviceId_DeviceLocalTime",
                table: "DeviceSmartMeterData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSmartMeterData_FirmId_DeviceLocalTime",
                table: "DeviceSmartMeterData",
                columns: new[] { "FirmId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSolarData_DeviceId_DeviceLocalTime",
                table: "DeviceSolarData",
                columns: new[] { "DeviceId", "DeviceLocalTime" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSolarData_FirmId_DeviceLocalTime",
                table: "DeviceSolarData",
                columns: new[] { "FirmId", "DeviceLocalTime" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceBessData");

            migrationBuilder.DropTable(
                name: "DevicePcsData");

            migrationBuilder.DropTable(
                name: "DeviceSmartMeterData");

            migrationBuilder.DropTable(
                name: "DeviceSolarData");

            migrationBuilder.CreateTable(
                name: "BESSDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BESSId = table.Column<int>(type: "integer", nullable: false),
                    BatteryTemperature = table.Column<double>(type: "double precision", nullable: true),
                    Capacity = table.Column<double>(type: "double precision", nullable: true),
                    ChargeLimit = table.Column<double>(type: "double precision", nullable: true),
                    ChargeStatus = table.Column<string>(type: "text", nullable: false),
                    CoolingActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DischargeLimit = table.Column<double>(type: "double precision", nullable: true),
                    EnergyThroughput = table.Column<double>(type: "double precision", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    NumberOfCells = table.Column<int>(type: "integer", nullable: true),
                    NumberOfModules = table.Column<int>(type: "integer", nullable: true),
                    NumberOfPacks = table.Column<int>(type: "integer", nullable: true),
                    StateOfCharge = table.Column<double>(type: "double precision", nullable: true),
                    StateOfHealth = table.Column<double>(type: "double precision", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    TotalEnergyStored = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    Status_FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Status_OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    Status_WarningStatus = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BESSDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrpcFacilityDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    GeneratorPower = table.Column<double>(type: "double precision", nullable: true),
                    GridPower = table.Column<double>(type: "double precision", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    LoadGeneratorContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadGridContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadPcsContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadSolarContribution = table.Column<double>(type: "double precision", nullable: true),
                    PcsPower = table.Column<double>(type: "double precision", nullable: true),
                    SolarPower = table.Column<double>(type: "double precision", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrpcFacilityDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PCSDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Efficiency = table.Column<double>(type: "double precision", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    PCSId = table.Column<int>(type: "integer", nullable: false),
                    TemperaturePcs = table.Column<double>(type: "double precision", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    TotalEnergyAbsorbed = table.Column<double>(type: "double precision", nullable: true),
                    TotalEnergyDelivered = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    AcPower_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PCSDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartMeterDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActiveEnergyExport = table.Column<double>(type: "double precision", nullable: true),
                    ActiveEnergyImport = table.Column<double>(type: "double precision", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CurrentDemand = table.Column<double>(type: "double precision", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    PowerDemand = table.Column<double>(type: "double precision", nullable: true),
                    ReactiveEnergyExport = table.Column<double>(type: "double precision", nullable: true),
                    ReactiveEnergyImport = table.Column<double>(type: "double precision", nullable: true),
                    SmartMeterId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    TotalActiveEnergy = table.Column<double>(type: "double precision", nullable: true),
                    TotalReactiveEnergy = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    AcPower_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartMeterDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolarDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    PerformanceRatio = table.Column<double>(type: "double precision", nullable: true),
                    PvPanelTemperature = table.Column<double>(type: "double precision", nullable: true),
                    SolarEnergyProduced = table.Column<double>(type: "double precision", nullable: true),
                    SolarId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Current = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Power = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_Current = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_Power = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarDatas", x => x.Id);
                });
        }
    }
}
