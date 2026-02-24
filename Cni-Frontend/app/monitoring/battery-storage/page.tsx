"use client";

import { useState, useMemo } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { BessDeviceTree } from "@/components/bess/bess-device-tree";
import { BessOverviewTable } from "@/components/bess/bess-overview-table";
import { BessDeviceDetail } from "@/components/bess/bess-device-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { bessUnits, buildDeviceTree } from "@/lib/bess/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { BessSelectedDevice } from "@/lib/bess/types";
import type { Alarm } from "@/lib/alarms/types";

export default function BatteryStoragePage() {
  const [selectedDevice, setSelectedDevice] =
    useState<BessSelectedDevice | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  const deviceTree = useMemo(() => buildDeviceTree(), []);

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

        {/* Secondary sidebar - device tree */}
        <BessDeviceTree
          tree={deviceTree}
          selectedDevice={selectedDevice}
          onSelectDevice={setSelectedDevice}
        />

        {/* Main content */}
        <main className="flex-1 flex flex-col overflow-hidden">
          {selectedDevice ? (
            <BessDeviceDetail
              selectedDevice={selectedDevice}
              onBack={() => setSelectedDevice(null)}
              onAlarmOpen={handleAlarmOpen}
            />
          ) : (
            <>
              {/* Page header */}
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">
                  Battery Energy Storage
                </h1>
              </div>

              <BessOverviewTable
                units={bessUnits}
                onUnitSelect={(unit) =>
                  setSelectedDevice({
                    id: unit.id,
                    type: "unit",
                    unitId: unit.id,
                  })
                }
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
