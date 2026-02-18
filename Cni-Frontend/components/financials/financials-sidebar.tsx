"use client";

import { cn } from "@/lib/utils";

export type FinancialView =
  | "overview"
  | "revenue"
  | "cost"
  | "tariff"
  | "asset-performance"
  | "transactions"
  | "projections";

interface FinancialsSidebarProps {
  activeItem?: FinancialView;
  onItemChange?: (item: FinancialView) => void;
}

const menuItems: { id: FinancialView; label: string }[] = [
  { id: "overview", label: "Overview" },
  { id: "revenue", label: "Revenue Streams" },
  { id: "cost", label: "Cost Analysis" },
  { id: "tariff", label: "Tariff & Pricing" },
  { id: "asset-performance", label: "Asset Performance" },
  { id: "transactions", label: "Transactions" },
  { id: "projections", label: "Projections" },
];

export function FinancialsSidebar({ activeItem = "overview", onItemChange }: FinancialsSidebarProps) {
  return (
    <aside className="w-[180px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-4 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
          Financials
        </p>
        <nav className="space-y-0.5">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            return (
              <button
                key={item.id}
                onClick={() => onItemChange?.(item.id)}
                className={cn(
                  "w-full text-left px-3 py-2 rounded-md text-sm transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary font-medium"
                    : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
                )}
              >
                {item.label}
              </button>
            );
          })}
        </nav>
      </div>
    </aside>
  );
}
