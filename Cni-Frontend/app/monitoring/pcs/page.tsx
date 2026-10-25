"use client";

import { useState, useEffect, useCallback } from "react";
import { Loader2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { PcsOverviewTable } from "@/components/pcs/pcs-overview-table";
import { PcsDetail } from "@/components/pcs/pcs-detail";
import * as pcsService from "@/lib/pcs/pcs-service";
import { useFacility } from "@/lib/facilities/facility-context";
import type { PcsUnitsResultDto } from "@/lib/pcs/api-types";

export default function PcsMonitoringPage() {
  const { selectedFacilityId } = useFacility();
  const [selectedUnitId, setSelectedUnitId] = useState<string | null>(null);
  const [unitsResult, setUnitsResult] = useState<PcsUnitsResultDto | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchData = useCallback(async () => {
    if (!selectedFacilityId) return;
    setLoading(true);
    try {
      const res = await pcsService.getUnits(selectedFacilityId);
      setUnitsResult(res);
    } catch {
      // Auth errors are handled by apiClient (redirect to login)
    } finally {
      setLoading(false);
    }
  }, [selectedFacilityId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />
        <main className="flex-1 flex flex-col overflow-hidden">
          {loading ? (
            <div className="flex-1 flex items-center justify-center">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : selectedUnitId ? (
            <PcsDetail
              unitId={selectedUnitId}
              onBack={() => setSelectedUnitId(null)}
            />
          ) : (
            <>
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">Power Conversion Systems (PCS)</h1>
              </div>
              {unitsResult && (
                <PcsOverviewTable
                  units={unitsResult.units}
                  aggregates={unitsResult.aggregates}
                  onUnitSelect={(unit) => setSelectedUnitId(unit.id)}
                />
              )}
            </>
          )}
        </main>
      </div>
    </div>
  );
}
