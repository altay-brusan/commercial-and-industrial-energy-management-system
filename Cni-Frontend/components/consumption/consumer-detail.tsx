"use client";

import { useState } from "react";
import { ChevronLeft, RefreshCw, Activity } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ConsumerFaultTab } from "./consumer-fault-tab";
import { ConsumerCurveTab } from "./consumer-curve-tab";
import type { Consumer, ConsumerType } from "@/lib/consumption/types";

interface ConsumerDetailProps {
  consumer: Consumer;
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

const consumerTypeLabels: Record<ConsumerType, string> = {
  "commercial-building": "Commercial Building",
  "industrial-facility": "Industrial Facility",
  "ev-charging-station": "EV Charging Station",
  "hvac-system": "HVAC System",
  "lighting-system": "Lighting System",
  "data-center": "Data Center",
  "water-pumping": "Water Pumping",
  "cold-storage": "Cold Storage",
};

const drStatusLabels: Record<string, string> = {
  enrolled: "Enrolled",
  active: "Active",
  standby: "Standby",
  "not-enrolled": "Not Enrolled",
};

export function ConsumerDetail({ consumer, onBack, onAlarmOpen }: ConsumerDetailProps) {
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
          <h2 className="text-lg font-semibold">{consumer.name}</h2>
          <span className="text-sm text-muted-foreground">
            Location: {consumer.location}
          </span>
          <span className="text-sm text-muted-foreground">
            Meter: {consumer.meterModel}
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

          {/* Electrical */}
          <SectionTitle title="Electrical" />
          <MetricGrid>
            <MetricItem label="Active power" value={consumer.activePower} unit="kW" />
            <MetricItem label="Reactive power" value={consumer.reactivePower} unit="kVar" />
            <MetricItem label="Power factor" value={consumer.powerFactor} unit="" />
          </MetricGrid>
          <MetricGrid>
            <MetricItem label="Peak demand today" value={consumer.peakDemandToday} unit="kW" />
            <MetricItem label="Contracted capacity" value={consumer.contractedCapacity} unit="kW" />
            <MetricItem label="Load factor" value={consumer.loadFactor} unit="%" />
          </MetricGrid>

          {/* Consumption */}
          <SectionTitle title="Consumption" />
          <MetricGrid>
            <MetricItem label="Consumption today" value={consumer.consumptionToday} unit="kWh" />
            <MetricItem label="Consumption MTD" value={consumer.consumptionMTD} unit="MWh" />
            <StringMetric label="Estimated cost today" value={consumer.estimatedCostToday !== null ? `$${consumer.estimatedCostToday.toLocaleString("en-US")}` : null} />
          </MetricGrid>

          {/* Demand Response */}
          <SectionTitle title="Demand Response" />
          <MetricGrid>
            <StringMetric label="Program status" value={drStatusLabels[consumer.drStatus]} />
            <MetricItem label="Curtailable capacity" value={consumer.curtailableCapacity} unit="kW" />
            <MetricItem label="Current curtailment" value={consumer.currentCurtailment} unit="kW" />
          </MetricGrid>
          <MetricGrid>
            <StringMetric label="DR events YTD" value={String(consumer.drEventsYTD)} />
          </MetricGrid>

          {/* Device Information */}
          <SectionTitle title="Device Information" />
          <div className="border rounded-md p-4 bg-muted/30">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Current status</span>
                <Badge
                  className={
                    consumer.status === "normal"
                      ? "bg-status-operational/15 text-status-operational border-status-operational/30"
                      : consumer.status === "warning"
                        ? "bg-status-warning/15 text-status-warning border-status-warning/30"
                        : consumer.status === "fault"
                          ? "bg-severity-critical/15 text-severity-critical border-severity-critical/30"
                          : consumer.status === "curtailed"
                            ? "bg-severity-medium/15 text-severity-medium border-severity-medium/30"
                            : "bg-muted text-muted-foreground"
                  }
                >
                  {consumer.status.charAt(0).toUpperCase() + consumer.status.slice(1)}
                </Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Consumer name</span>
                <span className="text-sm font-medium">{consumer.name}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Consumer type</span>
                <span className="text-sm font-medium">{consumerTypeLabels[consumer.type]}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Meter model</span>
                <span className="text-sm font-medium">{consumer.meterModel}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Commissioning date</span>
                <span className="text-sm font-medium">{consumer.commissioningDate}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">Manufacturer</span>
                <span className="text-sm font-medium">{consumer.manufacturer}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-sm text-muted-foreground">S/N</span>
                <span className="text-sm font-medium font-mono">{consumer.serialNumber}</span>
              </div>
            </div>
          </div>

          <div className="h-6" />
        </div>
      )}

      {activeTab === "fault" && (
        <ConsumerFaultTab consumerId={consumer.id} onAlarmOpen={onAlarmOpen} />
      )}

      {activeTab === "curve" && (
        <ConsumerCurveTab consumerId={consumer.id} />
      )}
    </div>
  );
}
