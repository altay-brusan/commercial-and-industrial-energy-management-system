"use client";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface RoleStatusBadgeProps {
  isActive: boolean;
}

export function RoleStatusBadge({ isActive }: RoleStatusBadgeProps) {
  return (
    <Badge
      variant="outline"
      className={cn(
        "text-xs font-medium",
        isActive
          ? "border-status-operational text-status-operational bg-status-operational/10"
          : "border-muted-foreground text-muted-foreground bg-muted"
      )}
    >
      {isActive ? "Active" : "Inactive"}
    </Badge>
  );
}
