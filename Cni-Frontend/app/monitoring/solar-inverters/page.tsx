"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { SolarInvertersSidebar } from "@/components/solar-inverters/solar-inverters-sidebar";
import { InverterTable } from "@/components/solar-inverters/inverter-table";
import { InverterDetail } from "@/components/solar-inverters/inverter-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { solarInverters } from "@/lib/solar-inverters/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { SolarInverter } from "@/lib/solar-inverters/types";
import type { Alarm } from "@/lib/alarms/types";

export default function SolarInvertersPage() {
  const [selectedInverter, setSelectedInverter] = useState<SolarInverter | null>(null);
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
        <SolarInvertersSidebar />

        {/* Main content */}
        <main className="flex-1 flex flex-col overflow-hidden">
          {selectedInverter ? (
            <InverterDetail
              inverter={selectedInverter}
              onBack={() => setSelectedInverter(null)}
              onAlarmOpen={handleAlarmOpen}
            />
          ) : (
            <>
              {/* Page header */}
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">Inverter</h1>
              </div>

              <InverterTable
                inverters={solarInverters}
                onInverterSelect={setSelectedInverter}
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
