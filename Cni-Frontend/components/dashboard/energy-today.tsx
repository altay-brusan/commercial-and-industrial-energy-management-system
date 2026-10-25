"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";

interface EnergySource {
  source: string;
  value: number;
  color: string;
}

interface EnergyData {
  production: {
    total: number;
    sources: EnergySource[];
  };
  consumption: number;
  netExport: number;
  selfConsumption: number;
}

const energyData: EnergyData = {
  production: {
    total: 335,
    sources: [
      { source: "Solar", value: 245.8, color: "bg-energy-solar" },
      { source: "BESS", value: 52.3, color: "bg-energy-battery" },
      { source: "Grid Import", value: 36.9, color: "bg-energy-grid" },
    ],
  },
  consumption: 287.2,
  netExport: 47.8,
  selfConsumption: 78.3,
};

export function EnergyToday() {
  const maxValue = Math.max(energyData.production.total, energyData.consumption);

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base">Energy Today</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-3">
          <div>
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm text-muted-foreground">Production</span>
              <span className="font-semibold text-energy-solar">
                {energyData.production.total} kWh
              </span>
            </div>
            <div className="h-3 rounded-full bg-muted overflow-hidden flex">
              {energyData.production.sources.map((source) => (
                <div
                  key={source.source}
                  className={`h-full ${source.color}`}
                  style={{
                    width: `${(source.value / energyData.production.total) * 100}%`,
                  }}
                />
              ))}
            </div>
            <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2 text-xs text-muted-foreground">
              {energyData.production.sources.map((source) => (
                <div key={source.source} className="flex items-center gap-1.5">
                  <div className={`w-2 h-2 rounded-full ${source.color}`} />
                  <span>
                    {source.source}: {source.value} kWh
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm text-muted-foreground">Consumption</span>
              <span className="font-semibold text-energy-consumption">
                {energyData.consumption} kWh
              </span>
            </div>
            <Progress
              value={(energyData.consumption / maxValue) * 100}
              className="h-3 [&>[data-slot=progress-indicator]]:bg-energy-consumption"
            />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4 pt-3 border-t">
          <div>
            <p className="text-xs text-muted-foreground">Net Export</p>
            <p
              className={`text-lg font-semibold ${
                energyData.netExport >= 0
                  ? "text-energy-solar"
                  : "text-energy-grid"
              }`}
            >
              {energyData.netExport >= 0 ? "+" : ""}
              {energyData.netExport} kWh
            </p>
          </div>
          <div>
            <p className="text-xs text-muted-foreground">Self-Consumption</p>
            <p className="text-lg font-semibold">
              {energyData.selfConsumption}%
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
