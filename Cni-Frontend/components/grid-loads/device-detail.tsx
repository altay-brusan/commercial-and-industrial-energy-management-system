"use client";

import { useState } from "react";
import { ChevronLeft, RefreshCw, Activity } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import { format } from "date-fns";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DeviceFaultTab } from "./device-fault-tab";
import { DeviceCurveTab } from "./device-curve-tab";
import type { GridDevice } from "@/lib/grid-loads/types";

interface DeviceDetailProps {
  device: GridDevice;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

type DetailTab = "general" | "fault" | "curve" | "settings";

function formatValue(value: number | null, unit: string): string {
  if (value === null) return "--";
  return `${value.toLocaleString("en-US")} ${unit}`;
}

function MetricItem({ label, value, unit }: { label: React.ReactNode; value: number | null; unit: string }) {
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">{formatValue(value, unit)}</span>
        <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
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

function StringMetric({ label, value }: { label: string; value: string | null }) {
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium">{value ?? "--"}</span>
    </div>
  );
}

const tabs: { id: DetailTab; label: string; disabled?: boolean }[] = [
  { id: "general", label: "General information" },
  { id: "fault", label: "Fault" },
  { id: "curve", label: "Curve" },
  { id: "settings", label: "Settings", disabled: true },
];

const deviceTypeLabels: Record<string, string> = {
  "connection-point": "Grid Connection",
  "transformer": "Transformer",
  "power-meter": "Power Meter",
  "protection-relay": "Protection Relay",
  "circuit-breaker": "Circuit Breaker",
  "switchgear": "Switchgear",
};

export function DeviceDetail({ device, onBack, onAlarmOpen }: DeviceDetailProps) {
  const [activeTab, setActiveTab] = useState<DetailTab>("general");

  const now = new Date();
  const updateTimeStr = `${now.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" }).replace(/ /g, "/")} ${now.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" })}`;

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">{device.deviceName}</h2>
          <span className="text-sm text-muted-foreground">
            Location: {device.location}
          </span>
          <span className="text-sm text-muted-foreground">
            Device model: {device.deviceModel}
          </span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => !tab.disabled && setActiveTab(tab.id)}
              disabled={tab.disabled}
              className={`px-4 pb-2 text-sm transition-colors border-b-2 ${
                activeTab === tab.id
                  ? "border-foreground text-foreground font-medium"
                  : tab.disabled
                    ? "border-transparent text-muted-foreground/50 cursor-not-allowed"
                    : "border-transparent text-muted-foreground hover:text-foreground cursor-pointer"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      {activeTab === "general" && (
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {/* Update time */}
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-semibold">Measuring point parameter</h3>
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <span>Data update time: {updateTimeStr}</span>
              <Button variant="ghost" size="icon" className="h-6 w-6">
                <RefreshCw className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>

          {/* Overview */}
          <SectionTitle title="Overview" />
          <MetricGrid>
            <MetricItem label="Active power" value={device.activePower} unit="kW" />
            <MetricItem label="Reactive power" value={device.reactivePower} unit="kVar" />
            <MetricItem label="Apparent power" value={device.apparentPower} unit="kVA" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Power factor" value={device.powerFactor} unit="" />
            <MetricItem label="Frequency" value={device.frequency} unit="Hz" />
            <StringMetric label="Power flow direction" value={device.powerFlowDirection ? device.powerFlowDirection.charAt(0).toUpperCase() + device.powerFlowDirection.slice(1) : null} />
          </MetricGrid>

          {/* Voltage & Current */}
          <SectionTitle title="Voltage & Current" />
          <MetricGrid>
            <MetricItem label="Phase A voltage" value={device.voltagePhaseA} unit="V" />
            <MetricItem label="Phase B voltage" value={device.voltagePhaseB} unit="V" />
            <MetricItem label="Phase C voltage" value={device.voltagePhaseC} unit="V" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Phase A current" value={device.currentPhaseA} unit="A" />
            <MetricItem label="Phase B current" value={device.currentPhaseB} unit="A" />
            <MetricItem label="Phase C current" value={device.currentPhaseC} unit="A" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label={<AbbrTooltip label="THD" title="Total Harmonic Distortion" description="Measure of power quality deviation from ideal waveform" />} value={device.thd} unit="%" />
          </MetricGrid>

          {/* Transformer Data (conditional) */}
          {device.deviceType === "transformer" && (
            <>
              <SectionTitle title="Transformer Data" />
              <MetricGrid>
                <MetricItem label="Oil temperature" value={device.oilTemperature} unit="°C" />
                <MetricItem label="Winding temperature" value={device.windingTemperature} unit="°C" />
                <MetricItem label="Tap position" value={device.tapPosition} unit="" />
              </MetricGrid>
              <MetricGrid>
                <MetricItem label="Loading" value={device.loadingPercent} unit="%" />
                <StringMetric label="Voltage ratio" value={device.voltageRatio} />
              </MetricGrid>
            </>
          )}

          {/* Transformer Lifecycle */}
          {device.deviceType === "transformer" && device.tapChangerOperationCount !== null && (
            <>
              <SectionTitle title="Transformer Lifecycle" />
              <MetricGrid>
                <MetricItem label="Tap changer operations" value={device.tapChangerOperationCount} unit="" />
                <MetricItem label="Insulation resistance" value={device.insulationResistance} unit="MOhm" />
                <MetricItem label="Total load throughput" value={device.totalLoadMVAh} unit="MVAh" />
              </MetricGrid>
              <MetricGrid>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-muted-foreground">Oil condition</span>
                  <Badge className={
                    device.oilCondition === "good"
                      ? "bg-energy-solar/15 text-energy-solar border-energy-solar/30"
                      : device.oilCondition === "fair"
                        ? "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30"
                        : "bg-energy-grid/15 text-energy-grid border-energy-grid/30"
                  }>
                    {device.oilCondition ? device.oilCondition.charAt(0).toUpperCase() + device.oilCondition.slice(1) : "--"}
                  </Badge>
                </div>
                <StringMetric label="Last oil test" value={device.oilTestDate} />
                <StringMetric label="Designed life" value={device.designedLifeYears ? `${device.designedLifeYears} years` : null} />
              </MetricGrid>
            </>
          )}

          {/* Breaker/Relay Data (conditional) */}
          {(device.deviceType === "circuit-breaker" || device.deviceType === "protection-relay" || device.deviceType === "switchgear") && (
            <>
              <SectionTitle title="Breaker / Relay Data" />
              <MetricGrid>
                <StringMetric label="Breaker position" value={device.breakerPosition ? device.breakerPosition.charAt(0).toUpperCase() + device.breakerPosition.slice(1) : null} />
                <StringMetric label="Relay trip status" value={device.relayTripStatus !== null ? (device.relayTripStatus ? "Tripped" : "Normal") : null} />
                <StringMetric label="Last trip time" value={device.lastTripTime ? format(device.lastTripTime, "dd/MMM/yyyy HH:mm") : null} />
              </MetricGrid>
            </>
          )}

          {/* Breaker Lifecycle */}
          {device.deviceType === "circuit-breaker" && device.totalOperationCount !== null && (
            <>
              <SectionTitle title="Breaker Lifecycle" />
              <MetricGrid>
                <MetricItem label="Total operations" value={device.totalOperationCount} unit="" />
                <MetricItem label="Fault trips" value={device.tripCount} unit="" />
                <MetricItem label="Rated operations" value={device.ratedOperationCount} unit="" />
              </MetricGrid>
              <MetricGrid>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-muted-foreground">Operation life used</span>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-medium">
                      {device.ratedOperationCount ? ((device.totalOperationCount / device.ratedOperationCount) * 100).toFixed(1) : "--"}%
                    </span>
                    <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
                  </div>
                </div>
                <StringMetric label="Designed life" value={device.designedLifeYears ? `${device.designedLifeYears} years` : null} />
              </MetricGrid>
            </>
          )}

          {/* Energy */}
          {(device.importedToday !== null || device.exportedToday !== null) && (
            <>
              <SectionTitle title="Energy" />
              <MetricGrid>
                <MetricItem label="Energy imported today" value={device.importedToday} unit="kWh" />
                <MetricItem label="Energy exported today" value={device.exportedToday} unit="kWh" />
              </MetricGrid>
              <MetricGrid>
                <MetricItem label="Total imported" value={device.totalImported} unit="MWh" />
                <MetricItem label="Total exported" value={device.totalExported} unit="MWh" />
              </MetricGrid>
            </>
          )}

          {/* Maintenance & Warranty (for transformer / circuit-breaker with lifecycle data) */}
          {device.lastMaintenanceDate !== null && (
            <>
              <SectionTitle title="Maintenance & Warranty" />
              <MetricGrid>
                <StringMetric label="Last maintenance" value={device.lastMaintenanceDate} />
                <StringMetric label="Next maintenance due" value={device.nextMaintenanceDue} />
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-muted-foreground">Warranty expiry</span>
                  <div className="flex items-center gap-1.5">
                    <span className="text-sm font-medium">{device.warrantyExpiryDate ?? "--"}</span>
                    {device.warrantyExpiryDate && (() => {
                      const parts = device.warrantyExpiryDate.split("/");
                      const monthMap: Record<string, number> = {
                        Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
                        Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11,
                      };
                      const expDate = new Date(Number(parts[2]), monthMap[parts[1]], Number(parts[0]));
                      const expired = Date.now() > expDate.getTime();
                      return (
                        <Badge className={expired ? "bg-energy-grid/15 text-energy-grid border-energy-grid/30" : "bg-energy-solar/15 text-energy-solar border-energy-solar/30"}>
                          {expired ? "Expired" : "Active"}
                        </Badge>
                      );
                    })()}
                  </div>
                </div>
              </MetricGrid>
              <MetricGrid>
                <StringMetric label="Designed life" value={device.designedLifeYears ? `${device.designedLifeYears} years` : null} />
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-muted-foreground">Asset age</span>
                  <span className="text-sm font-medium">
                    {(() => {
                      const parts = device.commissioningDate.split("/");
                      const monthMap: Record<string, number> = {
                        Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5,
                        Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11,
                      };
                      const commDate = new Date(Number(parts[2]), monthMap[parts[1]], Number(parts[0]));
                      const years = ((Date.now() - commDate.getTime()) / (365.25 * 24 * 3600 * 1000)).toFixed(1);
                      return `${years} years`;
                    })()}
                  </span>
                </div>
              </MetricGrid>
            </>
          )}

          {/* Device Information */}
          <SectionTitle title="Device Information" />
          <div className="border rounded-md p-4 bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Current status</span>
                <Badge
                  className={
                    device.status === "normal"
                      ? "bg-energy-solar/15 text-energy-solar border-energy-solar/30"
                      : device.status === "warning"
                        ? "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30"
                        : device.status === "fault" || device.status === "tripped"
                          ? "bg-energy-grid/15 text-energy-grid border-energy-grid/30"
                          : "bg-muted text-muted-foreground"
                  }
                >
                  {device.status.charAt(0).toUpperCase() + device.status.slice(1)}
                </Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device name</span>
                <span className="text-sm font-medium">{device.deviceName}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device type</span>
                <span className="text-sm font-medium">{deviceTypeLabels[device.deviceType]}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device model</span>
                <span className="text-sm font-medium">{device.deviceModel}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Commissioning date</span>
                <span className="text-sm font-medium">{device.commissioningDate}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Manufacturer</span>
                <span className="text-sm font-medium">{device.manufacturer}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">S/N</span>
                <span className="text-sm font-medium font-mono">{device.serialNumber}</span>
              </div>
            </div>
          </div>

          <div className="h-6" />
        </div>
      )}

      {activeTab === "fault" && (
        <DeviceFaultTab deviceId={device.id} onAlarmOpen={onAlarmOpen} />
      )}

      {activeTab === "curve" && (
        <DeviceCurveTab deviceId={device.id} />
      )}
    </div>
  );
}
