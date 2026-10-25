"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

export type ConsumptionView = "consumer-list" | "energy-analytics" | "demand-response";

const basePath = "/monitoring/consumption";

const menuItems: { id: ConsumptionView; label: string }[] = [
  { id: "consumer-list", label: "Consumer List" },
  { id: "energy-analytics", label: "Energy Analytics" },
  { id: "demand-response", label: "Demand Response" },
];

export function ConsumptionSidebar() {
  const pathname = usePathname();
  const segment = pathname.replace(basePath, "").replace(/^\//, "");
  const activeItem = menuItems.find((i) => i.id === segment)?.id || "consumer-list";

  return (
    <aside className="w-[180px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-4 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
          Consumption
        </p>
        <nav className="space-y-0.5">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            const href = item.id === "consumer-list" ? basePath : `${basePath}/${item.id}`;
            return (
              <Link
                key={item.id}
                href={href}
                className={cn(
                  "block w-full text-left px-3 py-2 rounded-md text-sm transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary font-medium"
                    : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
                )}
              >
                {item.label}
              </Link>
            );
          })}
        </nav>
      </div>
    </aside>
  );
}
