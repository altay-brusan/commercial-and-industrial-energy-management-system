"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { PcsOverviewTable } from "@/components/pcs/pcs-overview-table";
import { PcsDetail } from "@/components/pcs/pcs-detail";
import { pcsUnits } from "@/lib/pcs/mock-data";
import type { PcsUnit } from "@/lib/pcs/types";

export default function PcsMonitoringPage() {
  const [selectedUnit, setSelectedUnit] = useState<PcsUnit | null>(null);

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />
        <main className="flex-1 flex flex-col overflow-hidden">
          {selectedUnit ? (
            <PcsDetail
              unit={selectedUnit}
              onBack={() => setSelectedUnit(null)}
            />
          ) : (
            <>
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">Power Conversion Systems (PCS)</h1>
              </div>
              <PcsOverviewTable
                units={pcsUnits}
                onUnitSelect={setSelectedUnit}
              />
            </>
          )}
        </main>
      </div>
    </div>
  );
}
