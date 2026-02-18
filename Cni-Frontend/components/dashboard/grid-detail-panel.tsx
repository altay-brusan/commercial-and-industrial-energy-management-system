"use client";

import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { gridMetrics } from "@/lib/dashboard/mock-data";
import { cn } from "@/lib/utils";

interface GridDetailPanelProps {
  onClose: () => void;
}

interface InfoItem {
  label: string;
  value: string;
}

function InfoSection({ title, items }: { title: string; items: InfoItem[] }) {
  return (
    <div>
      <h3 className="text-sm font-semibold text-foreground mb-3 pb-2 border-b border-border">
        {title}
      </h3>
      <div className="space-y-2">
        {items.map((item) => (
          <div key={item.label} className="flex justify-between items-baseline">
            <span className="text-sm text-muted-foreground">{item.label}</span>
            <span className="text-sm font-medium text-foreground">{item.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

function getConnectionColor(status: string) {
  switch (status) {
    case "connected":
      return "bg-energy-solar/15 text-energy-solar border-energy-solar/30";
    case "disconnected":
      return "bg-energy-grid/15 text-energy-grid border-energy-grid/30";
    case "island":
      return "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30";
    default:
      return "bg-muted text-muted-foreground border-border";
  }
}

function getQualityColor(quality: string) {
  switch (quality) {
    case "excellent":
    case "good":
      return "bg-energy-solar/15 text-energy-solar border-energy-solar/30";
    case "fair":
      return "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30";
    case "poor":
      return "bg-energy-grid/15 text-energy-grid border-energy-grid/30";
    default:
      return "bg-muted text-muted-foreground border-border";
  }
}

export function GridDetailPanel({ onClose }: GridDetailPanelProps) {
  const m = gridMetrics;
  const netFlow = m.importPower - m.exportPower;

  return (
    <aside className="w-[420px] shrink-0 border-l border-border flex flex-col bg-background overflow-hidden">
      <div className="shrink-0 flex items-center justify-between px-5 py-4 border-b border-border bg-background">
        <h2 className="text-lg font-semibold">Grid Details</h2>
        <Button variant="ghost" size="icon" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto p-5 space-y-6">
        {/* Status Overview */}
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <Badge variant="outline" className={cn("capitalize", getConnectionColor(m.connectionStatus))}>
              {m.connectionStatus}
            </Badge>
            <Badge variant="outline" className={cn("capitalize", getQualityColor(m.quality))}>
              {m.quality} quality
            </Badge>
          </div>
          <div className="flex items-baseline gap-1">
            <span className="text-3xl font-semibold">{netFlow > 0 ? netFlow.toFixed(1) : "0"}</span>
            <span className="text-lg text-muted-foreground">kW</span>
            <span className="text-sm text-muted-foreground ml-1">
              {netFlow > 0 ? "importing" : netFlow < 0 ? "exporting" : "balanced"}
            </span>
          </div>
        </div>

        <InfoSection
          title="Connection"
          items={[
            { label: "Status", value: m.connectionStatus.charAt(0).toUpperCase() + m.connectionStatus.slice(1) },
            { label: "Quality", value: m.quality.charAt(0).toUpperCase() + m.quality.slice(1) },
            { label: "Protection", value: "Anti-islanding active" },
          ]}
        />

        <InfoSection
          title="Power"
          items={[
            { label: "Import Power", value: `${m.importPower} kW` },
            { label: "Export Power", value: `${m.exportPower} kW` },
            { label: "Net Flow", value: `${netFlow.toFixed(1)} kW ${netFlow >= 0 ? "(import)" : "(export)"}` },
          ]}
        />

        <InfoSection
          title="Electrical"
          items={[
            { label: "Frequency", value: `${m.frequency} Hz` },
            { label: "Voltage", value: `${m.voltage} V` },
            { label: "Power Factor", value: `${m.powerFactor}` },
          ]}
        />

        <InfoSection
          title="Tariff"
          items={[
            { label: "Current Rate", value: m.tariff.charAt(0).toUpperCase() + m.tariff.slice(1) },
            { label: "Peak Rate", value: "$0.28/kWh" },
            { label: "Off-Peak Rate", value: "$0.12/kWh" },
            { label: "Shoulder Rate", value: "$0.19/kWh" },
          ]}
        />

        <InfoSection
          title="Daily Totals"
          items={[
            { label: "Total Imported", value: "186.4 kWh" },
            { label: "Total Exported", value: "42.1 kWh" },
            { label: "Net Consumption", value: "144.3 kWh" },
            { label: "Estimated Cost", value: "$38.52" },
          ]}
        />

        <InfoSection
          title="Maintenance"
          items={[
            { label: "Grid Meter", value: "Schneider ION8650" },
            { label: "CT Ratio", value: "2000/5" },
            { label: "Last Calibration", value: "November 20, 2025" },
            { label: "Next Calibration", value: "November 20, 2026" },
          ]}
        />
      </div>
    </aside>
  );
}
