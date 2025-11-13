using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Domain.Entities.Proto;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;

namespace Backend.Infrastructure.Data.Configurations;


public class AppSolarDataConfiguration : IEntityTypeConfiguration<AppSolarData>
{
    public void Configure(EntityTypeBuilder<AppSolarData> entity)
    {
        // DC Power
        entity.OwnsOne(e => e.PanelPower, dp =>
        {
            dp.Property(p => p.Voltage).HasColumnName("PanelPower_Voltage");
            dp.Property(p => p.Current).HasColumnName("PanelPower_Current");
            dp.Property(p => p.Power).HasColumnName("PanelPower_Power");
            dp.Property(p => p.TotalEnergyDaily).HasColumnName("PanelPower_TotalEnergyDaily");
            dp.Property(p => p.TotalEnergyLifetime).HasColumnName("PanelPower_TotalEnergyLifetime");
        });

        entity.OwnsOne(e => e.ChargerPower, dp =>
        {
            dp.Property(p => p.Voltage).HasColumnName("ChargerPower_Voltage");
            dp.Property(p => p.Current).HasColumnName("ChargerPower_Current");
            dp.Property(p => p.Power).HasColumnName("ChargerPower_Power");
            dp.Property(p => p.TotalEnergyDaily).HasColumnName("ChargerPower_TotalEnergyDaily");
            dp.Property(p => p.TotalEnergyLifetime).HasColumnName("ChargerPower_TotalEnergyLifetime");
        });

        entity.OwnsOne(e => e.DCOutput, dp =>
        {
            dp.Property(p => p.Voltage).HasColumnName("DCOutput_Voltage");
            dp.Property(p => p.Current).HasColumnName("DCOutput_Current");
            dp.Property(p => p.Power).HasColumnName("DCOutput_Power");
            dp.Property(p => p.TotalEnergyDaily).HasColumnName("DCOutput_TotalEnergyDaily");
            dp.Property(p => p.TotalEnergyLifetime).HasColumnName("DCOutput_TotalEnergyLifetime");
        });

        // AC Power
        entity.OwnsOne(e => e.ACOutput, ac =>
        {
            ac.Property(p => p.VoltagePhase1).HasColumnName("ACOutput_Voltage_Phase1");
            ac.Property(p => p.VoltagePhase2).HasColumnName("ACOutput_Voltage_Phase2");
            ac.Property(p => p.VoltagePhase3).HasColumnName("ACOutput_Voltage_Phase3");
            ac.Property(p => p.LineToLineVoltagePhase1).HasColumnName("ACOutput_LineToLineVoltage_Phase1");
            ac.Property(p => p.LineToLineVoltagePhase2).HasColumnName("ACOutput_LineToLineVoltage_Phase2");
            ac.Property(p => p.LineToLineVoltagePhase3).HasColumnName("ACOutput_LineToLineVoltage_Phase3");
            ac.Property(p => p.VoltageRms).HasColumnName("ACOutput_Voltage_RMS");
            ac.Property(p => p.CurrentPhase1).HasColumnName("ACOutput_Current_Phase1");
            ac.Property(p => p.CurrentPhase2).HasColumnName("ACOutput_Current_Phase2");
            ac.Property(p => p.CurrentPhase3).HasColumnName("ACOutput_Current_Phase3");
            ac.Property(p => p.ActivePowerPhase1).HasColumnName("ACOutput_ActivePower_Phase1");
            ac.Property(p => p.ActivePowerPhase2).HasColumnName("ACOutput_ActivePower_Phase2");
            ac.Property(p => p.ActivePowerPhase3).HasColumnName("ACOutput_ActivePower_Phase3");
            ac.Property(p => p.TotalActivePower).HasColumnName("ACOutput_TotalActivePower");
            ac.Property(p => p.ReactivePowerPhase1).HasColumnName("ACOutput_ReactivePower_Phase1");
            ac.Property(p => p.ReactivePowerPhase2).HasColumnName("ACOutput_ReactivePower_Phase2");
            ac.Property(p => p.ReactivePowerPhase3).HasColumnName("ACOutput_ReactivePower_Phase3");
            ac.Property(p => p.TotalReactivePower).HasColumnName("ACOutput_TotalReactivePower");
            ac.Property(p => p.ApparentPowerPhase1).HasColumnName("ACOutput_ApparentPower_Phase1");
            ac.Property(p => p.ApparentPowerPhase2).HasColumnName("ACOutput_ApparentPower_Phase2");
            ac.Property(p => p.ApparentPowerPhase3).HasColumnName("ACOutput_ApparentPower_Phase3");
            ac.Property(p => p.TotalApparentPower).HasColumnName("ACOutput_TotalApparentPower");
            ac.Property(p => p.PowerFactorPhase1).HasColumnName("ACOutput_PowerFactor_Phase1");
            ac.Property(p => p.PowerFactorPhase2).HasColumnName("ACOutput_PowerFactor_Phase2");
            ac.Property(p => p.PowerFactorPhase3).HasColumnName("ACOutput_PowerFactor_Phase3");
            ac.Property(p => p.TotalPowerFactor).HasColumnName("ACOutput_TotalPowerFactor");
            ac.Property(p => p.TotalEnergyDaily).HasColumnName("ACOutput_TotalEnergyDaily");
            ac.Property(p => p.TotalEnergyLifetime).HasColumnName("ACOutput_TotalEnergyLifetime");
            ac.Property(p => p.Frequency).HasColumnName("ACOutput_Frequency");
        });

      
        // Device Status
        entity.OwnsOne(e => e.Status, status =>
        {
            status.Property(s => s.OperatingMode).HasColumnName("OperatingMode");
            status.Property(s => s.WarningStatus).HasColumnName("WarningStatus");
            status.Property(s => s.FaultStatus).HasColumnName("FaultStatus");
        });
    }
}



