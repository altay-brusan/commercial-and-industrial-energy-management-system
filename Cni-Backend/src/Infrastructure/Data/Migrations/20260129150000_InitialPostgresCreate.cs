using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Surname = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BESSDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    BESSId = table.Column<int>(type: "integer", nullable: false),
                    StateOfCharge = table.Column<double>(type: "double precision", nullable: true),
                    StateOfHealth = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    BatteryTemperature = table.Column<double>(type: "double precision", nullable: true),
                    TotalEnergyStored = table.Column<double>(type: "double precision", nullable: true),
                    EnergyThroughput = table.Column<double>(type: "double precision", nullable: true),
                    ChargeLimit = table.Column<double>(type: "double precision", nullable: true),
                    DischargeLimit = table.Column<double>(type: "double precision", nullable: true),
                    NumberOfPacks = table.Column<int>(type: "integer", nullable: true),
                    NumberOfModules = table.Column<int>(type: "integer", nullable: true),
                    NumberOfCells = table.Column<int>(type: "integer", nullable: true),
                    Capacity = table.Column<double>(type: "double precision", nullable: true),
                    CoolingActive = table.Column<bool>(type: "boolean", nullable: false),
                    ChargeStatus = table.Column<string>(type: "text", nullable: false),
                    Status_OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    Status_WarningStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Status_FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BESSDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HasGrpcStream = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDataCollectedViaOtherDevice = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrpcFacilityDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    GeneratorPower = table.Column<double>(type: "double precision", nullable: true),
                    GridPower = table.Column<double>(type: "double precision", nullable: true),
                    LoadGeneratorContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadGridContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadPcsContribution = table.Column<double>(type: "double precision", nullable: true),
                    LoadSolarContribution = table.Column<double>(type: "double precision", nullable: true),
                    PcsPower = table.Column<double>(type: "double precision", nullable: true),
                    SolarPower = table.Column<double>(type: "double precision", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
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
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    PCSId = table.Column<int>(type: "integer", nullable: false),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    AcPower_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    TemperaturePcs = table.Column<double>(type: "double precision", nullable: true),
                    Efficiency = table.Column<double>(type: "double precision", nullable: true),
                    TotalEnergyDelivered = table.Column<double>(type: "double precision", nullable: true),
                    TotalEnergyAbsorbed = table.Column<double>(type: "double precision", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PCSDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modules = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiAccess = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFacilities = table.Column<int>(type: "integer", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartMeterDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    SmartMeterId = table.Column<int>(type: "integer", nullable: false),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    AcPower_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    ActiveEnergyImport = table.Column<double>(type: "double precision", nullable: true),
                    ActiveEnergyExport = table.Column<double>(type: "double precision", nullable: true),
                    ReactiveEnergyImport = table.Column<double>(type: "double precision", nullable: true),
                    ReactiveEnergyExport = table.Column<double>(type: "double precision", nullable: true),
                    TotalActiveEnergy = table.Column<double>(type: "double precision", nullable: true),
                    TotalReactiveEnergy = table.Column<double>(type: "double precision", nullable: true),
                    CurrentDemand = table.Column<double>(type: "double precision", nullable: true),
                    PowerDemand = table.Column<double>(type: "double precision", nullable: true),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
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
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    SolarId = table.Column<int>(type: "integer", nullable: false),
                    PanelPower_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_Current = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_Power = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    PanelPower_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Current = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_Power = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ChargerPower_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Voltage = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Current = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_Power = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    DCOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_LineToLineVoltage_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Voltage_RMS = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Current_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_CurrentRms = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ActivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalActivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ReactivePower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalReactivePower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_ApparentPower_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalApparentPower = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase1 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase2 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_PowerFactor_Phase3 = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalPowerFactor = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyDaily = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_TotalEnergyLifetime = table.Column<double>(type: "double precision", nullable: true),
                    ACOutput_Frequency = table.Column<double>(type: "double precision", nullable: true),
                    PerformanceRatio = table.Column<double>(type: "double precision", nullable: true),
                    PvPanelTemperature = table.Column<double>(type: "double precision", nullable: true),
                    SolarEnergyProduced = table.Column<double>(type: "double precision", nullable: true),
                    OperatingMode = table.Column<int>(type: "integer", nullable: false),
                    WarningStatus = table.Column<bool>(type: "boolean", nullable: false),
                    FaultStatus = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConnectionString = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Items = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Colour_Code = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppRoleClaims_AppRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserClaims_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AppUserLogins_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AppRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserRoles_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AppUserTokens_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Firms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Fax = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxOffice = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Firms_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Reminder = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Done = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_TodoLists_ListId",
                        column: x => x.ListId,
                        principalTable: "TodoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FirmId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    GrpcStatus = table.Column<bool>(type: "boolean", nullable: false),
                    GrpcChannel = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facilities_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Facilities_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    MachineId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceTypeId = table.Column<int>(type: "integer", nullable: false),
                    SourceDeviceId = table.Column<int>(type: "integer", nullable: true),
                    GrpcConnection = table.Column<bool>(type: "boolean", nullable: false),
                    GrpcDeviceStatus = table.Column<int>(type: "integer", nullable: false),
                    GrpcLastDataReviceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_Devices_SourceDeviceId",
                        column: x => x.SourceDeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "UserFirmFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    FirmId = table.Column<int>(type: "integer", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFirmFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TopologyNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Extent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    TargetHandleCount = table.Column<int>(type: "integer", nullable: true),
                    SourceHandleCount = table.Column<int>(type: "integer", nullable: true),
                    Data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Position = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Style = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopologyEdges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    TargetHandle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceHandle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_TopologyNodes_SourceId",
                        column: x => x.SourceId,
                        principalTable: "TopologyNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_TopologyNodes_TargetId",
                        column: x => x.TargetId,
                        principalTable: "TopologyNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRoleClaims_RoleId",
                table: "AppRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AppRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserClaims_UserId",
                table: "AppUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserLogins_UserId",
                table: "AppUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserRoles_RoleId",
                table: "AppUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AppUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AppUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_FacilityId",
                table: "Devices",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TenantId",
                table: "Devices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_FirmId",
                table: "Facilities",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_TenantId",
                table: "Facilities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Firms_TenantId",
                table: "Firms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ListId",
                table: "TodoItems",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_FacilityId",
                table: "TopologyEdges",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_SourceId",
                table: "TopologyEdges",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_TargetId",
                table: "TopologyEdges",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_TenantId",
                table: "TopologyEdges",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_DeviceId",
                table: "TopologyNodes",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_FacilityId",
                table: "TopologyNodes",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_TenantId",
                table: "TopologyNodes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_FacilityId",
                table: "UserFirmFacilities",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_FirmId",
                table: "UserFirmFacilities",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_UserId_FirmId_FacilityId",
                table: "UserFirmFacilities",
                columns: new[] { "UserId", "FirmId", "FacilityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRoleClaims");

            migrationBuilder.DropTable(
                name: "AppUserClaims");

            migrationBuilder.DropTable(
                name: "AppUserLogins");

            migrationBuilder.DropTable(
                name: "AppUserRoles");

            migrationBuilder.DropTable(
                name: "AppUserTokens");

            migrationBuilder.DropTable(
                name: "BESSDatas");

            migrationBuilder.DropTable(
                name: "GrpcFacilityDatas");

            migrationBuilder.DropTable(
                name: "PCSDatas");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "SmartMeterDatas");

            migrationBuilder.DropTable(
                name: "SolarDatas");

            migrationBuilder.DropTable(
                name: "TodoItems");

            migrationBuilder.DropTable(
                name: "TopologyEdges");

            migrationBuilder.DropTable(
                name: "UserFirmFacilities");

            migrationBuilder.DropTable(
                name: "AppRoles");

            migrationBuilder.DropTable(
                name: "TodoLists");

            migrationBuilder.DropTable(
                name: "TopologyNodes");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DeviceTypes");

            migrationBuilder.DropTable(
                name: "Facilities");

            migrationBuilder.DropTable(
                name: "Firms");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
