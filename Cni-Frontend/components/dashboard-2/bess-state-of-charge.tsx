"use client";

import { Battery, BatteryCharging, Pause, ArrowUp } from "lucide-react";
import { bessStateData } from "@/lib/dashboard-2/mock-data";

export function BESSStateOfCharge() {
  const data = bessStateData;
  const soc = data.socPercent;

  const statusConfig = {
    charging: { icon: BatteryCharging, label: "Charging", color: "text-energy-solar" },
    discharging: { icon: ArrowUp, label: "Discharging", color: "text-energy-consumption" },
    idle: { icon: Pause, label: "Idle", color: "text-muted-foreground" },
  } as const;

  const status = statusConfig[data.status as keyof typeof statusConfig] ?? statusConfig.idle;
  const StatusIcon = status.icon;

  return (
    <div className="rounded-lg bg-muted/30 p-5 flex flex-col h-full">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-battery/10">
            <Battery className="h-[18px] w-[18px] text-energy-battery" />
          </div>
          <div>
            <p className="text-sm font-medium">{data.name}</p>
            <p className="text-xs text-muted-foreground">Battery Storage</p>
          </div>
        </div>
        <div className={`flex items-center gap-1.5 text-xs ${status.color}`}>
          <StatusIcon className="h-3 w-3" />
          {status.label}
        </div>
      </div>

      {/* Main Metric */}
      <div className="mb-4">
        <span className="text-3xl font-semibold tracking-tight">
          {soc.toFixed(1)}
        </span>
        <span className="text-sm font-normal text-muted-foreground ml-1">%</span>
        <p className="text-xs text-muted-foreground mt-0.5">State of Charge</p>
      </div>

      {/* Bottom section pushed down */}
      <div className="mt-auto space-y-4">
        {/* SOC bar */}
        <div>
          <div className="flex items-center justify-between text-xs mb-1.5">
            <span className="text-muted-foreground">Charge Level</span>
            <span className="font-medium">{soc.toFixed(0)}%</span>
          </div>
          <div className="h-2 rounded-full bg-muted overflow-hidden">
            <div
              className="h-full rounded-full bg-energy-battery transition-all duration-700"
              style={{ width: `${soc}%` }}
            />
          </div>
        </div>

        {/* Stats grid */}
        <div className="grid grid-cols-2 gap-2">
          <div className="rounded-md bg-background/60 p-2.5">
            <div className="flex items-center gap-1.5 mb-1">
              <Battery className="h-3 w-3 text-muted-foreground" />
              <span className="text-[10px] text-muted-foreground">Capacity</span>
            </div>
            <p className="text-sm font-semibold">Full</p>
          </div>
          <div className="rounded-md bg-background/60 p-2.5">
            <div className="flex items-center gap-1.5 mb-1">
              <StatusIcon className="h-3 w-3 text-muted-foreground" />
              <span className="text-[10px] text-muted-foreground">Health</span>
            </div>
            <p className="text-sm font-semibold text-energy-solar">Good</p>
          </div>
        </div>
      </div>
    </div>
  );
}
