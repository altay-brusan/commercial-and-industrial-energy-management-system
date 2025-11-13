using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSolarData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolarDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    SolarId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<int>(type: "int", nullable: false),
                    SolarInputVoltage = table.Column<float>(type: "real", nullable: false),
                    SolarInputCurrent = table.Column<float>(type: "real", nullable: false),
                    SolarInputPower = table.Column<float>(type: "real", nullable: false),
                    ChargerOutputVoltage = table.Column<float>(type: "real", nullable: false),
                    ChargerOutputCurrent = table.Column<float>(type: "real", nullable: false),
                    ChargerOutputPower = table.Column<float>(type: "real", nullable: false),
                    ChargingEfficiency = table.Column<float>(type: "real", nullable: false),
                    OperatingTemperature = table.Column<float>(type: "real", nullable: false),
                    MaximumChargeVoltage = table.Column<float>(type: "real", nullable: false),
                    MaximumChargeCurrent = table.Column<float>(type: "real", nullable: false),
                    ChargerMode = table.Column<int>(type: "int", nullable: false),
                    FaultStatus = table.Column<int>(type: "int", nullable: false),
                    WarningStatus = table.Column<int>(type: "int", nullable: false),
                    PvPanelTemperature = table.Column<float>(type: "real", nullable: false),
                    SolarChargerSocTarget = table.Column<float>(type: "real", nullable: false),
                    SolarEnergyProduced = table.Column<float>(type: "real", nullable: false),
                    SolarChargerRunTime = table.Column<float>(type: "real", nullable: false),
                    SolarInputStatus = table.Column<bool>(type: "bit", nullable: false),
                    SolarPowerLimitCommand = table.Column<float>(type: "real", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolarDatas");
        }
    }
}
