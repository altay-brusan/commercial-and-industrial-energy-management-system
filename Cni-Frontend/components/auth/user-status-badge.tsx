"use client";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface UserStatusBadgeProps {
  isActive: boolean;
}

export function UserStatusBadge({ isActive }: UserStatusBadgeProps) {
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
