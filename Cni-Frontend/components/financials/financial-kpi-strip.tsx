"use client";

import { Button } from "@/components/ui/button";
import { financialKPI, formatCurrency, USD_TO_TRY } from "@/lib/financials/mock-data";
import type { Currency } from "@/lib/financials/types";

interface FinancialKPIStripProps {
  currency: Currency;
  onCurrencyToggle: () => void;
}

export function FinancialKPIStrip({ currency, onCurrencyToggle }: FinancialKPIStripProps) {
  const stats = [
    {
      label: "Total Revenue",
      value: formatCurrency(financialKPI.totalRevenue, currency),
      unit: "/mo",
      color: "text-energy-solar",
    },
    {
      label: "Total Cost",
      value: formatCurrency(financialKPI.totalCost, currency),
      unit: "/mo",
      color: "text-energy-grid",
    },
    {
      label: "Net Income",
      value: formatCurrency(financialKPI.netIncome, currency),
      unit: "/mo",
      color: financialKPI.netIncome >= 0 ? "text-energy-solar" : "text-energy-grid",
    },
    {
      label: "Gross Margin",
      value: `${financialKPI.grossMargin.toFixed(1)}%`,
      unit: "",
      color: "",
    },
    {
      label: "Portfolio LCOE",
      value: currency === "TRY"
        ? `₺${(financialKPI.portfolioLCOE * USD_TO_TRY).toFixed(2)}`
        : `$${financialKPI.portfolioLCOE.toFixed(3)}`,
      unit: "/kWh",
      color: "",
    },
    {
      label: "ROI",
      value: `${financialKPI.roi.toFixed(1)}%`,
      unit: "",
      color: "text-energy-solar",
    },
  ];

  return (
    <div className="shrink-0 border-b border-border">
      <div className="grid grid-cols-7">
        {stats.map((stat, i) => (
          <div
            key={stat.label}
            className={`px-5 py-4 ${i < stats.length ? "border-r border-border" : ""}`}
          >
            <p className="text-xs text-muted-foreground mb-1">{stat.label}</p>
            <div className="flex items-baseline gap-1">
              <span className={`text-2xl font-semibold tracking-tight ${stat.color}`}>
                {stat.value}
              </span>
              {stat.unit && (
                <span className="text-xs text-muted-foreground">{stat.unit}</span>
              )}
            </div>
          </div>
        ))}
        <div className="px-5 py-4 flex items-center justify-center">
          <Button
            variant="outline"
            size="sm"
            onClick={onCurrencyToggle}
            className="text-xs font-mono"
          >
            {currency === "USD" ? "$ USD" : "₺ TRY"}
          </Button>
        </div>
      </div>
    </div>
  );
}
