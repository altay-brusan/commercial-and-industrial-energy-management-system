"use client";

import { Badge } from "@/components/ui/badge";
import { dailyEnergyCost } from "@/lib/dashboard-2/mock-data";

export function DailyEnergyCost() {
  const data = dailyEnergyCost;

  const formatTL = (value: number) =>
    value.toLocaleString("tr-TR", { minimumFractionDigits: 2 });

  return (
    <div className="rounded-lg bg-muted/30 p-6 space-y-6">
      <div>
        <h2 className="text-lg font-semibold">Daily Energy Cost</h2>
        <p className="text-sm text-muted-foreground">
          Cost &amp; savings summary
        </p>
      </div>

      <div>
        <p className="text-sm text-muted-foreground mb-1">Total Cost</p>
        <div className="flex items-baseline gap-2">
          <span className="text-3xl font-semibold tracking-tight">
            {formatTL(data.totalCostTL)}
          </span>
          <span className="text-sm text-muted-foreground">TL</span>
        </div>
      </div>

      <div>
        <p className="text-sm text-muted-foreground mb-1">You Saved</p>
        <div className="flex items-baseline gap-2">
          <span className="text-3xl font-semibold tracking-tight text-energy-solar">
            {formatTL(data.savedTL)}
          </span>
          <span className="text-sm text-muted-foreground">TL</span>
        </div>
        {data.savedTL === 0 && (
          <Badge variant="secondary" className="mt-2 text-xs font-normal text-muted-foreground">
            No savings today
          </Badge>
        )}
      </div>
    </div>
  );
}
