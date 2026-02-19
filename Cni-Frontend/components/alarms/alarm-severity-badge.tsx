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
      return "bg-energy-grid text-white hover:bg-energy-grid/90";
    case "high":
      return "bg-energy-consumption text-white hover:bg-energy-consumption/90";
    case "medium":
      return "bg-energy-battery text-white hover:bg-energy-battery/90";
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
