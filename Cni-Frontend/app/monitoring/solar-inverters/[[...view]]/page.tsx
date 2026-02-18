"use client";

import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { SolarInvertersSidebar } from "@/components/solar-inverters/solar-inverters-sidebar";
import { InverterTable } from "@/components/solar-inverters/inverter-table";
import { InverterDetail } from "@/components/solar-inverters/inverter-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { EnergyAnalyticsView } from "@/components/solar-inverters/energy-analytics-view";
import { PerformanceView } from "@/components/solar-inverters/performance-view";
import { solarInverters } from "@/lib/solar-inverters/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { SolarInverter } from "@/lib/solar-inverters/types";
import type { Alarm } from "@/lib/alarms/types";
import type { SolarInverterView } from "@/components/solar-inverters/solar-inverters-sidebar";

const validViews: SolarInverterView[] = ["energy-analytics", "performance"];

export default function SolarInvertersPage() {
  const pathname = usePathname();
  const segment = pathname.replace("/monitoring/solar-inverters", "").replace(/^\//, "");
  const activeView: SolarInverterView = validViews.includes(segment as SolarInverterView)
    ? (segment as SolarInverterView)
    : "device-list";

  const [selectedInverter, setSelectedInverter] = useState<SolarInverter | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  useEffect(() => {
    setSelectedInverter(null);
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
        return selectedInverter ? (
          <InverterDetail
            inverter={selectedInverter}
            onBack={() => setSelectedInverter(null)}
            onAlarmOpen={handleAlarmOpen}
          />
        ) : (
          <>
            <div className="shrink-0 bg-background px-6 py-5 border-b">
              <h1 className="text-xl font-semibold">Inverter</h1>
            </div>
            <InverterTable
              inverters={solarInverters}
              onInverterSelect={setSelectedInverter}
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
        <SolarInvertersSidebar />
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
