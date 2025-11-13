using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBessAndPcsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BESSDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    SolarId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    StateOfCharge = table.Column<float>(type: "real", nullable: false),
                    StateOfHealth = table.Column<float>(type: "real", nullable: false),
                    BatteryVoltage = table.Column<float>(type: "real", nullable: false),
                    BatteryCurrent = table.Column<float>(type: "real", nullable: false),
                    BatteryPower = table.Column<float>(type: "real", nullable: false),
                    BatteryTemperature = table.Column<float>(type: "real", nullable: false),
                    CellVoltageMin = table.Column<float>(type: "real", nullable: false),
                    CellVoltageMax = table.Column<float>(type: "real", nullable: false),
                    CellVoltageAvg = table.Column<float>(type: "real", nullable: false),
                    CellTemperatureMin = table.Column<float>(type: "real", nullable: false),
                    CellTemperatureMax = table.Column<float>(type: "real", nullable: false),
                    CellTemperatureAvg = table.Column<float>(type: "real", nullable: false),
                    TotalEnergyStored = table.Column<float>(type: "real", nullable: false),
                    EnergyThroughput = table.Column<float>(type: "real", nullable: false),
                    ChargeLimit = table.Column<float>(type: "real", nullable: false),
                    DischargeLimit = table.Column<float>(type: "real", nullable: false),
                    SolarChargeVoltage = table.Column<float>(type: "real", nullable: false),
                    SolarChargeCurrent = table.Column<float>(type: "real", nullable: false),
                    SolarPowerInput = table.Column<float>(type: "real", nullable: false),
                    FaultStatus = table.Column<int>(type: "int", nullable: false),
                    WarningStatus = table.Column<int>(type: "int", nullable: false),
                    InverterEfficiency = table.Column<float>(type: "real", nullable: false),
                    InputVoltage = table.Column<float>(type: "real", nullable: false),
                    OutputVoltageGrid = table.Column<float>(type: "real", nullable: false),
                    InputCurrent = table.Column<float>(type: "real", nullable: false),
                    OutputCurrentGrid = table.Column<float>(type: "real", nullable: false),
                    InputPowerBESS = table.Column<float>(type: "real", nullable: false),
                    OutputPowerGrid = table.Column<float>(type: "real", nullable: false),
                    FrequencyGrid = table.Column<float>(type: "real", nullable: false),
                    TemperatureInverter = table.Column<float>(type: "real", nullable: false),
                    ChargeStatus = table.Column<int>(type: "int", nullable: false),
                    TotalEnergyDelivered = table.Column<float>(type: "real", nullable: false),
                    TotalEnergyAbsorbed = table.Column<float>(type: "real", nullable: false),
                    OperatingMode = table.Column<int>(type: "int", nullable: false),
                    ReactivePowerQ = table.Column<float>(type: "real", nullable: false),
                    ActivePowerP = table.Column<float>(type: "real", nullable: false),
                    PowerFactor = table.Column<float>(type: "real", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BESSDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PCSDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    SolarId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<int>(type: "int", nullable: false),
                    VoltageDcInput = table.Column<float>(type: "real", nullable: false),
                    CurrentDcInput = table.Column<float>(type: "real", nullable: false),
                    PowerDcInput = table.Column<float>(type: "real", nullable: false),
                    VoltageAcOutput = table.Column<float>(type: "real", nullable: false),
                    CurrentAcOutput = table.Column<float>(type: "real", nullable: false),
                    PowerAcOutput = table.Column<float>(type: "real", nullable: false),
                    FrequencyAcOutput = table.Column<float>(type: "real", nullable: false),
                    TemperaturePcs = table.Column<float>(type: "real", nullable: false),
                    Efficiency = table.Column<float>(type: "real", nullable: false),
                    OperatingMode = table.Column<int>(type: "int", nullable: false),
                    FaultStatus = table.Column<int>(type: "int", nullable: false),
                    WarningStatus = table.Column<int>(type: "int", nullable: false),
                    TotalEnergyDelivered = table.Column<float>(type: "real", nullable: false),
                    TotalEnergyAbsorbed = table.Column<float>(type: "real", nullable: false),
                    ReactivePower = table.Column<float>(type: "real", nullable: false),
                    ApparentPower = table.Column<float>(type: "real", nullable: false),
                    PowerFactor = table.Column<float>(type: "real", nullable: false),
                    GridSyncStatus = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PCSDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BESSDatas");

            migrationBuilder.DropTable(
                name: "PCSDatas");
        }
    }
}
