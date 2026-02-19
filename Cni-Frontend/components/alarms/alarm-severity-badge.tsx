import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { AlarmSeverity } from "@/lib/maintenance/types";

interface AlarmSeverityBadgeProps {
  severity: AlarmSeverity;
  className?: string;
}

export function getSeverityColor(severity: AlarmSeverity): string {
  switch (severity) {
    case "critical":
      return "bg-severity-critical text-white hover:bg-severity-critical/90";
    case "high":
      return "bg-severity-high text-white hover:bg-severity-high/90";
    case "medium":
      return "bg-severity-medium text-white hover:bg-severity-medium/90";
    case "low":
      return "bg-muted text-muted-foreground hover:bg-muted/90";
  }
}

export function getSeverityLabel(severity: AlarmSeverity): string {
  switch (severity) {
    case "critical":
      return "Critical";
    case "high":
      return "High";
    case "medium":
      return "Medium";
    case "low":
      return "Low";
  }
}

export function AlarmSeverityBadge({ severity, className }: AlarmSeverityBadgeProps) {
  return (
    <Badge className={cn(getSeverityColor(severity), className)}>
      {getSeverityLabel(severity)}
    </Badge>
  );
}
