"use client";

import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { WindTurbinesSidebar } from "@/components/wind-turbines/wind-turbines-sidebar";
import { TurbineTable } from "@/components/wind-turbines/turbine-table";
import { TurbineDetail } from "@/components/wind-turbines/turbine-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { EnergyAnalyticsView } from "@/components/wind-turbines/energy-analytics-view";
import { PerformanceView } from "@/components/wind-turbines/performance-view";
import { windTurbines } from "@/lib/wind-turbines/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { WindTurbine } from "@/lib/wind-turbines/types";
import type { Alarm } from "@/lib/alarms/types";
import type { WindTurbineView } from "@/components/wind-turbines/wind-turbines-sidebar";

const validViews: WindTurbineView[] = ["energy-analytics", "performance"];

export default function WindTurbinesPage() {
  const pathname = usePathname();
  const segment = pathname.replace("/monitoring/wind-turbines", "").replace(/^\//, "");
  const activeView: WindTurbineView = validViews.includes(segment as WindTurbineView)
    ? (segment as WindTurbineView)
    : "device-list";

  const [selectedTurbine, setSelectedTurbine] = useState<WindTurbine | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  useEffect(() => {
    setSelectedTurbine(null);
    setSelectedAlarm(null);
  }, [activeView]);

  const handleAlarmOpen = (alarmId: string) => {
    const alarm = getAlarmById(alarmId);
    if (alarm) setSelectedAlarm(alarm);
  };

  const renderContent = () => {
    switch (activeView) {
      case "energy-analytics":
        return <EnergyAnalyticsView />;
      case "performance":
        return <PerformanceView />;
      default:
        return selectedTurbine ? (
          <TurbineDetail
            turbine={selectedTurbine}
            onBack={() => setSelectedTurbine(null)}
            onAlarmOpen={handleAlarmOpen}
          />
        ) : (
          <>
            <div className="shrink-0 bg-background px-6 py-5 border-b">
              <h1 className="text-xl font-semibold">Wind Turbines</h1>
            </div>
            <TurbineTable
              turbines={windTurbines}
              onTurbineSelect={setSelectedTurbine}
            />
          </>
        );
    }
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />
        <WindTurbinesSidebar />
        <main className="flex-1 flex flex-col overflow-hidden">
          {renderContent()}
        </main>
        {selectedAlarm && (
          <AlarmDetailPanel
            alarm={selectedAlarm}
            onClose={() => setSelectedAlarm(null)}
          />
        )}
      </div>
    </div>
  );
}
