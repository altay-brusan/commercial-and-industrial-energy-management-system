"use client";

import { Sun, AlertTriangle, TrendingUp } from "lucide-react";
import { solarCapacityData } from "@/lib/dashboard-2/mock-data";

export function SolarEnergyCapacity() {
  const data = solarCapacityData;
  const capacityPercent =
    data.maxCapacityKW > 0
      ? (data.currentPowerKW / data.maxCapacityKW) * 100
      : 0;

  return (
    <div className="rounded-lg bg-muted/30 p-5 flex flex-col h-full">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-energy-solar/10">
            <Sun className="h-[18px] w-[18px] text-energy-solar" />
          </div>
          <div>
            <p className="text-sm font-medium">Solar</p>
            <p className="text-xs text-muted-foreground">PV Array</p>
          </div>
        </div>
        {data.maintenanceRequired && (
          <div className="flex items-center gap-1.5 text-xs text-energy-consumption">
            <AlertTriangle className="h-3.5 w-3.5" />
            Maintenance
          </div>
        )}
      </div>

      {/* Main Metric */}
      <div className="mb-4">
        <span className="text-3xl font-semibold tracking-tight">
          {data.currentPowerKW.toFixed(1)}
        </span>
        <span className="text-sm font-normal text-muted-foreground ml-1">kW</span>
        <p className="text-xs text-muted-foreground mt-0.5">Current Power</p>
      </div>

      {/* Bottom section pushed down */}
      <div className="mt-auto space-y-4">
        {/* Capacity bar */}
        <div>
          <div className="flex items-center justify-between text-xs mb-1.5">
            <span className="text-muted-foreground">Capacity</span>
            <span className="font-medium">
              {data.currentPowerKW.toFixed(0)} / {data.maxCapacityKW} kW
            </span>
          </div>
          <div className="h-2 rounded-full bg-muted overflow-hidden">
            <div
              className="h-full rounded-full bg-energy-solar transition-all duration-700"
              style={{ width: `${capacityPercent}%` }}
            />
          </div>
        </div>

        {/* Stats grid */}
        <div className="grid grid-cols-2 gap-2">
        <div className="rounded-md bg-background/60 p-2.5">
          <div className="flex items-center gap-1.5 mb-1">
            <TrendingUp className="h-3 w-3 text-muted-foreground" />
            <span className="text-[10px] text-muted-foreground">Produced</span>
          </div>
          <p className="text-sm font-semibold">{data.energyProducedKWh.toFixed(1)} kWh</p>
        </div>
        <div className="rounded-md bg-background/60 p-2.5">
          <div className="flex items-center gap-1.5 mb-1">
            <Sun className="h-3 w-3 text-muted-foreground" />
            <span className="text-[10px] text-muted-foreground">Temp</span>
          </div>
          <p className="text-sm font-semibold">{data.temperatureC}&deg;C</p>
        </div>
      </div>
      </div>
    </div>
  );
}
