"use client";

import Link from "next/link";
import { formatDistanceToNow } from "date-fns";
import { AlertTriangle, AlertCircle, Info, ArrowRight } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { alarms } from "@/lib/alarms/mock-data";
import { cn } from "@/lib/utils";
import type { DetailPanelState } from "@/lib/dashboard/types";
import type { AlarmSeverity } from "@/lib/maintenance/types";

interface DashboardAlertsWidgetProps {
  onPanelChange: (state: DetailPanelState) => void;
}

function getSeverityIcon(severity: AlarmSeverity) {
  switch (severity) {
    case "critical":
    case "high":
      return AlertTriangle;
    case "medium":
      return AlertCircle;
    case "low":
      return Info;
  }
}

function getSeverityColor(severity: AlarmSeverity) {
  switch (severity) {
    case "critical":
      return "bg-energy-grid/15 text-energy-grid border-energy-grid/30";
    case "high":
      return "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30";
    case "medium":
      return "bg-energy-battery/15 text-energy-battery border-energy-battery/30";
    case "low":
      return "bg-muted text-muted-foreground border-border";
  }
}

function getSeverityIconColor(severity: AlarmSeverity) {
  switch (severity) {
    case "critical":
      return "text-energy-grid";
    case "high":
      return "text-energy-consumption";
    case "medium":
      return "text-energy-battery";
    case "low":
      return "text-muted-foreground";
  }
}

export function DashboardAlertsWidget({ onPanelChange }: DashboardAlertsWidgetProps) {
  const recentAlarms = alarms
    .filter((a) => a.status !== "resolved")
    .sort((a, b) => b.triggeredAt.getTime() - a.triggeredAt.getTime())
    .slice(0, 5);

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Recent Alerts</CardTitle>
          <Link
            href="/alarms"
            className="flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            View All
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>
      </CardHeader>
      <CardContent className="p-4 pt-0 space-y-2">
        {recentAlarms.map((alarm) => {
          const Icon = getSeverityIcon(alarm.severity);
          return (
            <div
              key={alarm.id}
              className="flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-muted/50 transition-colors"
              onClick={() => onPanelChange({ type: "alert", alertId: alarm.id })}
            >
              <Icon className={cn("h-4 w-4 mt-0.5 shrink-0", getSeverityIconColor(alarm.severity))} />
              <div className="flex-1 min-w-0">
                <p className="text-sm truncate">{alarm.message}</p>
                <div className="flex items-center gap-2 mt-1.5">
                  <Badge variant="outline" className={cn("text-[10px]", getSeverityColor(alarm.severity))}>
                    {alarm.severity}
                  </Badge>
                  <span className="text-xs text-muted-foreground">
                    {formatDistanceToNow(alarm.triggeredAt, { addSuffix: true })}
                  </span>
                </div>
              </div>
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
}
