"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { StatsRow } from "@/components/dashboard/stats-row";
import { EnergyChart } from "@/components/dashboard/energy-chart";
import { PowerFlow } from "@/components/dashboard/power-flow";
import { RecentActivity } from "@/components/dashboard/recent-activity";
import { SolarDetailPanel } from "@/components/dashboard/solar-detail-panel";

export default function Dashboard() {
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
        <main className="flex-1 overflow-y-auto">
          <StatsRow />
          <EnergyChart />
          <PowerFlow onSourceClick={handleSourceClick} />
          <RecentActivity />
        </main>

        {solarDetailOpen && (
          <SolarDetailPanel onClose={() => setSolarDetailOpen(false)} />
        )}
      </div>
    </div>
  );
}
