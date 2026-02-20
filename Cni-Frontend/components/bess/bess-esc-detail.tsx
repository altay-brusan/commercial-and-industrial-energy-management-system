"use client";

import { useState } from "react";
import { ChevronLeft, Activity } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { BessAlarmsTab } from "./bess-alarms-tab";
import { BessEscSettingsTab } from "./settings/bess-esc-settings-tab";
import { ESC_TABS } from "@/lib/bess/types";
import type { BessEsc, BessComponentStatus } from "@/lib/bess/types";

interface BessEscDetailProps {
  esc: BessEsc;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  const styles: Record<BessComponentStatus, string> = {
    normal: "bg-status-operational/15 text-status-operational border-status-operational/30",
    warning: "bg-status-warning/15 text-status-warning border-status-warning/30",
    fault: "bg-status-critical/15 text-status-critical border-status-critical/30",
    offline: "bg-muted text-muted-foreground",
  };
  return <Badge className={styles[status]}>{status}</Badge>;
}

function Metric({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        <span className="text-sm font-medium">{value}</span>
        <Activity className="h-3.5 w-3.5 text-muted-foreground/40" />
      </div>
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

function EscDetailsTab({ esc }: { esc: BessEsc }) {
  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Device Info */}
      <SectionTitle title="Device Information" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Model</span>
            <span className="text-sm font-medium">{esc.deviceInfo.model}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Serial Number</span>
            <span className="text-sm font-medium font-mono">{esc.deviceInfo.serialNumber}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Firmware</span>
            <span className="text-sm font-medium">{esc.deviceInfo.firmware}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Protocol</span>
            <span className="text-sm font-medium">{esc.deviceInfo.protocol}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Modbus Address</span>
            <span className="text-sm font-medium">{esc.deviceInfo.modbusAddress}</span>
          </div>
        </div>
      </div>

      {/* Live Data */}
      <SectionTitle title="Live Data" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label="DC Voltage In" value={`${esc.live.dcVoltageIn} V`} />
        <Metric label="DC Voltage Out" value={`${esc.live.dcVoltageOut} V`} />
        <Metric label="DC Current" value={`${esc.live.dcCurrent} A`} />
        <Metric label="Power" value={`${esc.live.power} kW`} />
        <Metric label="Efficiency" value={`${esc.live.efficiency}%`} />
        <Metric label="Temperature" value={`${esc.live.temperature}°C`} />
      </div>

      {/* Safety */}
      <SectionTitle title="Safety & Status" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Isolation Resistance</span>
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">{esc.live.isolationResistance} k&Omega;</span>
            {esc.live.isolationResistance < 500 ? (
              <Badge className="bg-severity-high/15 text-severity-high border-severity-high/30 text-xs">Low</Badge>
            ) : (
              <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 text-xs">Normal</Badge>
            )}
          </div>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Operating State</span>
          <Badge variant="secondary" className="text-xs">{esc.live.operatingState}</Badge>
        </div>
      </div>

      <div className="h-6" />
    </div>
  );
}

export function BessEscDetail({ esc, onBack, onAlarmOpen }: BessEscDetailProps) {
  const [activeTab, setActiveTab] = useState("details");

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">ESC-1</h2>
          <ComponentStatusBadge status={esc.status} />
          <span className="text-sm text-muted-foreground">{esc.deviceInfo.model}</span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {ESC_TABS.map((tab) => (
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
      {activeTab === "details" && <EscDetailsTab esc={esc} />}
      {activeTab === "alarms" && (
        <BessAlarmsTab
          unitId={esc.unitId}
          onAlarmOpen={onAlarmOpen}
          sourceFilter="ESC"
        />
      )}
      {activeTab === "settings" && <BessEscSettingsTab escId={esc.id} />}
    </div>
  );
}
