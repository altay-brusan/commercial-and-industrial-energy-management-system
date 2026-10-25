"use client";

import { useState, useMemo } from "react";
import { usePathname, useRouter } from "next/navigation";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { SolarInvertersSidebar } from "@/components/solar-inverters/solar-inverters-sidebar";
import { InverterTable } from "@/components/solar-inverters/inverter-table";
import { InverterDetail } from "@/components/solar-inverters/inverter-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { EnergyAnalyticsView } from "@/components/solar-inverters/energy-analytics-view";
import { PerformanceView } from "@/components/solar-inverters/performance-view";
import { solarInverters, buildSolarDeviceTree } from "@/lib/solar-inverters/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { Alarm } from "@/lib/alarms/types";
import type { SolarInverterView } from "@/components/solar-inverters/solar-inverters-sidebar";

type DetailTab = "general" | "fault" | "curve" | "history" | "settings";

const validViews: SolarInverterView[] = ["energy-analytics", "performance"];
const validTabs: DetailTab[] = ["general", "fault", "curve", "history", "settings"];
const basePath = "/monitoring/solar-inverters";

export default function SolarInvertersPage() {
  const pathname = usePathname();
  const router = useRouter();
  const segments = pathname
    .replace(basePath, "")
    .replace(/^\//, "")
    .split("/")
    .filter(Boolean);

  // Determine what to show from URL
  const activeView: SolarInverterView = validViews.includes(segments[0] as SolarInverterView)
    ? (segments[0] as SolarInverterView)
    : "device-list";

  const selectedInverter =
    activeView === "device-list" && segments[0]
      ? solarInverters.find((inv) => inv.id === segments[0]) ?? null
      : null;

  const activeTab: DetailTab =
    selectedInverter && validTabs.includes(segments[1] as DetailTab)
      ? (segments[1] as DetailTab)
      : "general";

  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);
  const deviceTree = useMemo(() => buildSolarDeviceTree(), []);

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
            activeTab={activeTab}
            onBack={() => router.push(basePath)}
            onAlarmOpen={handleAlarmOpen}
          />
        ) : (
          <>
            <div className="shrink-0 bg-background px-6 py-5 border-b">
              <h1 className="text-xl font-semibold">Inverter</h1>
            </div>
            <InverterTable
              inverters={solarInverters}
              onInverterSelect={(inv) => router.push(`${basePath}/${inv.id}`)}
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
        <SolarInvertersSidebar
          tree={deviceTree}
          selectedInverterId={selectedInverter?.id ?? null}
        />
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
