"use client";

import { List } from "lucide-react";

interface SolarInvertersSidebarProps {
  activeItem?: string;
}

const menuItems = [
  { id: "device-list", label: "Device List", icon: List },
];

export function SolarInvertersSidebar({ activeItem = "device-list" }: SolarInvertersSidebarProps) {
  return (
    <aside className="w-[200px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="p-4">
        <h3 className="text-sm font-semibold text-foreground mb-3">Device</h3>
        <nav className="space-y-1">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            const Icon = item.icon;
            return (
              <button
                key={item.id}
                className={`w-full flex items-center gap-2 px-3 py-2 rounded-md text-sm transition-colors ${
                  isActive
                    ? "bg-secondary text-foreground font-medium"
                    : "text-muted-foreground hover:bg-secondary/50 hover:text-foreground"
                }`}
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </button>
            );
          })}
        </nav>
      </div>
    </aside>
  );
}
