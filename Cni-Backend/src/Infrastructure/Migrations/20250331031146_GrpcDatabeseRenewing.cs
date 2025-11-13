using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrpcDatabeseRenewing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargerMode",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerOutputCurrent",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerOutputPower",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerOutputVoltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargingEfficiency",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "MaximumChargeCurrent",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "MaximumChargeVoltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "OperatingTemperature",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarChargerRunTime",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarChargerSocTarget",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarInputCurrent",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarInputPower",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarInputStatus",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarInputVoltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "SolarPowerLimitCommand",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "CurrentUnbalanceAlarm",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "MeterFaultStatus",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "ApparentPower",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "CurrentAcOutput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "CurrentDcInput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "FrequencyAcOutput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "GridSyncStatus",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "PowerAcOutput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "PowerDcInput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "PowerFactor",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ReactivePower",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "VoltageAcOutput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "VoltageDcInput",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ActivePowerP",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "BatteryCurrent",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "BatteryPower",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "BatteryVoltage",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellTemperatureAvg",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellTemperatureMax",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellTemperatureMin",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellVoltageAvg",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellVoltageMax",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CellVoltageMin",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "FrequencyGrid",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "InputCurrent",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "InputPowerBESS",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "InputVoltage",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "InverterEfficiency",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "OutputCurrentGrid",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "OutputPowerGrid",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "OutputVoltageGrid",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "PowerFactor",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "ReactivePowerQ",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "SolarChargeCurrent",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "SolarChargeVoltage",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "SolarPowerInput",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "TemperatureInverter",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "TotalEnergyAbsorbed",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "TotalEnergyDelivered",
                table: "BESSDatas");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "SolarDatas",
                newName: "OperatingMode");

            migrationBuilder.RenameColumn(
                name: "VoltagePhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_Voltage_Phase3");

            migrationBuilder.RenameColumn(
                name: "VoltagePhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_Voltage_Phase2");

            migrationBuilder.RenameColumn(
                name: "VoltagePhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_Voltage_Phase1");

            migrationBuilder.RenameColumn(
                name: "TotalReactivePower",
                table: "SmartMeterDatas",
                newName: "ACOutput_TotalReactivePower");

            migrationBuilder.RenameColumn(
                name: "TotalPowerFactor",
                table: "SmartMeterDatas",
                newName: "ACOutput_TotalPowerFactor");

            migrationBuilder.RenameColumn(
                name: "TotalApparentPower",
                table: "SmartMeterDatas",
                newName: "ACOutput_TotalApparentPower");

            migrationBuilder.RenameColumn(
                name: "TotalActivePower",
                table: "SmartMeterDatas",
                newName: "ACOutput_TotalActivePower");

            migrationBuilder.RenameColumn(
                name: "ReactivePowerPhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_ReactivePower_Phase3");

            migrationBuilder.RenameColumn(
                name: "ReactivePowerPhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_ReactivePower_Phase2");

            migrationBuilder.RenameColumn(
                name: "ReactivePowerPhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_ReactivePower_Phase1");

            migrationBuilder.RenameColumn(
                name: "PowerFactorPhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_PowerFactor_Phase3");

            migrationBuilder.RenameColumn(
                name: "PowerFactorPhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_PowerFactor_Phase2");

            migrationBuilder.RenameColumn(
                name: "PowerFactorPhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_PowerFactor_Phase1");

            migrationBuilder.RenameColumn(
                name: "LineToLineVoltagePhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_LineToLineVoltage_Phase3");

            migrationBuilder.RenameColumn(
                name: "LineToLineVoltagePhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_LineToLineVoltage_Phase2");

            migrationBuilder.RenameColumn(
                name: "LineToLineVoltagePhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_LineToLineVoltage_Phase1");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "SmartMeterDatas",
                newName: "ACOutput_Frequency");

            migrationBuilder.RenameColumn(
                name: "CurrentPhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_Current_Phase3");

            migrationBuilder.RenameColumn(
                name: "CurrentPhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_Current_Phase2");

            migrationBuilder.RenameColumn(
                name: "CurrentPhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_Current_Phase1");

            migrationBuilder.RenameColumn(
                name: "ApparentPowerPhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_ApparentPower_Phase3");

            migrationBuilder.RenameColumn(
                name: "ApparentPowerPhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_ApparentPower_Phase2");

            migrationBuilder.RenameColumn(
                name: "ApparentPowerPhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_ApparentPower_Phase1");

            migrationBuilder.RenameColumn(
                name: "ActivePowerPhase3",
                table: "SmartMeterDatas",
                newName: "ACOutput_ActivePower_Phase3");

            migrationBuilder.RenameColumn(
                name: "ActivePowerPhase2",
                table: "SmartMeterDatas",
                newName: "ACOutput_ActivePower_Phase2");

            migrationBuilder.RenameColumn(
                name: "ActivePowerPhase1",
                table: "SmartMeterDatas",
                newName: "ACOutput_ActivePower_Phase1");

            migrationBuilder.RenameColumn(
                name: "VoltageUnbalanceAlarm",
                table: "SmartMeterDatas",
                newName: "WarningStatus");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "SmartMeterDatas",
                newName: "OperatingMode");

            migrationBuilder.RenameColumn(
                name: "OverloadAlarm",
                table: "SmartMeterDatas",
                newName: "FaultStatus");

            migrationBuilder.RenameColumn(
                name: "WarningStatus",
                table: "BESSDatas",
                newName: "Status_WarningStatus");

            migrationBuilder.RenameColumn(
                name: "OperatingMode",
                table: "BESSDatas",
                newName: "Status_OperatingMode");

            migrationBuilder.RenameColumn(
                name: "FaultStatus",
                table: "BESSDatas",
                newName: "Status_FaultStatus");

            migrationBuilder.AlterColumn<bool>(
                name: "WarningStatus",
                table: "SolarDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "SolarEnergyProduced",
                table: "SolarDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "PvPanelTemperature",
                table: "SolarDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<bool>(
                name: "FaultStatus",
                table: "SolarDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_CurrentRms",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Frequency",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalActivePower",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalApparentPower",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyDaily",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyLifetime",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalPowerFactor",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalReactivePower",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase1",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase2",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase3",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_RMS",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChargerPower_Current",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChargerPower_Power",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChargerPower_TotalEnergyDaily",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChargerPower_TotalEnergyLifetime",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChargerPower_Voltage",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Current",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Power",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyDaily",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyLifetime",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Voltage",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PanelPower_Current",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PanelPower_Power",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PanelPower_TotalEnergyDaily",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PanelPower_TotalEnergyLifetime",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PanelPower_Voltage",
                table: "SolarDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PerformanceRatio",
                table: "SolarDatas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<double>(
                name: "TotalReactiveEnergy",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "TotalActiveEnergy",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ReactiveEnergyImport",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ReactiveEnergyExport",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "PowerDemand",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "CurrentDemand",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ActiveEnergyImport",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ActiveEnergyExport",
                table: "SmartMeterDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Voltage_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Voltage_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Voltage_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_TotalReactivePower",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_TotalPowerFactor",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_TotalApparentPower",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_TotalActivePower",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ReactivePower_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ReactivePower_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ReactivePower_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_PowerFactor_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_PowerFactor_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_PowerFactor_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Frequency",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Current_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Current_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_Current_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ApparentPower_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ApparentPower_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ApparentPower_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ActivePower_Phase3",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ActivePower_Phase2",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "ACOutput_ActivePower_Phase1",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyDaily",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyLifetime",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_RMS",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AcPower_CurrentRms",
                table: "SmartMeterDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "WarningStatus",
                table: "PCSDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "TotalEnergyDelivered",
                table: "PCSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "TotalEnergyAbsorbed",
                table: "PCSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "TemperaturePcs",
                table: "PCSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<bool>(
                name: "FaultStatus",
                table: "PCSDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "Efficiency",
                table: "PCSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ActivePower_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ApparentPower_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Current_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Frequency",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_PowerFactor_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_ReactivePower_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalActivePower",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalApparentPower",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyDaily",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalEnergyLifetime",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalPowerFactor",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_TotalReactivePower",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase1",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase2",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_Phase3",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ACOutput_Voltage_RMS",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AcPower_CurrentRms",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Current",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Power",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyDaily",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyLifetime",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Voltage",
                table: "PCSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TotalEnergyStored",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "StateOfHealth",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "StateOfCharge",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "EnergyThroughput",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "DischargeLimit",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<string>(
                name: "ChargeStatus",
                table: "BESSDatas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "ChargeLimit",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<double>(
                name: "BatteryTemperature",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AlterColumn<bool>(
                name: "Status_WarningStatus",
                table: "BESSDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "Status_FaultStatus",
                table: "BESSDatas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "Capacity",
                table: "BESSDatas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "CoolingActive",
                table: "BESSDatas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Current",
                table: "BESSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Power",
                table: "BESSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyDaily",
                table: "BESSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_TotalEnergyLifetime",
                table: "BESSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DCOutput_Voltage",
                table: "BESSDatas",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfCells",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfModules",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPacks",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_CurrentRms",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Frequency",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalActivePower",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalApparentPower",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyDaily",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyLifetime",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalPowerFactor",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalReactivePower",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase1",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase2",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase3",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_RMS",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerPower_Current",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerPower_Power",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerPower_TotalEnergyDaily",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerPower_TotalEnergyLifetime",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ChargerPower_Voltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Current",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Power",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyDaily",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyLifetime",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Voltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PanelPower_Current",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PanelPower_Power",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PanelPower_TotalEnergyDaily",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PanelPower_TotalEnergyLifetime",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PanelPower_Voltage",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "PerformanceRatio",
                table: "SolarDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyDaily",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyLifetime",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_RMS",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "AcPower_CurrentRms",
                table: "SmartMeterDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ActivePower_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ApparentPower_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Current_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Frequency",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_PowerFactor_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_ReactivePower_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalActivePower",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalApparentPower",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyDaily",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalEnergyLifetime",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalPowerFactor",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_TotalReactivePower",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase1",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase2",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_Phase3",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "ACOutput_Voltage_RMS",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "AcPower_CurrentRms",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Current",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Power",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyDaily",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyLifetime",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Voltage",
                table: "PCSDatas");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "CoolingActive",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Current",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Power",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyDaily",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_TotalEnergyLifetime",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "DCOutput_Voltage",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "NumberOfCells",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "NumberOfModules",
                table: "BESSDatas");

            migrationBuilder.DropColumn(
                name: "NumberOfPacks",
                table: "BESSDatas");

            migrationBuilder.RenameColumn(
                name: "OperatingMode",
                table: "SolarDatas",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Voltage_Phase3",
                table: "SmartMeterDatas",
                newName: "VoltagePhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Voltage_Phase2",
                table: "SmartMeterDatas",
                newName: "VoltagePhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Voltage_Phase1",
                table: "SmartMeterDatas",
                newName: "VoltagePhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_TotalReactivePower",
                table: "SmartMeterDatas",
                newName: "TotalReactivePower");

            migrationBuilder.RenameColumn(
                name: "ACOutput_TotalPowerFactor",
                table: "SmartMeterDatas",
                newName: "TotalPowerFactor");

            migrationBuilder.RenameColumn(
                name: "ACOutput_TotalApparentPower",
                table: "SmartMeterDatas",
                newName: "TotalApparentPower");

            migrationBuilder.RenameColumn(
                name: "ACOutput_TotalActivePower",
                table: "SmartMeterDatas",
                newName: "TotalActivePower");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ReactivePower_Phase3",
                table: "SmartMeterDatas",
                newName: "ReactivePowerPhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ReactivePower_Phase2",
                table: "SmartMeterDatas",
                newName: "ReactivePowerPhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ReactivePower_Phase1",
                table: "SmartMeterDatas",
                newName: "ReactivePowerPhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_PowerFactor_Phase3",
                table: "SmartMeterDatas",
                newName: "PowerFactorPhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_PowerFactor_Phase2",
                table: "SmartMeterDatas",
                newName: "PowerFactorPhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_PowerFactor_Phase1",
                table: "SmartMeterDatas",
                newName: "PowerFactorPhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_LineToLineVoltage_Phase3",
                table: "SmartMeterDatas",
                newName: "LineToLineVoltagePhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_LineToLineVoltage_Phase2",
                table: "SmartMeterDatas",
                newName: "LineToLineVoltagePhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_LineToLineVoltage_Phase1",
                table: "SmartMeterDatas",
                newName: "LineToLineVoltagePhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Frequency",
                table: "SmartMeterDatas",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Current_Phase3",
                table: "SmartMeterDatas",
                newName: "CurrentPhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Current_Phase2",
                table: "SmartMeterDatas",
                newName: "CurrentPhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_Current_Phase1",
                table: "SmartMeterDatas",
                newName: "CurrentPhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ApparentPower_Phase3",
                table: "SmartMeterDatas",
                newName: "ApparentPowerPhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ApparentPower_Phase2",
                table: "SmartMeterDatas",
                newName: "ApparentPowerPhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ApparentPower_Phase1",
                table: "SmartMeterDatas",
                newName: "ApparentPowerPhase1");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ActivePower_Phase3",
                table: "SmartMeterDatas",
                newName: "ActivePowerPhase3");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ActivePower_Phase2",
                table: "SmartMeterDatas",
                newName: "ActivePowerPhase2");

            migrationBuilder.RenameColumn(
                name: "ACOutput_ActivePower_Phase1",
                table: "SmartMeterDatas",
                newName: "ActivePowerPhase1");

            migrationBuilder.RenameColumn(
                name: "WarningStatus",
                table: "SmartMeterDatas",
                newName: "VoltageUnbalanceAlarm");

            migrationBuilder.RenameColumn(
                name: "OperatingMode",
                table: "SmartMeterDatas",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "FaultStatus",
                table: "SmartMeterDatas",
                newName: "OverloadAlarm");

            migrationBuilder.RenameColumn(
                name: "Status_WarningStatus",
                table: "BESSDatas",
                newName: "WarningStatus");

            migrationBuilder.RenameColumn(
                name: "Status_OperatingMode",
                table: "BESSDatas",
                newName: "OperatingMode");

            migrationBuilder.RenameColumn(
                name: "Status_FaultStatus",
                table: "BESSDatas",
                newName: "FaultStatus");

            migrationBuilder.AlterColumn<int>(
                name: "WarningStatus",
                table: "SolarDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<float>(
                name: "SolarEnergyProduced",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "PvPanelTemperature",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "FaultStatus",
                table: "SolarDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "ChargerMode",
                table: "SolarDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "ChargerOutputCurrent",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ChargerOutputPower",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ChargerOutputVoltage",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ChargingEfficiency",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "MaximumChargeCurrent",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "MaximumChargeVoltage",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OperatingTemperature",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarChargerRunTime",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarChargerSocTarget",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarInputCurrent",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarInputPower",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "SolarInputStatus",
                table: "SolarDatas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "SolarInputVoltage",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarPowerLimitCommand",
                table: "SolarDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AlterColumn<float>(
                name: "TotalReactiveEnergy",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TotalActiveEnergy",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "ReactiveEnergyImport",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "ReactiveEnergyExport",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "PowerDemand",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "CurrentDemand",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "ActiveEnergyImport",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "ActiveEnergyExport",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "VoltagePhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "VoltagePhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "VoltagePhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "TotalReactivePower",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "TotalPowerFactor",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "TotalApparentPower",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "TotalActivePower",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ReactivePowerPhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ReactivePowerPhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ReactivePowerPhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PowerFactorPhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PowerFactorPhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "PowerFactorPhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "LineToLineVoltagePhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "LineToLineVoltagePhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "LineToLineVoltagePhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Frequency",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "CurrentPhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "CurrentPhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "CurrentPhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ApparentPowerPhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ApparentPowerPhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ApparentPowerPhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ActivePowerPhase3",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ActivePowerPhase2",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "ActivePowerPhase1",
                table: "SmartMeterDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CurrentUnbalanceAlarm",
                table: "SmartMeterDatas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MeterFaultStatus",
                table: "SmartMeterDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "WarningStatus",
                table: "PCSDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<float>(
                name: "TotalEnergyDelivered",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TotalEnergyAbsorbed",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "TemperaturePcs",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "FaultStatus",
                table: "PCSDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<float>(
                name: "Efficiency",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<float>(
                name: "ApparentPower",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CurrentAcOutput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CurrentDcInput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "FrequencyAcOutput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "GridSyncStatus",
                table: "PCSDatas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "PowerAcOutput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PowerDcInput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PowerFactor",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ReactivePower",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "VoltageAcOutput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "VoltageDcInput",
                table: "PCSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AlterColumn<float>(
                name: "TotalEnergyStored",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "StateOfHealth",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "StateOfCharge",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "EnergyThroughput",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "DischargeLimit",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "ChargeStatus",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<float>(
                name: "ChargeLimit",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "BatteryTemperature",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "WarningStatus",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "FaultStatus",
                table: "BESSDatas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<float>(
                name: "ActivePowerP",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BatteryCurrent",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BatteryPower",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "BatteryVoltage",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellTemperatureAvg",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellTemperatureMax",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellTemperatureMin",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellVoltageAvg",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellVoltageMax",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "CellVoltageMin",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "FrequencyGrid",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InputCurrent",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InputPowerBESS",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InputVoltage",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "InverterEfficiency",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OutputCurrentGrid",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OutputPowerGrid",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "OutputVoltageGrid",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PowerFactor",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "ReactivePowerQ",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarChargeCurrent",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarChargeVoltage",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SolarPowerInput",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TemperatureInverter",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "BESSDatas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<float>(
                name: "TotalEnergyAbsorbed",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TotalEnergyDelivered",
                table: "BESSDatas",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
