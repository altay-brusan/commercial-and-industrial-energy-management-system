import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import type { TaskStatus, TaskPriority } from "@/lib/maintenance/types";

interface TaskStatusBadgeProps {
  status: TaskStatus;
  className?: string;
}

// Status badges use OUTLINED style with colored borders
export function getStatusColor(status: TaskStatus): string {
  switch (status) {
    case "scheduled":
      return "border-muted-foreground/50 text-muted-foreground bg-transparent";
    case "pending":
      return "border-energy-battery text-energy-battery bg-energy-battery/10";
    case "in_progress":
      return "border-energy-consumption text-energy-consumption bg-energy-consumption/10";
    case "completed":
      return "border-energy-solar text-energy-solar bg-energy-solar/10";
    case "closed":
      return "border-muted-foreground text-muted-foreground bg-muted";
    case "overdue":
      return "border-energy-grid text-energy-grid bg-energy-grid/10";
  }
}

export function getStatusLabel(status: TaskStatus): string {
  switch (status) {
    case "scheduled":
      return "Scheduled";
    case "pending":
      return "Pending";
    case "in_progress":
      return "In Progress";
    case "completed":
      return "Completed";
    case "closed":
      return "Closed";
    case "overdue":
      return "Overdue";
  }
}

export function TaskStatusBadge({ status, className }: TaskStatusBadgeProps) {
  return (
    <Badge
      variant="outline"
      className={cn(getStatusColor(status), className)}
    >
      {getStatusLabel(status)}
    </Badge>
  );
}

interface TaskPriorityBadgeProps {
  priority: TaskPriority;
  className?: string;
}

// Priority badges use FILLED style - solid colors
export function getPriorityColor(priority: TaskPriority): string {
  switch (priority) {
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

export function getPriorityLabel(priority: TaskPriority): string {
  switch (priority) {
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

export function TaskPriorityBadge({ priority, className }: TaskPriorityBadgeProps) {
  return (
    <Badge className={cn(getPriorityColor(priority), className)}>
      {getPriorityLabel(priority)}
    </Badge>
  );
}
