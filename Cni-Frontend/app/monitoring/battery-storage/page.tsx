"use client";

import { useState, useEffect, useCallback } from "react";
import { Loader2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { BessDeviceTree } from "@/components/bess/bess-device-tree";
import { BessOverviewTable } from "@/components/bess/bess-overview-table";
import { BessUnitDetail } from "@/components/bess/bess-unit-detail";
import * as bessService from "@/lib/bess/bess-service";
import type { BessTreeNodeDto, BessUnitsResultDto } from "@/lib/bess/api-types";

interface SelectedDevice {
  id: string;
  type: string;
  unitId: string;
}

export default function BatteryStoragePage() {
  const [selectedDevice, setSelectedDevice] = useState<SelectedDevice | null>(null);
  const [tree, setTree] = useState<BessTreeNodeDto[]>([]);
  const [unitsResult, setUnitsResult] = useState<BessUnitsResultDto | null>(null);
  const [loading, setLoading] = useState(true);

  const fetchData = useCallback(async () => {
    try {
      const [treeRes, unitsRes] = await Promise.all([
        bessService.getTree(),
        bessService.getUnits(),
      ]);
      setTree(treeRes.tree);
      setUnitsResult(unitsRes);
    } catch {
      // Auth errors are handled by apiClient (redirect to login)
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleTreeSelect = (device: SelectedDevice) => {
    setSelectedDevice(device);
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />

        <BessDeviceTree
          tree={tree}
          selectedDevice={selectedDevice}
          onSelectDevice={handleTreeSelect}
        />

        <main className="flex-1 flex flex-col overflow-hidden">
          {loading ? (
            <div className="flex-1 flex items-center justify-center">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : selectedDevice ? (
            <BessUnitDetail
              deviceId={selectedDevice.id}
              onBack={() => setSelectedDevice(null)}
            />
          ) : (
            <>
              <div className="shrink-0 bg-background px-6 py-5 border-b">
                <h1 className="text-xl font-semibold">
                  Battery Energy Storage
                </h1>
              </div>

              {unitsResult && (
                <BessOverviewTable
                  units={unitsResult.units}
                  aggregates={unitsResult.aggregates}
                  onUnitSelect={(unit) =>
                    setSelectedDevice({
                      id: unit.id,
                      type: "unit",
                      unitId: unit.id,
                    })
                  }
                />
              )}
            </>
          )}
        </main>
      </div>
    </div>
  );
}
