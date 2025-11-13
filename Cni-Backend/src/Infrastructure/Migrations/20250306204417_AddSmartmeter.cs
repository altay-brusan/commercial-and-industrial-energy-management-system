using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartmeter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SolarId",
                table: "PCSDatas",
                newName: "PCSId");

            migrationBuilder.RenameColumn(
                name: "SolarId",
                table: "BESSDatas",
                newName: "BESSId");

            migrationBuilder.CreateTable(
                name: "SmartMeterDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    SmartMeterId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<int>(type: "int", nullable: false),
                    VoltagePhase1 = table.Column<float>(type: "real", nullable: false),
                    VoltagePhase2 = table.Column<float>(type: "real", nullable: false),
                    VoltagePhase3 = table.Column<float>(type: "real", nullable: false),
                    LineToLineVoltagePhase1 = table.Column<float>(type: "real", nullable: false),
                    LineToLineVoltagePhase2 = table.Column<float>(type: "real", nullable: false),
                    LineToLineVoltagePhase3 = table.Column<float>(type: "real", nullable: false),
                    CurrentPhase1 = table.Column<float>(type: "real", nullable: false),
                    CurrentPhase2 = table.Column<float>(type: "real", nullable: false),
                    CurrentPhase3 = table.Column<float>(type: "real", nullable: false),
                    ActivePowerPhase1 = table.Column<float>(type: "real", nullable: false),
                    ActivePowerPhase2 = table.Column<float>(type: "real", nullable: false),
                    ActivePowerPhase3 = table.Column<float>(type: "real", nullable: false),
                    TotalActivePower = table.Column<float>(type: "real", nullable: false),
                    ReactivePowerPhase1 = table.Column<float>(type: "real", nullable: false),
                    ReactivePowerPhase2 = table.Column<float>(type: "real", nullable: false),
                    ReactivePowerPhase3 = table.Column<float>(type: "real", nullable: false),
                    TotalReactivePower = table.Column<float>(type: "real", nullable: false),
                    ApparentPowerPhase1 = table.Column<float>(type: "real", nullable: false),
                    ApparentPowerPhase2 = table.Column<float>(type: "real", nullable: false),
                    ApparentPowerPhase3 = table.Column<float>(type: "real", nullable: false),
                    TotalApparentPower = table.Column<float>(type: "real", nullable: false),
                    PowerFactorPhase1 = table.Column<float>(type: "real", nullable: false),
                    PowerFactorPhase2 = table.Column<float>(type: "real", nullable: false),
                    PowerFactorPhase3 = table.Column<float>(type: "real", nullable: false),
                    TotalPowerFactor = table.Column<float>(type: "real", nullable: false),
                    Frequency = table.Column<float>(type: "real", nullable: false),
                    ActiveEnergyImport = table.Column<float>(type: "real", nullable: false),
                    ActiveEnergyExport = table.Column<float>(type: "real", nullable: false),
                    ReactiveEnergyImport = table.Column<float>(type: "real", nullable: false),
                    ReactiveEnergyExport = table.Column<float>(type: "real", nullable: false),
                    TotalActiveEnergy = table.Column<float>(type: "real", nullable: false),
                    TotalReactiveEnergy = table.Column<float>(type: "real", nullable: false),
                    CurrentDemand = table.Column<float>(type: "real", nullable: false),
                    PowerDemand = table.Column<float>(type: "real", nullable: false),
                    OverloadAlarm = table.Column<bool>(type: "bit", nullable: false),
                    VoltageUnbalanceAlarm = table.Column<bool>(type: "bit", nullable: false),
                    CurrentUnbalanceAlarm = table.Column<bool>(type: "bit", nullable: false),
                    MeterFaultStatus = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartMeterDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmartMeterDatas");

            migrationBuilder.RenameColumn(
                name: "PCSId",
                table: "PCSDatas",
                newName: "SolarId");

            migrationBuilder.RenameColumn(
                name: "BESSId",
                table: "BESSDatas",
                newName: "SolarId");
        }
    }
}
