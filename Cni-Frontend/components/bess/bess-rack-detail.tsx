"use client";

import { useState } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { BessRackSummaryTab } from "./bess-rack-summary-tab";
import { BessAlarmsTab } from "./bess-alarms-tab";
import { RACK_TABS } from "@/lib/bess/types";
import type { BessRack, BessUnit, BessComponentStatus } from "@/lib/bess/types";

interface BessRackDetailProps {
  rack: BessRack;
  unit: BessUnit;
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

export function BessRackDetail({ rack, unit, onBack, onAlarmOpen }: BessRackDetailProps) {
  const [activeTab, setActiveTab] = useState("summary");

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">Rack {rack.rackNumber}</h2>
          <ComponentStatusBadge status={rack.status} />
          <span className="text-sm text-muted-foreground">{unit.name}</span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {RACK_TABS.map((tab) => (
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
      {activeTab === "summary" && <BessRackSummaryTab rack={rack} />}
      {activeTab === "alarms" && (
        <BessAlarmsTab
          unitId={unit.id}
          onAlarmOpen={onAlarmOpen}
          sourceFilter={`Rack ${rack.rackNumber}`}
        />
      )}
    </div>
  );
}
