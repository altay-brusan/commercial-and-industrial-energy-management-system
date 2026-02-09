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
import { InverterFaultTab } from "./inverter-fault-tab";
import { InverterCurveTab } from "./inverter-curve-tab";
import type { SolarInverter } from "@/lib/solar-inverters/types";

interface InverterDetailProps {
  inverter: SolarInverter;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

type DetailTab = "general" | "fault" | "curve" | "settings";

function formatValue(value: number | null, unit: string): string {
  if (value === null) return "--";
  return `${value.toLocaleString()} ${unit}`;
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

export function InverterDetail({ inverter, onBack, onAlarmOpen }: InverterDetailProps) {
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
          <h2 className="text-lg font-semibold">{inverter.deviceName}</h2>
          <span className="text-sm text-muted-foreground">
            Plant name: {inverter.associatedPlant}
          </span>
          <span className="text-sm text-muted-foreground">
            Device model: {inverter.deviceModel}
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
            <MetricItem label="Yield today" value={inverter.yieldToday} unit="kWh" />
            <MetricItem label="Total active power" value={inverter.totalActivePower} unit="kW" />
            <MetricItem label="Total DC power" value={inverter.totalDcPower} unit="kW" />
          </MetricGrid>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-muted-foreground">Operating status</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium">{inverter.operatingStatus}</span>
              </div>
            </div>
            <MetricItem label="Phase A current" value={inverter.phaseACurrent} unit="A" />
            <MetricItem label="Phase B current" value={inverter.phaseBCurrent} unit="A" />
          </div>

          <MetricGrid>
            <MetricItem label="Phase C current" value={inverter.phaseCCurrent} unit="A" />
            <MetricItem label="Total reactive power" value={inverter.totalReactivePower} unit="var" />
            <MetricItem label="Insulation impedance to ground" value={inverter.insulationImpedance} unit="kΩ" />
          </MetricGrid>

          <MetricGrid>
            <MetricItem label="Grid frequency" value={inverter.gridFrequency} unit="Hz" />
            <MetricItem label="A-B line voltage" value={inverter.abLineVoltage} unit="V" />
            <MetricItem label="B-C line voltage" value={inverter.bcLineVoltage} unit="V" />
          </MetricGrid>

          <MetricGrid>
            <MetricItem label="C-A line voltage" value={inverter.caLineVoltage} unit="V" />
            <MetricItem label="Total power factor" value={inverter.totalPowerFactor} unit="" />
            <MetricItem label="Total yield" value={inverter.totalYield} unit="MWh" />
          </MetricGrid>

          {/* MPPT Information */}
          <SectionTitle title="MPPT information" />
          <div className="border rounded-md overflow-hidden">
            <Table>
              <TableHeader>
                <TableRow className="hover:bg-transparent">
                  <TableHead className="bg-muted w-[200px]">MPPT</TableHead>
                  <TableHead className="bg-muted w-[200px]">Voltage</TableHead>
                  <TableHead className="bg-muted">Current</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {inverter.mpptData.map((mppt) => (
                  <TableRow key={mppt.id}>
                    <TableCell className="font-medium">MPPT {mppt.id}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span>{formatValue(mppt.voltage, "V")}</span>
                        <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <span>{formatValue(mppt.current, "A")}</span>
                        <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {/* String Information */}
          <SectionTitle title="String information" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
            {inverter.stringCurrents.map((current, index) => (
              <MetricItem
                key={index}
                label={`String ${index + 1} current`}
                value={current}
                unit="A"
              />
            ))}
          </div>

          {/* Inverter Unit 1 */}
          <SectionTitle title="Inverter unit 1" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
            <MetricItem label="Total apparent power" value={inverter.totalApparentPower} unit="VA" />
            <MetricItem label="Daily operating time" value={inverter.dailyOperatingTime} unit="min" />
          </div>

          {/* Other Information */}
          <SectionTitle title="Other information" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
            <MetricItem label="Daily yield (theoretical)" value={inverter.dailyYieldTheoretical} unit="kWh" />
          </div>

          {/* Device Information */}
          <SectionTitle title="Device information" />
          <div className="border rounded-md p-4 bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Current status</span>
                <Badge
                  className={
                    inverter.status === "normal"
                      ? "bg-energy-solar/15 text-energy-solar border-energy-solar/30"
                      : "bg-muted text-muted-foreground"
                  }
                >
                  {inverter.status === "normal" ? "Normal" : inverter.operatingStatus}
                </Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device name</span>
                <span className="text-sm font-medium">{inverter.deviceName}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Device model</span>
                <span className="text-sm font-medium">{inverter.deviceModel}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Commissioning date</span>
                <span className="text-sm font-medium">{inverter.commissioningDate}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Manufacturer</span>
                <span className="text-sm font-medium">{inverter.manufacturer}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">S/N</span>
                <span className="text-sm font-medium font-mono">{inverter.serialNumber}</span>
              </div>
            </div>
          </div>

          <div className="h-6" />
        </div>
      )}

      {activeTab === "fault" && (
        <InverterFaultTab inverterId={inverter.id} onAlarmOpen={onAlarmOpen} />
      )}

      {activeTab === "curve" && (
        <InverterCurveTab inverterId={inverter.id} />
      )}
    </div>
  );
}
