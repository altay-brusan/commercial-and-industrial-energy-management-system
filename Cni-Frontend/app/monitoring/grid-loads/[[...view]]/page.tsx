"use client";

import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { GridLoadsSidebar } from "@/components/grid-loads/grid-loads-sidebar";
import { DeviceTable } from "@/components/grid-loads/device-table";
import { DeviceDetail } from "@/components/grid-loads/device-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { EnergyAnalyticsView } from "@/components/grid-loads/energy-analytics-view";
import { PerformanceView } from "@/components/grid-loads/performance-view";
import { gridDevices } from "@/lib/grid-loads/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { GridDevice } from "@/lib/grid-loads/types";
import type { Alarm } from "@/lib/alarms/types";
import type { GridLoadsView } from "@/components/grid-loads/grid-loads-sidebar";

const validViews: GridLoadsView[] = ["energy-analytics", "performance"];

export default function GridLoadsPage() {
  const pathname = usePathname();
  const segment = pathname.replace("/monitoring/grid-loads", "").replace(/^\//, "");
  const activeView: GridLoadsView = validViews.includes(segment as GridLoadsView)
    ? (segment as GridLoadsView)
    : "device-list";

  const [selectedDevice, setSelectedDevice] = useState<GridDevice | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  useEffect(() => {
    setSelectedDevice(null);
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
        return selectedDevice ? (
          <DeviceDetail
            device={selectedDevice}
            onBack={() => setSelectedDevice(null)}
            onAlarmOpen={handleAlarmOpen}
          />
        ) : (
          <>
            <div className="shrink-0 bg-background px-6 py-5 border-b">
              <h1 className="text-xl font-semibold">Grid & Loads</h1>
            </div>
            <DeviceTable
              devices={gridDevices}
              onDeviceSelect={setSelectedDevice}
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
        <GridLoadsSidebar />
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
