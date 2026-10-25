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
      return "border-severity-critical text-severity-critical bg-severity-critical/10";
    case "acknowledged":
      return "border-severity-high text-severity-high bg-severity-high/10";
    case "resolved":
      return "border-status-operational text-status-operational bg-status-operational/10";
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
