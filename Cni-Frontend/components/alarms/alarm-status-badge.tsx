import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { AlarmStatus } from "@/lib/alarms/types";

interface AlarmStatusBadgeProps {
  status: AlarmStatus;
  className?: string;
}

export function getAlarmStatusColor(status: AlarmStatus): string {
  switch (status) {
    case "active":
      return "border-energy-grid text-energy-grid bg-energy-grid/10";
    case "acknowledged":
      return "border-energy-consumption text-energy-consumption bg-energy-consumption/10";
    case "resolved":
      return "border-energy-solar text-energy-solar bg-energy-solar/10";
  }
}

export function getAlarmStatusLabel(status: AlarmStatus): string {
  switch (status) {
    case "active":
      return "Active";
    case "acknowledged":
      return "Acknowledged";
    case "resolved":
      return "Resolved";
  }
}

export function AlarmStatusBadge({ status, className }: AlarmStatusBadgeProps) {
  return (
    <Badge
      variant="outline"
      className={cn(getAlarmStatusColor(status), className)}
    >
      {getAlarmStatusLabel(status)}
    </Badge>
  );
}
