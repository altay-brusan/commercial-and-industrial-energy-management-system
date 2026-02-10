"use client";

import { cn } from "@/lib/utils";

interface BessSidebarProps {
  activeItem?: string;
}

const menuItems = [
  { id: "device-list", label: "Device List" },
];

export function BessSidebar({ activeItem = "device-list" }: BessSidebarProps) {
  return (
    <aside className="w-[180px] shrink-0 border-r border-border bg-background overflow-y-auto">
      <div className="px-4 py-4">
        <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
          Battery Storage
        </p>
        <nav className="space-y-0.5">
          {menuItems.map((item) => {
            const isActive = item.id === activeItem;
            return (
              <button
                key={item.id}
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
