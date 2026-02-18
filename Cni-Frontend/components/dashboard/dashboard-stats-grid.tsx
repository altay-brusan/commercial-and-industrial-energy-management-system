"use client";

import { useRouter } from "next/navigation";
import { Sun, Battery, Zap, AlertTriangle } from "lucide-react";
import { DashboardStatsCard } from "./dashboard-stats-card";
import { solarMetrics, batteryMetrics, gridMetrics } from "@/lib/dashboard/mock-data";
import { getActiveAlarmCount, getCriticalAlarmCount } from "@/lib/alarms/mock-data";
import type { DetailPanelState } from "@/lib/dashboard/types";

interface DashboardStatsGridProps {
  onPanelChange: (state: DetailPanelState) => void;
}

export function DashboardStatsGrid({ onPanelChange }: DashboardStatsGridProps) {
  const router = useRouter();
  const activeAlarms = getActiveAlarmCount();
  const criticalAlarms = getCriticalAlarmCount();

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <DashboardStatsCard
        title="Solar Production"
        value={`${solarMetrics.currentPower} kW`}
        subtitle={`${solarMetrics.dailyGeneration} kWh today`}
        icon={Sun}
        colorClass="bg-energy-solar/20 text-energy-solar"
        onClick={() => onPanelChange({ type: "solar" })}
      />
      <DashboardStatsCard
        title="Battery Status"
        value={`${batteryMetrics.soc}%`}
        subtitle={`${batteryMetrics.power} kW ${batteryMetrics.mode}`}
        icon={Battery}
        colorClass="bg-energy-battery/20 text-energy-battery"
        onClick={() => onPanelChange({ type: "battery" })}
      />
      <DashboardStatsCard
        title="Grid Power"
        value={`${gridMetrics.importPower} kW`}
        subtitle={`${gridMetrics.connectionStatus} - ${gridMetrics.quality}`}
        icon={Zap}
        colorClass="bg-energy-grid/20 text-energy-grid"
        onClick={() => onPanelChange({ type: "grid" })}
      />
      <DashboardStatsCard
        title="Active Alarms"
        value={`${activeAlarms}`}
        subtitle={`${criticalAlarms} critical`}
        icon={AlertTriangle}
        colorClass={criticalAlarms > 0 ? "bg-energy-grid/20 text-energy-grid" : "bg-energy-consumption/20 text-energy-consumption"}
        onClick={() => router.push("/alarms")}
      />
    </div>
  );
}
