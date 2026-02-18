"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

export type SolarInverterView = "device-list" | "energy-analytics" | "performance";

const basePath = "/monitoring/solar-inverters";

const menuItems: { id: SolarInverterView; label: string }[] = [
  { id: "device-list", label: "Device List" },
  { id: "energy-analytics", label: "Energy Analytics" },
  { id: "performance", label: "Performance" },
];

export function SolarInvertersSidebar() {
  const pathname = usePathname();
  const segment = pathname.replace(basePath, "").replace(/^\//, "");
  const activeItem = menuItems.find((i) => i.id === segment)?.id || "device-list";

  return (
    <aside className="w-[180px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-4 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
          Solar & Inverters
        </p>
        <nav className="space-y-0.5">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            const href = item.id === "device-list" ? basePath : `${basePath}/${item.id}`;
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
