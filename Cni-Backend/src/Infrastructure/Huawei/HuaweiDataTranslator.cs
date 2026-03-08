using System.Text.Json;
using Backend.Application.Common.Models.Huawei;
using Backend.Domain.Entities.DeviceData;

namespace Backend.Infrastructure.Huawei;

public class HuaweiDataTranslator
{
    /// <summary>
    /// Translates Huawei battery (devTypeId=39) real-time KPI to DeviceBatteryData.
    /// </summary>
    public DeviceBatteryData TranslateBattery(HuaweiDevRealKpiData kpi, Guid deviceId, DateTimeOffset timestamp)
    {
        var map = kpi.DataItemMap;

        var payload = new Dictionary<string, object?>();
        TryAdd(payload, "soc", map, "battery_soc");
        TryAdd(payload, "soh", map, "battery_soh");
        TryAdd(payload, "chDischargePower", map, "ch_discharge_power");
        TryAdd(payload, "busbarVoltage", map, "busbar_u");
        TryAdd(payload, "chargeEnergy", map, "charge_cap");
        TryAdd(payload, "dischargeEnergy", map, "discharge_cap");
        TryAdd(payload, "ratedCapacity", map, "rated_capacity");
        TryAdd(payload, "maxChargePower", map, "max_charge_power");
        TryAdd(payload, "maxDischargePower", map, "max_discharge_power");
        TryAdd(payload, "batteryStatus", map, "battery_status");

        // Preserve nested battery_unit_info as-is
        if (map.TryGetProperty("battery_unit_info", out var unitInfo))
        {
            payload["batteryUnitInfo"] = unitInfo;
        }

        return new DeviceBatteryData
        {
            DeviceId = deviceId,
            DeviceLocalTime = timestamp,
            Kind = "Battery.Energy",
            Payload = JsonSerializer.Serialize(payload)
        };
    }

    /// <summary>
    /// Translates Huawei inverter (devTypeId=1) real-time KPI to DevicePcsData.
    /// </summary>
    public DevicePcsData TranslatePcs(HuaweiDevRealKpiData kpi, Guid deviceId, DateTimeOffset timestamp)
    {
        var map = kpi.DataItemMap;

        var payload = new Dictionary<string, object?>();
        TryAdd(payload, "activePower", map, "active_power");
        TryAdd(payload, "reactivePower", map, "reactive_power");
        TryAdd(payload, "powerFactor", map, "power_factor");
        TryAdd(payload, "gridFrequency", map, "elec_freq");
        TryAdd(payload, "phaseAVoltage", map, "a_u");
        TryAdd(payload, "phaseBVoltage", map, "b_u");
        TryAdd(payload, "phaseCVoltage", map, "c_u");
        TryAdd(payload, "phaseACurrent", map, "a_i");
        TryAdd(payload, "phaseBCurrent", map, "b_i");
        TryAdd(payload, "phaseCCurrent", map, "c_i");
        TryAdd(payload, "lineABVoltage", map, "ab_u");
        TryAdd(payload, "lineBCVoltage", map, "bc_u");
        TryAdd(payload, "lineCAVoltage", map, "ca_u");
        TryAdd(payload, "temperature", map, "temperature");
        TryAdd(payload, "efficiency", map, "efficiency");
        TryAdd(payload, "inverterState", map, "inverter_state");
        TryAdd(payload, "dailyYield", map, "day_cap");
        TryAdd(payload, "totalYield", map, "total_cap");

        return new DevicePcsData
        {
            DeviceId = deviceId,
            DeviceLocalTime = timestamp,
            Kind = "Pcs.AcMeasurement",
            Payload = JsonSerializer.Serialize(payload)
        };
    }

    /// <summary>
    /// Translates Huawei inverter (devTypeId=1) real-time KPI to DeviceSolarData.
    /// </summary>
    public DeviceSolarData TranslateSolar(HuaweiDevRealKpiData kpi, Guid deviceId, DateTimeOffset timestamp)
    {
        var map = kpi.DataItemMap;

        var payload = new Dictionary<string, object?>();
        TryAdd(payload, "activePower", map, "active_power");
        TryAdd(payload, "mpptPower", map, "mppt_power");
        TryAdd(payload, "dailyYield", map, "day_cap");
        TryAdd(payload, "totalYield", map, "total_cap");

        // PV string voltages and currents (up to 28 strings)
        for (var i = 1; i <= 28; i++)
        {
            TryAdd(payload, $"pv{i}Voltage", map, $"pv{i}_u");
            TryAdd(payload, $"pv{i}Current", map, $"pv{i}_i");
        }

        return new DeviceSolarData
        {
            DeviceId = deviceId,
            DeviceLocalTime = timestamp,
            Kind = "Solar.Power",
            Payload = JsonSerializer.Serialize(payload)
        };
    }

    private static void TryAdd(Dictionary<string, object?> payload, string targetKey, JsonElement map, string sourceKey)
    {
        if (!map.TryGetProperty(sourceKey, out var value))
            return;

        if (value.ValueKind == JsonValueKind.Null)
            return;

        payload[targetKey] = value.ValueKind switch
        {
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.String => value.GetString(),
            _ => value
        };
    }
}
