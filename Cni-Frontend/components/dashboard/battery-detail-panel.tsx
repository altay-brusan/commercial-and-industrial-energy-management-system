"use client";

import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Badge } from "@/components/ui/badge";
import { batteryMetrics } from "@/lib/dashboard/mock-data";
import { cn } from "@/lib/utils";

interface BatteryDetailPanelProps {
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

function getModeColor(mode: string) {
  switch (mode) {
    case "charging":
      return "bg-status-operational/15 text-status-operational border-status-operational/30";
    case "discharging":
      return "bg-severity-high/15 text-severity-high border-severity-high/30";
    case "idle":
    case "standby":
      return "bg-muted text-muted-foreground border-border";
    default:
      return "bg-muted text-muted-foreground border-border";
  }
}

export function BatteryDetailPanel({ onClose }: BatteryDetailPanelProps) {
  const m = batteryMetrics;

  return (
    <aside className="w-[420px] shrink-0 border-l border-border flex flex-col bg-background overflow-hidden">
      <div className="shrink-0 flex items-center justify-between px-5 py-4 border-b border-border bg-background">
        <h2 className="text-lg font-semibold">Battery Details</h2>
        <Button variant="ghost" size="icon" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto p-5 space-y-6">
        {/* SOC Visual */}
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">State of Charge</span>
            <Badge variant="outline" className={cn("capitalize", getModeColor(m.mode))}>
              {m.mode}
            </Badge>
          </div>
          <div className="space-y-1.5">
            <div className="flex items-baseline gap-1">
              <span className="text-3xl font-semibold">{m.soc}</span>
              <span className="text-lg text-muted-foreground">%</span>
            </div>
            <Progress value={m.soc} className="h-3" />
          </div>
          <p className="text-xs text-muted-foreground">
            {m.power} kW {m.mode} power
          </p>
        </div>

        <InfoSection
          title="Current Status"
          items={[
            { label: "Mode", value: m.mode.charAt(0).toUpperCase() + m.mode.slice(1) },
            { label: "Power", value: `${m.power} kW` },
            { label: "State of Charge", value: `${m.soc}%` },
            { label: "Capacity", value: `${m.capacity} kWh` },
          ]}
        />

        <InfoSection
          title="Electrical"
          items={[
            { label: "Voltage", value: `${m.voltage} V` },
            { label: "Current", value: `${m.current} A` },
            { label: "Temperature", value: `${m.temperature}°C` },
          ]}
        />

        <InfoSection
          title="Daily Statistics"
          items={[
            { label: "Today Charged", value: `${m.todayCharged} kWh` },
            { label: "Today Discharged", value: `${m.todayDischarged} kWh` },
            { label: "Net Energy", value: `${(m.todayCharged - m.todayDischarged).toFixed(1)} kWh` },
          ]}
        />

        <InfoSection
          title="Health & Lifecycle"
          items={[
            { label: "State of Health", value: `${m.stateOfHealth}%` },
            { label: "Cycle Count", value: `${m.cycleCount}` },
            { label: "Estimated Remaining", value: "5,153 cycles" },
          ]}
        />

        <InfoSection
          title="Maintenance"
          items={[
            { label: "Last Maintenance", value: "January 15, 2026" },
            { label: "Next Scheduled", value: "April 15, 2026" },
            { label: "Warranty Expiry", value: "March 10, 2034" },
          ]}
        />
      </div>
    </aside>
  );
}
