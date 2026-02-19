"use client";

import { Activity, ArrowUpRight, ArrowDownLeft, Minus } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessUnit } from "@/lib/bess/types";
import { getChillerStateForUnit, getHvcuStateForUnit } from "@/lib/bess/mock-data";

interface BessSummaryTabProps {
  unit: BessUnit;
}

function formatValue(value: number | null, unit: string): string {
  if (value === null) return "--";
  return `${value.toLocaleString("en-US")} ${unit}`;
}

function MetricItem({ label, value, unit }: { label: React.ReactNode; value: number | null | string; unit?: string }) {
  const displayValue = typeof value === "string" ? value : formatValue(value, unit ?? "");
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">{displayValue}</span>
        {typeof value === "number" && <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />}
      </div>
    </div>
  );
}

function MetricGrid({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
      {children}
    </div>
  );
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-2 first:mt-0">
      {title}
    </h3>
  );
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-status-operational";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-severity-high";
  return "[&>[data-slot=progress-indicator]]:bg-severity-critical";
}

function StatusBadge({ active, onLabel, offLabel }: { active: boolean; onLabel?: string; offLabel?: string }) {
  return active ? (
    <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 text-xs">{onLabel ?? "On"}</Badge>
  ) : (
    <Badge variant="outline" className="text-muted-foreground text-xs">{offLabel ?? "Off"}</Badge>
  );
}

function formatCellLoc(loc: { bmuLoc: number; cellLoc: number }): string {
  return `BMU ${loc.bmuLoc} / Cell ${loc.cellLoc}`;
}

