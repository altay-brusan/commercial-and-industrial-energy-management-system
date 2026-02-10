"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Eye, Sun, Wind, Battery, Network, Zap, type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface MenuItem {
  label: string;
  href: string;
  icon: LucideIcon;
  exact?: boolean;
}

const menuItems: MenuItem[] = [
  { label: "Overview", href: "/monitoring", icon: Eye, exact: true },
  { label: "Solar", href: "/monitoring/solar-inverters", icon: Sun },
  { label: "Wind", href: "/monitoring/wind-turbines", icon: Wind },
  { label: "Battery", href: "/monitoring/battery-storage", icon: Battery },
  { label: "Grid", href: "/monitoring/grid-loads", icon: Network },
  { label: "Consume", href: "/monitoring/consumption", icon: Zap },
];

export function MonitoringSidebar() {
  const pathname = usePathname();

  return (
    <aside className="w-[72px] shrink-0 border-r border-border bg-muted/30 flex flex-col items-center px-2 py-3 gap-1 overflow-y-auto">
      {menuItems.map((item) => {
        const isActive = item.exact
          ? pathname === item.href
          : pathname === item.href || pathname.startsWith(item.href + "/");
        const Icon = item.icon;
        return (
          <Link
            key={item.href}
            href={item.href}
            className={cn(
              "relative flex flex-col items-center justify-center w-[52px] h-[52px] rounded-lg text-center transition-colors group",
              isActive
                ? "bg-primary/10 text-primary font-medium"
                : "text-muted-foreground hover:bg-background/60 hover:text-foreground"
            )}
          >
            <Icon className="h-[18px] w-[18px]" />
            <span className="text-[10px] leading-tight mt-1">{item.label}</span>
          </Link>
        );
      })}
    </aside>
  );
}
