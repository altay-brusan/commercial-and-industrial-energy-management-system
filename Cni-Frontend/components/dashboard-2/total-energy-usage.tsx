"use client";

import { totalEnergyUsage } from "@/lib/dashboard-2/mock-data";

export function TotalEnergyUsage() {
  const data = totalEnergyUsage;

  return (
    <div className="rounded-lg bg-muted/30 p-6 space-y-4">
      <div>
        <h2 className="text-lg font-semibold">Total Energy Usage</h2>
        <p className="text-sm text-muted-foreground">
          Production breakdown by source
        </p>
      </div>

      <div className="text-3xl font-semibold tracking-tight">
        {data.totalKWh}
        <span className="text-sm font-normal text-muted-foreground ml-1">
          kWh
        </span>
      </div>

      {/* Multi-segment progress bar */}
      <div className="space-y-3">
        <div className="h-3 rounded-full bg-muted overflow-hidden flex">
          {data.sources.map((source) => (
            <div
              key={source.label}
              className={`h-full ${source.color}`}
              style={{ width: `${source.percentage}%` }}
            />
          ))}
        </div>
        <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
          {data.sources.map((source) => (
            <div key={source.label} className="flex items-center gap-1.5">
              <div className={`w-2 h-2 rounded-full ${source.color}`} />
              <span>
                {source.label}: {source.value} kWh
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Active / Reactive Power */}
      <div className="grid grid-cols-2 gap-4 pt-4 border-t border-border/50">
        <div>
          <p className="text-sm text-muted-foreground mb-1">Active Power</p>
          <span className="text-2xl font-semibold">
            {data.activePowerKW}
            <span className="text-sm font-normal text-muted-foreground ml-1">
              kW
            </span>
          </span>
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-1">Reactive Power</p>
          <span className="text-2xl font-semibold">
            {data.reactivePowerKVAR}
            <span className="text-sm font-normal text-muted-foreground ml-1">
              kVAR
            </span>
          </span>
        </div>
      </div>
    </div>
  );
}