export function BessSummaryTab({ unit }: BessSummaryTabProps) {
  const { nameplate, live, lifecycle, connectedDevices } = unit;
  const chillerState = getChillerStateForUnit(unit.id);
  const hvcuState = getHvcuStateForUnit(unit.id);

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Nameplate */}
      <SectionTitle title="Nameplate Information" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Brand</span>
            <span className="text-sm font-medium">{nameplate.brand}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Model</span>
            <span className="text-sm font-medium">{nameplate.model}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Serial Number</span>
            <span className="text-sm font-medium font-mono">{nameplate.serialNumber}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Chemistry</span>
            <span className="text-sm font-medium">{nameplate.chemistry}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Commissioning Date</span>
            <span className="text-sm font-medium">{nameplate.commissioningDate}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Capacity</span>
            <span className="text-sm font-medium">{nameplate.capacityKWh} kWh</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Max Power</span>
            <span className="text-sm font-medium">{nameplate.maxPowerKW} kW</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Voltage Range</span>
            <span className="text-sm font-medium">{nameplate.voltageRange[0]}–{nameplate.voltageRange[1]} V</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Hierarchy</span>
            <span className="text-sm font-medium">
              {nameplate.rackCount}R / {nameplate.packsPerRack}P / {nameplate.modulesPerPack}M / {nameplate.cellsPerModule}C
            </span>
          </div>
        </div>
      </div>

      {/* Live Status */}
      <SectionTitle title="Live Status" />

      {/* SoC progress bar */}
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">State of Charge</span>
          <span className="text-sm font-semibold">{live.soc}%</span>
        </div>
        <Progress value={live.soc} className={`h-3 ${getSOCColor(live.soc)}`} />
      </div>

      <MetricGrid>
        <MetricItem label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} value={live.soh} unit="%" />
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Power</span>
          <div className="flex items-center gap-2">
            {live.power === 0 ? (
              <div className="flex items-center gap-1 text-sm font-medium text-muted-foreground">
                <Minus className="h-3.5 w-3.5" />
                0 kW
              </div>
            ) : live.power < 0 ? (
              <div className="flex items-center gap-1 text-sm font-medium text-energy-solar">
                <ArrowDownLeft className="h-3.5 w-3.5" />
                {Math.abs(live.power)} kW
              </div>
            ) : (
              <div className="flex items-center gap-1 text-sm font-medium text-energy-battery">
                <ArrowUpRight className="h-3.5 w-3.5" />
                {live.power} kW
              </div>
            )}
            <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
          </div>
        </div>
        <MetricItem label="Voltage" value={live.voltage} unit="V" />
      </MetricGrid>

      <MetricGrid>
        <MetricItem label="Current" value={live.current} unit="A" />
        <MetricItem label="Frequency" value={live.frequency} unit="Hz" />
        <MetricItem label="Temp (Avg)" value={live.tempAvg} unit="°C" />
      </MetricGrid>

      <MetricGrid>
        <MetricItem label="Temp (Min)" value={live.tempMin} unit="°C" />
        <MetricItem label="Temp (Max)" value={live.tempMax} unit="°C" />
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Operating Mode</span>
          <Badge variant="secondary" className="capitalize">{unit.operatingMode}</Badge>
        </div>
      </MetricGrid>

      {/* Energy & Limits */}
      <SectionTitle title="Energy & Limits" />
      <div className="mb-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Remaining Energy</span>
          <span className="text-sm font-semibold">{live.remainingEnergyKwh} kWh</span>
        </div>
        <Progress
          value={(live.remainingEnergyKwh / nameplate.capacityKWh) * 100}
          className={`h-2 ${getSOCColor((live.remainingEnergyKwh / nameplate.capacityKWh) * 100)}`}
        />
      </div>
      <MetricGrid>
        <MetricItem label="Available Energy" value={live.availableEnergyKwh} unit="kWh" />
        <MetricItem label={<AbbrTooltip label="DMCC" title="Dynamic Max Charge Current" description="Maximum allowable charge current based on current battery state" />} value={live.dmccA} unit="A" />
        <MetricItem label={<AbbrTooltip label="DMDC" title="Dynamic Max Discharge Current" description="Maximum allowable discharge current based on current battery state" />} value={live.dmdcA} unit="A" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Output Voltage (+)" value={live.outVoltPosV} unit="V" />
        <MetricItem label="Output Voltage (−)" value={live.outVoltNegV} unit="V" />
        <MetricItem label="State Code" value={live.stateCode} unit="" />
      </MetricGrid>

      {/* Cell Extremes */}
      <SectionTitle title="Cell Extremes" />
      <MetricGrid>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Max Cell Voltage</span>
          <div className="text-right">
            <span className="text-sm font-medium">{live.cellVoltageMaxMv} mV</span>
            <span className="text-xs text-muted-foreground ml-2">{formatCellLoc(live.cellVoltageMaxLoc)}</span>
          </div>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Min Cell Voltage</span>
          <div className="text-right">
            <span className="text-sm font-medium">{live.cellVoltageMinMv} mV</span>
            <span className="text-xs text-muted-foreground ml-2">{formatCellLoc(live.cellVoltageMinLoc)}</span>
          </div>
        </div>
        <MetricItem label="Avg Cell Voltage" value={live.avgCellVoltageMv} unit="mV" />
      </MetricGrid>
      <MetricGrid>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Max Cell Temp</span>
          <div className="text-right">
            <span className="text-sm font-medium">{live.cellTempMaxC} °C</span>
            <span className="text-xs text-muted-foreground ml-2">{formatCellLoc(live.cellTempMaxLoc)}</span>
          </div>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Min Cell Temp</span>
          <div className="text-right">
            <span className="text-sm font-medium">{live.cellTempMinC} °C</span>
            <span className="text-xs text-muted-foreground ml-2">{formatCellLoc(live.cellTempMinLoc)}</span>
          </div>
        </div>
        <MetricItem label="Voltage Spread" value={live.cellVoltageMaxMv - live.cellVoltageMinMv} unit="mV" />
      </MetricGrid>

      {/* Contactors & Safety */}
      <SectionTitle title="Contactors & Safety" />
      <MetricGrid>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Positive Contactor</span>
          <StatusBadge active={live.contactorPositive} onLabel="Closed" offLabel="Open" />
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Negative Contactor</span>
          <StatusBadge active={live.contactorNegative} onLabel="Closed" offLabel="Open" />
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Isolation Resistance</span>
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">{live.isolationStatus} kΩ</span>
            {live.isolationStatus < 500 ? (
              <Badge className="bg-severity-high/15 text-severity-high border-severity-high/30 text-xs">Low</Badge>
            ) : (
              <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 text-xs">Normal</Badge>
            )}
          </div>
        </div>
      </MetricGrid>

      {/* Thermal Management (TMS) */}
      {chillerState && (
        <>
          <SectionTitle title="Thermal Management (TMS)" />
          <MetricGrid>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-muted-foreground">Operating Mode</span>
              <Badge variant="secondary" className="text-xs">{chillerState.operatingModeStr}</Badge>
            </div>
            <MetricItem label="Coolant Supply Temp" value={chillerState.outletCoolantTempC} unit="°C" />
            <MetricItem label="Coolant Return Temp" value={chillerState.inletCoolantTempC} unit="°C" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Coolant Pressure (In)" value={chillerState.inletCoolantPressure} unit="bar" />
            <MetricItem label="Coolant Pressure (Out)" value={chillerState.outletCoolantPressure} unit="bar" />
            <MetricItem label="Ambient Temp" value={chillerState.ambientTempC} unit="°C" />
          </MetricGrid>
          <MetricGrid>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-muted-foreground">Compressor</span>
              <div className="flex items-center gap-2">
                <StatusBadge active={chillerState.compressorStatus} />
                <span className="text-xs text-muted-foreground">{chillerState.compressorSpeed}%</span>
              </div>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-muted-foreground">Pump</span>
              <div className="flex items-center gap-2">
                <StatusBadge active={chillerState.pumpStatus} />
                <span className="text-xs text-muted-foreground">{chillerState.pumpSpeed}%</span>
              </div>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-muted-foreground">Fan / Heater</span>
              <div className="flex items-center gap-2">
                <StatusBadge active={chillerState.fanRunStatus} onLabel="Fan" offLabel="Fan" />
                <StatusBadge active={chillerState.electricHeaterStatus} onLabel="Heater" offLabel="Heater" />
              </div>
            </div>
          </MetricGrid>
        </>
      )}

      {/* HVCU Control */}
      {hvcuState && (
        <>
          <SectionTitle title="HVCU Control" />
          <div className="border rounded-md overflow-hidden bg-muted/30">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b bg-muted/50">
                  <th className="text-left px-4 py-2 text-xs font-medium text-muted-foreground">BCU #</th>
                  <th className="text-left px-4 py-2 text-xs font-medium text-muted-foreground">State</th>
                  <th className="text-left px-4 py-2 text-xs font-medium text-muted-foreground">Description</th>
                </tr>
              </thead>
              <tbody>
                {hvcuState.bcuStates.map((bcu) => (
                  <tr key={bcu.id} className="border-b last:border-b-0">
                    <td className="px-4 py-2 font-medium">BCU {bcu.id}</td>
                    <td className="px-4 py-2 font-mono text-xs">{bcu.controlState}</td>
                    <td className="px-4 py-2">
                      <Badge
                        variant="outline"
                        className={
                          bcu.stateStr === "Active"
                            ? "bg-status-operational/15 text-status-operational border-status-operational/30 text-xs"
                            : bcu.stateStr === "Fault"
                              ? "bg-severity-critical/15 text-severity-critical border-severity-critical/30 text-xs"
                              : "text-xs"
                        }
                      >
                        {bcu.stateStr}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-2">
            <MetricGrid>
              <div className="flex items-center justify-between py-2">
                <span className="text-sm text-muted-foreground">Isolation Control</span>
                <StatusBadge active={hvcuState.isolationControl} onLabel="Active" offLabel="Inactive" />
              </div>
            </MetricGrid>
          </div>
        </>
      )}

      {/* Health (preview) */}
      <SectionTitle title="Health" />
      <MetricGrid>
        <MetricItem label="Cycle Count" value={lifecycle.cycleCount} unit="" />
        <MetricItem label={<AbbrTooltip label="RTE" title="Round-Trip Efficiency" description="Ratio of energy discharged to energy charged. Formula: (Discharged / Charged) × 100%" />} value={lifecycle.roundTripEfficiency} unit="%" />
        <MetricItem label="Equiv. Full Cycles" value={lifecycle.equivalentFullCycles} unit="" />
      </MetricGrid>

      {/* Connected Devices */}
      <SectionTitle title="Connected Devices" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Inverter / PCS</span>
            <span className="text-sm font-medium">{connectedDevices.inverter}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Meter</span>
            <span className="text-sm font-medium">{connectedDevices.meter}</span>
          </div>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}
