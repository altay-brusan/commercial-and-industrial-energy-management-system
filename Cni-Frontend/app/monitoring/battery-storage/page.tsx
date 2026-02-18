"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { BessSidebar } from "@/components/bess/bess-sidebar";
import { BessOverviewTable } from "@/components/bess/bess-overview-table";
import { BessUnitDetail } from "@/components/bess/bess-unit-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { bessUnits } from "@/lib/bess/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { BessUnit } from "@/lib/bess/types";
import type { Alarm } from "@/lib/alarms/types";

export default function BatteryStoragePage() {
  const [selectedUnit, setSelectedUnit] = useState<BessUnit | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  const handleAlarmOpen = (alarmId: string) => {
    const alarm = getAlarmById(alarmId);
    if (alarm) setSelectedAlarm(alarm);
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        {/* Primary sidebar - monitoring nav */}
        <MonitoringSidebar />

        {/* Secondary sidebar - device menu */}
        <BessSidebar />

        {/* Main content */}
        <main className="flex-1 flex flex-col overflow-hidden">
          {selectedUnit ? (
            <BessUnitDetail
              unit={selectedUnit}
              onBack={() => setSelectedUnit(null)}
              onAlarmOpen={handleAlarmOpen}
            />
          ) : (
            <>
              {/* Page header */}
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">Battery Energy Storage</h1>
              </div>

              <BessOverviewTable
                units={bessUnits}
                onUnitSelect={setSelectedUnit}
              />
            </>
          )}
        </main>

        {/* Alarm Detail Panel */}
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
