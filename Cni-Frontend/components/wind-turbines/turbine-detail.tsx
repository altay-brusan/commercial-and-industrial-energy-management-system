"use client";

import { useState } from "react";
import { ChevronLeft, RefreshCw, Activity } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { TurbineFaultTab } from "./turbine-fault-tab";
import { TurbineCurveTab } from "./turbine-curve-tab";
import type { WindTurbine } from "@/lib/wind-turbines/types";

interface TurbineDetailProps {
  turbine: WindTurbine;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

type DetailTab = "general" | "fault" | "curve" | "settings";

function formatValue(value: number | null, unit: string): string {
  if (value === null) return "--";
  return `${value.toLocaleString("en-US")} ${unit}`;
}

function MetricItem({ label, value, unit }: { label: string; value: number | null; unit: string }) {
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

const tabs: { id: DetailTab; label: string; disabled?: boolean }[] = [
  { id: "general", label: "General information" },
  { id: "fault", label: "Fault" },
  { id: "curve", label: "Curve" },
  { id: "settings", label: "Settings", disabled: true },
];

export function TurbineDetail({ turbine, onBack, onAlarmOpen }: TurbineDetailProps) {
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
          <h2 className="text-lg font-semibold">{turbine.deviceName}</h2>
          <span className="text-sm text-muted-foreground">
            Wind farm: {turbine.windFarm}
          </span>
          <span className="text-sm text-muted-foreground">
            Device model: {turbine.deviceModel}
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
            <MetricItem label="Yield today" value={turbine.yieldToday} unit="MWh" />
            <MetricItem label="Active power" value={turbine.activePower} unit="kW" />
            <MetricItem label="Rated power" value={turbine.ratedPower} unit="kW" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Rotor speed" value={turbine.rotorSpeed} unit="rpm" />
            <MetricItem label="Generator speed" value={turbine.generatorSpeed} unit="rpm" />
            <MetricItem label="Generator power" value={turbine.generatorPower} unit="kW" />
          </MetricGrid>

          {/* Wind Conditions */}
          <SectionTitle title="Wind Conditions" />
          <MetricGrid>
            <MetricItem label="Wind speed" value={turbine.windSpeed} unit="m/s" />
            <MetricItem label="Wind direction" value={turbine.windDirection} unit="°" />
            <MetricItem label="Ambient temperature" value={turbine.ambientTemp} unit="°C" />
          </MetricGrid>

          {/* Generator & Electrical */}
          <SectionTitle title="Generator & Electrical" />
          <MetricGrid>
            <MetricItem label="Generator power" value={turbine.generatorPower} unit="kW" />
            <MetricItem label="Reactive power" value={turbine.reactivePower} unit="kVar" />
            <MetricItem label="Power factor" value={turbine.powerFactor} unit="" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Grid frequency" value={turbine.gridFrequency} unit="Hz" />
            <MetricItem label="Grid voltage" value={turbine.gridVoltage} unit="V" />
          </MetricGrid>

          {/* Pitch & Yaw */}
          <SectionTitle title="Pitch & Yaw" />
          <MetricGrid>
            <MetricItem label="Blade 1 pitch angle" value={turbine.pitchAngles[0]} unit="°" />
            <MetricItem label="Blade 2 pitch angle" value={turbine.pitchAngles[1]} unit="°" />
            <MetricItem label="Blade 3 pitch angle" value={turbine.pitchAngles[2]} unit="°" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Nacelle position" value={turbine.nacellePosition} unit="°" />
            <MetricItem label="Yaw error" value={turbine.yawError} unit="°" />
          </MetricGrid>

          {/* Drive Train Temperatures */}
          <SectionTitle title="Drive Train Temperatures" />
          <div className="border rounded-md overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow className="hover:bg-transparent">
                  <TableHead className="bg-muted w-[250px]">Component</TableHead>
                  <TableHead className="bg-muted">Temperature</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {[
                  { label: "Generator", value: turbine.generatorTemp },
                  { label: "Gearbox Oil", value: turbine.gearboxOilTemp },
                  { label: "Main Bearing", value: turbine.mainBearingTemp },
                  { label: "Control Cabinet", value: turbine.controlCabinetTemp },
                ].map((item) => (
                  <TableRow key={item.label}>
                    <TableCell className="font-medium">{item.label}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span>{formatValue(item.value, "°C")}</span>
                        <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {/* Cumulative Data */}
          <SectionTitle title="Cumulative Data" />
          <MetricGrid>
            <MetricItem label="Total yield" value={turbine.totalYield} unit="MWh" />
            <MetricItem label="Total operating hours" value={turbine.totalOperatingHours} unit="h" />
            <MetricItem label="Daily operating time" value={turbine.dailyOperatingTime} unit="h" />
          </MetricGrid>

          {/* Device Information */}
          <SectionTitle title="Device Information" />
          <div className="border rounded-md p-4 bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Current status</span>
                <Badge
                  className={
                    turbine.status === "normal"
                      ? "bg-energy-solar/15 text-energy-solar border-energy-solar/30"
                      : turbine.status === "warning"
                        ? "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30"
                        : turbine.status === "fault"
                          ? "bg-energy-grid/15 text-energy-grid border-energy-grid/30"
                          : "bg-muted text-muted-foreground"
                  }
                >
                  {turbine.status.charAt(0).toUpperCase() + turbine.status.slice(1)}
                </Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device name</span>
                <span className="text-sm font-medium">{turbine.deviceName}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device model</span>
                <span className="text-sm font-medium">{turbine.deviceModel}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Hub height</span>
                <span className="text-sm font-medium">{turbine.hubHeight} m</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Rotor diameter</span>
                <span className="text-sm font-medium">{turbine.rotorDiameter} m</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Commissioning date</span>
                <span className="text-sm font-medium">{turbine.commissioningDate}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Manufacturer</span>
                <span className="text-sm font-medium">{turbine.manufacturer}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">S/N</span>
                <span className="text-sm font-medium font-mono">{turbine.serialNumber}</span>
              </div>
            </div>
          </div>

          <div className="h-6" />
        </div>
      )}

      {activeTab === "fault" && (
        <TurbineFaultTab turbineId={turbine.id} onAlarmOpen={onAlarmOpen} />
      )}

      {activeTab === "curve" && (
        <TurbineCurveTab turbineId={turbine.id} />
      )}
    </div>
  );
}
