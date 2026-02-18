"use client";

import type { LucideIcon } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface DashboardStatsCardProps {
  title: string;
  value: string;
  subtitle: string;
  icon: LucideIcon;
  colorClass: string;
  onClick?: () => void;
}

export function DashboardStatsCard({
  title,
  value,
  subtitle,
  icon: Icon,
  colorClass,
  onClick,
}: DashboardStatsCardProps) {
  return (
    <Card
      className={cn(
        "transition-all",
        onClick && "cursor-pointer hover:ring-1 hover:ring-border hover:bg-muted/50"
      )}
      onClick={onClick}
    >
      <CardContent className="p-5">
        <div className="flex items-start justify-between">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">{title}</p>
            <p className="text-2xl font-semibold tracking-tight">{value}</p>
            <p className="text-xs text-muted-foreground">{subtitle}</p>
          </div>
          <div className={cn("rounded-lg p-2.5", colorClass)}>
            <Icon className="h-5 w-5" />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
