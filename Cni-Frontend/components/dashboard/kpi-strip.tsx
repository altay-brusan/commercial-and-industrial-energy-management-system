"use client";

import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useState } from "react";

interface KPI {
  label: string;
  value: string;
  unit: string;
  target: string;
  trend: "up" | "down" | "neutral";
  status: "on-track" | "at-risk" | "behind";
}

const kpiData: Record<string, KPI[]> = {
  today: [
    {
      label: "Availability",
      value: "99.2",
      unit: "%",
      target: "99%",
      trend: "up",
      status: "on-track",
    },
    {
      label: "Efficiency",
      value: "94.5",
      unit: "%",
      target: "95%",
      trend: "down",
      status: "at-risk",
    },
    {
      label: "Self-Consumption",
      value: "78.3",
      unit: "%",
      target: "80%",
      trend: "up",
      status: "at-risk",
    },
    {
      label: "Carbon Saved",
      value: "1.2",
      unit: "t",
      target: "1.0t",
      trend: "up",
      status: "on-track",
    },
  ],
  week: [
    {
      label: "Availability",
      value: "98.8",
      unit: "%",
      target: "99%",
      trend: "neutral",
      status: "at-risk",
    },
    {
      label: "Efficiency",
      value: "93.2",
      unit: "%",
      target: "95%",
      trend: "down",
      status: "behind",
    },
    {
      label: "Self-Consumption",
      value: "75.6",
      unit: "%",
      target: "80%",
      trend: "up",
      status: "at-risk",
    },
    {
      label: "Carbon Saved",
      value: "8.4",
      unit: "t",
      target: "7.0t",
      trend: "up",
      status: "on-track",
    },
  ],
  month: [
    {
      label: "Availability",
      value: "99.1",
      unit: "%",
      target: "99%",
      trend: "up",
      status: "on-track",
    },
    {
      label: "Efficiency",
      value: "94.8",
      unit: "%",
      target: "95%",
      trend: "neutral",
      status: "at-risk",
    },
    {
      label: "Self-Consumption",
      value: "81.2",
      unit: "%",
      target: "80%",
      trend: "up",
      status: "on-track",
    },
    {
      label: "Carbon Saved",
      value: "32.5",
      unit: "t",
      target: "30t",
      trend: "up",
      status: "on-track",
    },
  ],
};

function getTrendArrow(trend: "up" | "down" | "neutral") {
  switch (trend) {
    case "up":
      return "^";
    case "down":
      return "v";
    default:
      return "-";
  }
}

function getStatusColor(status: "on-track" | "at-risk" | "behind") {
  switch (status) {
    case "on-track":
      return "text-energy-solar";
    case "at-risk":
      return "text-energy-consumption";
    case "behind":
      return "text-energy-grid";
  }
}

export function KPIStrip() {
  const [period, setPeriod] = useState<"today" | "week" | "month">("today");
  const kpis = kpiData[period];

  return (
    <Card>
      <CardContent className="pt-4">
        <div className="flex items-center justify-between mb-4">
          <span className="text-sm font-medium text-muted-foreground">
            Key Performance Indicators
          </span>
          <Tabs
            value={period}
            onValueChange={(v) => setPeriod(v as "today" | "week" | "month")}
          >
            <TabsList className="h-7">
              <TabsTrigger value="today" className="text-xs px-2 h-6">
                Today
              </TabsTrigger>
              <TabsTrigger value="week" className="text-xs px-2 h-6">
                Week
              </TabsTrigger>
              <TabsTrigger value="month" className="text-xs px-2 h-6">
                Month
              </TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {kpis.map((kpi) => (
            <div key={kpi.label} className="space-y-1">
              <p className="text-xs text-muted-foreground">{kpi.label}</p>
              <div className="flex items-baseline gap-1">
                <span className="text-2xl font-semibold">{kpi.value}</span>
                <span className="text-sm text-muted-foreground">{kpi.unit}</span>
                <span
                  className={`text-xs ml-1 ${
                    kpi.trend === "up"
                      ? "text-energy-solar"
                      : kpi.trend === "down"
                      ? "text-energy-grid"
                      : "text-muted-foreground"
                  }`}
                >
                  {getTrendArrow(kpi.trend)}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">
                  Target: {kpi.target}
                </span>
                <Badge
                  variant="secondary"
                  className={`text-[10px] px-1.5 py-0 ${getStatusColor(
                    kpi.status
                  )}`}
                >
                  {kpi.status === "on-track"
                    ? "On Track"
                    : kpi.status === "at-risk"
                    ? "At Risk"
                    : "Behind"}
                </Badge>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