public class AppBessDataConfiguration : IEntityTypeConfiguration<AppBESSData>
{
    public void Configure(EntityTypeBuilder<AppBESSData> entity)
    {
        entity.OwnsOne(e => e.DcPower, dp =>
        {
            dp.Property(p => p.Voltage).HasColumnName("DCOutput_Voltage");
            dp.Property(p => p.Current).HasColumnName("DCOutput_Current");
            dp.Property(p => p.Power).HasColumnName("DCOutput_Power");
            dp.Property(p => p.TotalEnergyDaily).HasColumnName("DCOutput_TotalEnergyDaily");
            dp.Property(p => p.TotalEnergyLifetime).HasColumnName("DCOutput_TotalEnergyLifetime");
        });

        // Device Status'ı Owned Entity olarak tanımla
        entity.OwnsOne(e => e.Status, st =>
        {
            st.Property(s => s.OperatingMode).HasColumnName("Status_OperatingMode");
            st.Property(s => s.WarningStatus).HasColumnName("Status_WarningStatus");
            st.Property(s => s.FaultStatus).HasColumnName("Status_FaultStatus");
        });

        // BESS Durumunu Enum olarak tanımla
        entity.Property(e => e.ChargeStatus)
            .HasColumnName("ChargeStatus")
            .HasConversion(
                v => v.ToString(),
                v => (AppBESSStatus)Enum.Parse(typeof(AppBESSStatus), v));
    }
}
public class AppPcsDataConfiguration : IEntityTypeConfiguration<AppPCSData>
{
    public void Configure(EntityTypeBuilder<AppPCSData> entity)
    {
        // AC Power
        entity.OwnsOne(e => e.AcPower, ac =>
        {
            ac.Property(p => p.VoltagePhase1).HasColumnName("ACOutput_Voltage_Phase1");
            ac.Property(p => p.VoltagePhase2).HasColumnName("ACOutput_Voltage_Phase2");
            ac.Property(p => p.VoltagePhase3).HasColumnName("ACOutput_Voltage_Phase3");
            ac.Property(p => p.LineToLineVoltagePhase1).HasColumnName("ACOutput_LineToLineVoltage_Phase1");
            ac.Property(p => p.LineToLineVoltagePhase2).HasColumnName("ACOutput_LineToLineVoltage_Phase2");
            ac.Property(p => p.LineToLineVoltagePhase3).HasColumnName("ACOutput_LineToLineVoltage_Phase3");
            ac.Property(p => p.VoltageRms).HasColumnName("ACOutput_Voltage_RMS");
            ac.Property(p => p.CurrentPhase1).HasColumnName("ACOutput_Current_Phase1");
            ac.Property(p => p.CurrentPhase2).HasColumnName("ACOutput_Current_Phase2");
            ac.Property(p => p.CurrentPhase3).HasColumnName("ACOutput_Current_Phase3");
            ac.Property(p => p.ActivePowerPhase1).HasColumnName("ACOutput_ActivePower_Phase1");
            ac.Property(p => p.ActivePowerPhase2).HasColumnName("ACOutput_ActivePower_Phase2");
            ac.Property(p => p.ActivePowerPhase3).HasColumnName("ACOutput_ActivePower_Phase3");
            ac.Property(p => p.TotalActivePower).HasColumnName("ACOutput_TotalActivePower");
            ac.Property(p => p.ReactivePowerPhase1).HasColumnName("ACOutput_ReactivePower_Phase1");
            ac.Property(p => p.ReactivePowerPhase2).HasColumnName("ACOutput_ReactivePower_Phase2");
            ac.Property(p => p.ReactivePowerPhase3).HasColumnName("ACOutput_ReactivePower_Phase3");
            ac.Property(p => p.TotalReactivePower).HasColumnName("ACOutput_TotalReactivePower");
            ac.Property(p => p.ApparentPowerPhase1).HasColumnName("ACOutput_ApparentPower_Phase1");
            ac.Property(p => p.ApparentPowerPhase2).HasColumnName("ACOutput_ApparentPower_Phase2");
            ac.Property(p => p.ApparentPowerPhase3).HasColumnName("ACOutput_ApparentPower_Phase3");
            ac.Property(p => p.TotalApparentPower).HasColumnName("ACOutput_TotalApparentPower");
            ac.Property(p => p.PowerFactorPhase1).HasColumnName("ACOutput_PowerFactor_Phase1");
            ac.Property(p => p.PowerFactorPhase2).HasColumnName("ACOutput_PowerFactor_Phase2");
            ac.Property(p => p.PowerFactorPhase3).HasColumnName("ACOutput_PowerFactor_Phase3");
            ac.Property(p => p.TotalPowerFactor).HasColumnName("ACOutput_TotalPowerFactor");
            ac.Property(p => p.TotalEnergyDaily).HasColumnName("ACOutput_TotalEnergyDaily");
            ac.Property(p => p.TotalEnergyLifetime).HasColumnName("ACOutput_TotalEnergyLifetime");
            ac.Property(p => p.Frequency).HasColumnName("ACOutput_Frequency");
        });
        entity.OwnsOne(e => e.DcPower, dp =>
        {
            dp.Property(p => p.Voltage).HasColumnName("DCOutput_Voltage");
            dp.Property(p => p.Current).HasColumnName("DCOutput_Current");
            dp.Property(p => p.Power).HasColumnName("DCOutput_Power");
            dp.Property(p => p.TotalEnergyDaily).HasColumnName("DCOutput_TotalEnergyDaily");
            dp.Property(p => p.TotalEnergyLifetime).HasColumnName("DCOutput_TotalEnergyLifetime");
        });
        entity.OwnsOne(e => e.Status, status =>
        {
            status.Property(s => s.OperatingMode).HasColumnName("OperatingMode");
            status.Property(s => s.WarningStatus).HasColumnName("WarningStatus");
            status.Property(s => s.FaultStatus).HasColumnName("FaultStatus");
        });
    }
}
public class AppSmartMeterDataConfiguration : IEntityTypeConfiguration<AppSmartMeterData>
{
    public void Configure(EntityTypeBuilder<AppSmartMeterData> entity)
    {
        // AC Power
        entity.OwnsOne(e => e.AcPower, ac =>
        {
            ac.Property(p => p.VoltagePhase1).HasColumnName("ACOutput_Voltage_Phase1");
            ac.Property(p => p.VoltagePhase2).HasColumnName("ACOutput_Voltage_Phase2");
            ac.Property(p => p.VoltagePhase3).HasColumnName("ACOutput_Voltage_Phase3");
            ac.Property(p => p.LineToLineVoltagePhase1).HasColumnName("ACOutput_LineToLineVoltage_Phase1");
            ac.Property(p => p.LineToLineVoltagePhase2).HasColumnName("ACOutput_LineToLineVoltage_Phase2");
            ac.Property(p => p.LineToLineVoltagePhase3).HasColumnName("ACOutput_LineToLineVoltage_Phase3");
            ac.Property(p => p.VoltageRms).HasColumnName("ACOutput_Voltage_RMS");
            ac.Property(p => p.CurrentPhase1).HasColumnName("ACOutput_Current_Phase1");
            ac.Property(p => p.CurrentPhase2).HasColumnName("ACOutput_Current_Phase2");
            ac.Property(p => p.CurrentPhase3).HasColumnName("ACOutput_Current_Phase3");
            ac.Property(p => p.ActivePowerPhase1).HasColumnName("ACOutput_ActivePower_Phase1");
            ac.Property(p => p.ActivePowerPhase2).HasColumnName("ACOutput_ActivePower_Phase2");
            ac.Property(p => p.ActivePowerPhase3).HasColumnName("ACOutput_ActivePower_Phase3");
            ac.Property(p => p.TotalActivePower).HasColumnName("ACOutput_TotalActivePower");
            ac.Property(p => p.ReactivePowerPhase1).HasColumnName("ACOutput_ReactivePower_Phase1");
            ac.Property(p => p.ReactivePowerPhase2).HasColumnName("ACOutput_ReactivePower_Phase2");
            ac.Property(p => p.ReactivePowerPhase3).HasColumnName("ACOutput_ReactivePower_Phase3");
            ac.Property(p => p.TotalReactivePower).HasColumnName("ACOutput_TotalReactivePower");
            ac.Property(p => p.ApparentPowerPhase1).HasColumnName("ACOutput_ApparentPower_Phase1");
            ac.Property(p => p.ApparentPowerPhase2).HasColumnName("ACOutput_ApparentPower_Phase2");
            ac.Property(p => p.ApparentPowerPhase3).HasColumnName("ACOutput_ApparentPower_Phase3");
            ac.Property(p => p.TotalApparentPower).HasColumnName("ACOutput_TotalApparentPower");
            ac.Property(p => p.PowerFactorPhase1).HasColumnName("ACOutput_PowerFactor_Phase1");
            ac.Property(p => p.PowerFactorPhase2).HasColumnName("ACOutput_PowerFactor_Phase2");
            ac.Property(p => p.PowerFactorPhase3).HasColumnName("ACOutput_PowerFactor_Phase3");
            ac.Property(p => p.TotalPowerFactor).HasColumnName("ACOutput_TotalPowerFactor");
            ac.Property(p => p.TotalEnergyDaily).HasColumnName("ACOutput_TotalEnergyDaily");
            ac.Property(p => p.TotalEnergyLifetime).HasColumnName("ACOutput_TotalEnergyLifetime");
            ac.Property(p => p.Frequency).HasColumnName("ACOutput_Frequency");
        });
        entity.OwnsOne(e => e.Status, status =>
        {
            status.Property(s => s.OperatingMode).HasColumnName("OperatingMode");
            status.Property(s => s.WarningStatus).HasColumnName("WarningStatus");
            status.Property(s => s.FaultStatus).HasColumnName("FaultStatus");
        });
    }
}

