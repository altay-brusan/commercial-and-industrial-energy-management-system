"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { SystemHealth } from "@/components/dashboard/system-health";
import { ActiveAlarms } from "@/components/dashboard/active-alarms";
import { BESSStatus } from "@/components/dashboard/bess-status";
import { SolarStatus } from "@/components/dashboard/solar-status";
import { GridStatus } from "@/components/dashboard/grid-status";
import { WeatherWidget } from "@/components/dashboard/weather-widget";
import { EnergyToday } from "@/components/dashboard/energy-today";
import { FinancialSummary } from "@/components/dashboard/financial-summary";
import { QuickActions } from "@/components/dashboard/quick-actions";
import { KPIStrip } from "@/components/dashboard/kpi-strip";
import { PowerFlow } from "@/components/dashboard/power-flow";
import { SolarDetailPanel } from "@/components/dashboard/solar-detail-panel";

export default function Dashboard2() {
  const [solarDetailOpen, setSolarDetailOpen] = useState(false);

  const handleSourceClick = (sourceId: string) => {
    if (sourceId === "solar") {
      setSolarDetailOpen(true);
    }
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="dashboard" />
      <div className="flex flex-1 overflow-hidden">
        <main className="flex-1 overflow-y-auto p-4 md:p-6 space-y-4 md:space-y-6">
          {/* Tier 1: Critical */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <SystemHealth />
            <ActiveAlarms />
          </div>

          {/* Live Power Flow */}
          <div className="bg-card rounded-lg border border-border">
            <PowerFlow onSourceClick={handleSourceClick} />
          </div>

          {/* Tier 2: Important */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <BESSStatus />
            <SolarStatus />
            <GridStatus />
            <WeatherWidget />
          </div>

          {/* Tier 3: Informative */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <EnergyToday />
            <FinancialSummary />
            <QuickActions />
          </div>

          {/* KPI Strip */}
          <KPIStrip />
        </main>

        {solarDetailOpen && (
          <SolarDetailPanel onClose={() => setSolarDetailOpen(false)} />
        )}
      </div>
    </div>
  );
}
