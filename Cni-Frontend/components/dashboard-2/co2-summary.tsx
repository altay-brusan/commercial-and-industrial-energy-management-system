"use client";

import { useState } from "react";
import { CloudOff } from "lucide-react";
import {
  co2TodayKg,
  co2EmissionData,
  co2MonthlyData,
} from "@/lib/dashboard-2/mock-data";

type Period = "today" | "weekly" | "monthly";

const tabs: { key: Period; label: string }[] = [
  { key: "today", label: "Today" },
  { key: "weekly", label: "Weekly" },
  { key: "monthly", label: "Monthly" },
];

export function CO2Summary() {
  const [period, setPeriod] = useState<Period>("today");

  const weeklyTotal = co2EmissionData.reduce((s, d) => s + d.value, 0);
  const monthlyData = co2MonthlyData;

  return (
    <div className="rounded-lg bg-muted/30 p-5 flex flex-col h-full overflow-hidden">
      {/* Header */}
      <div className="flex items-center gap-3 mb-4">
        <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-muted">
          <CloudOff className="h-[18px] w-[18px] text-muted-foreground" />
        </div>
        <div>
          <h2 className="text-sm font-medium">CO2 Emission</h2>
          <p className="text-xs text-muted-foreground">Carbon footprint</p>
        </div>
      </div>

      {/* Period tabs */}
      <div className="flex rounded-lg bg-background/60 p-0.5 mb-4">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setPeriod(tab.key)}
            className={`flex-1 px-2 py-1.5 text-xs font-medium rounded-md transition-colors ${
              period === tab.key
                ? "bg-foreground text-background shadow-sm"
                : "text-muted-foreground hover:text-foreground"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Content */}
      <div className="flex-1 min-h-0 overflow-y-auto">
        {period === "today" && (
          <div className="h-full flex flex-col justify-center items-center text-center">
            <span className="text-4xl font-semibold tracking-tight">
              {co2TodayKg}
            </span>
            <span className="text-sm text-muted-foreground mt-1">kg CO2</span>
          </div>
        )}

        {period === "weekly" && (
          <div className="space-y-1.5">
            {co2EmissionData.map((d) => (
              <div
                key={d.day}
                className="flex items-center justify-between rounded-md bg-background/60 px-3 py-2"
              >
                <span className="text-xs text-muted-foreground">{d.day}</span>
                <span className="text-xs font-medium">{d.value} kg</span>
              </div>
            ))}
            <div className="flex items-center justify-between pt-2 border-t border-border/50 px-1">
              <span className="text-xs text-muted-foreground">Total</span>
              <span className="text-sm font-semibold">{weeklyTotal} kg</span>
            </div>
          </div>
        )}

        {period === "monthly" && (
          <div className="space-y-1.5">
            {monthlyData.map((d) => (
              <div
                key={d.month}
                className="flex items-center justify-between rounded-md bg-background/60 px-3 py-2"
              >
                <span className="text-xs text-muted-foreground">{d.month}</span>
                <span className="text-xs font-medium">{d.value.toLocaleString("en-US")} kg</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
