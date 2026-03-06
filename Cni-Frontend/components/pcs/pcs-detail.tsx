"use client";

import { useState, useEffect } from "react";
import { ChevronLeft, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { PcsGeneralTab } from "./pcs-general-tab";
import { PcsFaultTab } from "./pcs-fault-tab";
import { PcsCurveTab } from "./pcs-curve-tab";
import * as pcsService from "@/lib/pcs/pcs-service";
import type { PcsHeaderDto } from "@/lib/pcs/api-types";
import { cn } from "@/lib/utils";

interface PcsDetailProps {
  unitId: string;
  onBack: () => void;
}

function badgeColorToClass(color: string): string {
  const map: Record<string, string> = {
    green: "bg-status-operational/15 text-status-operational border-status-operational/30",
    red: "bg-status-critical/15 text-status-critical border-status-critical/30",
    orange: "bg-severity-high/15 text-severity-high border-severity-high/30",
    yellow: "bg-status-standby/15 text-status-standby border-status-standby/30",
    grey: "bg-muted text-muted-foreground",
    gray: "bg-muted text-muted-foreground",
  };
  return map[color] ?? "bg-muted text-muted-foreground";
}

function TabContent({ tabKey, unitId }: { tabKey: string; unitId: string }) {
  switch (tabKey) {
    case "general":
      return <PcsGeneralTab unitId={unitId} />;
    case "fault":
      return <PcsFaultTab unitId={unitId} />;
    case "curve":
      return <PcsCurveTab unitId={unitId} />;
    default:
      return (
        <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
          This tab is not available yet.
        </div>
      );
  }
}

export function PcsDetail({ unitId, onBack }: PcsDetailProps) {
  const [header, setHeader] = useState<PcsHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<string>("general");

  useEffect(() => {
    setLoading(true);
    setActiveTab("general");
    pcsService.getUnitHeader(unitId).then((data) => {
      setHeader(data);
      const firstEnabled = data.availableTabs
        .filter((t) => t.isEnabled)
        .sort((a, b) => a.order - b.order)[0];
      if (firstEnabled) setActiveTab(firstEnabled.key);
    }).catch(() => {
      // handled by apiClient
    }).finally(() => setLoading(false));
  }, [unitId]);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!header) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground">
        PCS unit not found.
      </div>
    );
  }

  const tabs = header.availableTabs
    .filter((t) => t.isEnabled)
    .sort((a, b) => a.order - b.order);

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="shrink-0 border-b bg-background">
        <div className="flex items-center gap-3 px-6 py-4">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={onBack}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <h2 className="text-lg font-semibold">{header.name}</h2>
          <Badge className={badgeColorToClass(header.status.color)}>
            {header.status.label}
          </Badge>
          <span className="text-sm text-muted-foreground">{header.deviceId}</span>
          {header.linkedBessUnit && (
            <span className="text-xs text-muted-foreground ml-auto">
              Linked: {header.linkedBessUnit.name}
            </span>
          )}
        </div>

        {/* Tabs */}
        <div className="px-6 flex gap-0 border-b-0">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={cn(
                "px-4 pb-2 text-sm transition-colors border-b-2",
                activeTab === tab.key
                  ? "border-foreground text-foreground font-medium"
                  : "border-transparent text-muted-foreground hover:text-foreground cursor-pointer"
              )}
            >
              {tab.label}
            </button>
          ))}
        </div>
      </div>

      {/* Tab content */}
      <TabContent tabKey={activeTab} unitId={unitId} />
    </div>
  );
}
