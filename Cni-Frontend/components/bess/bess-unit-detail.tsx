"use client";

import { useState } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { BessSummaryTab } from "./bess-summary-tab";
import { BessHealthTab } from "./bess-health-tab";
import { BessHierarchyTab } from "./bess-hierarchy-tab";
import { BessMonitoringTab } from "./bess-monitoring-tab";
import { BessFaultTab } from "./bess-fault-tab";
import { BessAlarmsTab } from "./bess-alarms-tab";
import type { BessUnit } from "@/lib/bess/types";

interface BessUnitDetailProps {
  unit: BessUnit;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

type DetailTab = "summary" | "health" | "hierarchy" | "monitoring" | "fault" | "alarms" | "settings";

function UnitStatusBadge({ status }: { status: BessUnit["status"] }) {
  const styles: Record<BessUnit["status"], string> = {
    online: "bg-status-operational/15 text-status-operational border-status-operational/30",
    offline: "bg-muted text-muted-foreground",
    fault: "bg-status-critical/15 text-status-critical border-status-critical/30",
    standby: "bg-status-standby/15 text-status-standby border-status-standby/30",
  };
  const labels: Record<BessUnit["status"], string> = {
    online: "Online",
    offline: "Offline",
    fault: "Fault",
    standby: "Standby",
  };
  return <Badge className={styles[status]}>{labels[status]}</Badge>;
}

const tabs: { id: DetailTab; label: string; disabled?: boolean }[] = [
  { id: "summary", label: "Summary" },
  { id: "hierarchy", label: "Hierarchy" },
  { id: "monitoring", label: "Monitoring" },
  { id: "health", label: "Health" },
  { id: "fault", label: "Fault" },
  { id: "alarms", label: "Alarms" },
  { id: "settings", label: "Settings", disabled: true },
];

export function BessUnitDetail({ unit, onBack, onAlarmOpen }: BessUnitDetailProps) {
  const [activeTab, setActiveTab] = useState<DetailTab>("summary");

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">{unit.name}</h2>
          <UnitStatusBadge status={unit.status} />
          <span className="text-sm text-muted-foreground">
            {unit.nameplate.brand} {unit.nameplate.model}
          </span>
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {tabs.map((tab) => (
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
      {activeTab === "summary" && <BessSummaryTab unit={unit} />}
      {activeTab === "health" && <BessHealthTab unit={unit} />}
      {activeTab === "hierarchy" && <BessHierarchyTab unit={unit} />}
      {activeTab === "monitoring" && <BessMonitoringTab unitId={unit.id} />}
      {activeTab === "fault" && <BessFaultTab unitId={unit.id} />}
      {activeTab === "alarms" && <BessAlarmsTab unitId={unit.id} onAlarmOpen={onAlarmOpen} />}
    </div>
  );
}
