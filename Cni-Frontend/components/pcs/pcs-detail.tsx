"use client";

import { useState } from "react";
import { ChevronLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { PcsGeneralTab } from "./pcs-general-tab";
import { PcsFaultTab } from "./pcs-fault-tab";
import type { PcsUnit } from "@/lib/pcs/types";

interface PcsDetailProps {
  unit: PcsUnit;
  onBack: () => void;
}

type DetailTab = "general" | "fault" | "curve" | "settings";

function StatusBadge({ status }: { status: PcsUnit["status"] }) {
  const styles: Record<PcsUnit["status"], string> = {
    online: "bg-sky-500/15 text-sky-600 border-sky-500/30",
    offline: "bg-muted text-muted-foreground",
    fault: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    standby: "bg-yellow-500/15 text-yellow-600 border-yellow-500/30",
  };
  return <Badge className={styles[status]}>{status}</Badge>;
}

const tabs: { id: DetailTab; label: string; disabled?: boolean }[] = [
  { id: "general", label: "General" },
  { id: "fault", label: "Fault" },
  { id: "curve", label: "Curve", disabled: true },
  { id: "settings", label: "Settings", disabled: true },
];

export function PcsDetail({ unit, onBack }: PcsDetailProps) {
  const [activeTab, setActiveTab] = useState<DetailTab>("general");

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">{unit.name}</h2>
          <StatusBadge status={unit.status} />
          <span className="text-sm text-muted-foreground">{unit.deviceId}</span>
          <span className="text-xs text-muted-foreground ml-auto">Linked: {unit.linkedBessUnitName}</span>
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
      {activeTab === "general" && <PcsGeneralTab unit={unit} />}
      {activeTab === "fault" && <PcsFaultTab pcsId={unit.id} />}
    </div>
  );
}
