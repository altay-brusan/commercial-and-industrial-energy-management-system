"use client";

import { useState } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { BessHvacMonitoringTab } from "./bess-hvac-monitoring-tab";
import { BessAlarmsTab } from "./bess-alarms-tab";
import { BessHvacSettingsTab } from "./settings/bess-hvac-settings-tab";
import { HVAC_TABS } from "@/lib/bess/types";
import type { BessHvac, BessComponentStatus } from "@/lib/bess/types";

interface BessHvacDetailProps {
  hvac: BessHvac;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  const styles: Record<BessComponentStatus, string> = {
    normal: "bg-status-operational/15 text-status-operational border-status-operational/30",
    warning: "bg-status-warning/15 text-status-warning border-status-warning/30",
    fault: "bg-status-critical/15 text-status-critical border-status-critical/30",
    offline: "bg-muted text-muted-foreground",
  };
  return <Badge className={styles[status]}>{status}</Badge>;
}

export function BessHvacDetail({ hvac, onBack, onAlarmOpen }: BessHvacDetailProps) {
  const [activeTab, setActiveTab] = useState("monitoring");

  // Extract HVAC number from id (e.g., "bess-01-HVAC-2" → "2")
  const hvacNumber = hvac.id.split("-").pop() ?? "1";

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">HVAC-{hvacNumber}</h2>
          <ComponentStatusBadge status={hvac.status} />
          <span className="text-sm text-muted-foreground">{hvac.deviceInfo.model}</span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {HVAC_TABS.map((tab) => (
            <button
              key={tab.id}
              onClick={() => !tab.disabled && setActiveTab(tab.id)}
              disabled={tab.disabled}
              className={`px-4 pb-2 text-sm transition-colors border-b-2 ${
                activeTab === tab.id
                  ? "border-foreground text-foreground font-medium"
                  : tab.disabled
                    ? "border-transparent text-muted-foreground/50 cursor-not-allowed"
                    : "border-transparent text-muted-foreground hover:text-foreground cursor-pointer"
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      {activeTab === "monitoring" && <BessHvacMonitoringTab hvac={hvac} />}
      {activeTab === "alarms" && (
        <BessAlarmsTab
          unitId={hvac.unitId}
          onAlarmOpen={onAlarmOpen}
          sourceFilter="HVAC"
        />
      )}
      {activeTab === "settings" && <BessHvacSettingsTab hvacId={hvac.id} />}
    </div>
  );
}
