"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";

const tabs = [
  { label: "Users", href: "/configuration/users" },
  { label: "Roles", href: "/configuration/roles" },
  { label: "Permissions", href: "/configuration/permissions" },
];

export function AccessTabs() {
  const pathname = usePathname();

  return (
    <div className="flex items-center gap-1 border-b border-border -mx-6 px-6">
      {tabs.map((tab) => {
        const isActive =
          pathname === tab.href || pathname.startsWith(tab.href + "/");

        return (
          <Link
            key={tab.href}
            href={tab.href}
            className={cn(
              "px-3 py-2 text-sm font-medium transition-colors relative",
              isActive
                ? "text-primary"
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            {tab.label}
            {isActive && (
              <span className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary" />
            )}
          </Link>
        );
      })}
    </div>
  );
}
