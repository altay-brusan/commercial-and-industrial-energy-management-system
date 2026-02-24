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
      return "border-severity-medium text-severity-medium bg-severity-medium/10";
    case "in_progress":
      return "border-severity-high text-severity-high bg-severity-high/10";
    case "completed":
      return "border-status-operational text-status-operational bg-status-operational/10";
    case "closed":
      return "border-muted-foreground text-muted-foreground bg-muted";
    case "overdue":
      return "border-severity-critical text-severity-critical bg-severity-critical/10";
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
      return "bg-severity-critical text-white hover:bg-severity-critical/90";
    case "high":
      return "bg-severity-high text-white hover:bg-severity-high/90";
    case "medium":
      return "bg-severity-medium text-white hover:bg-severity-medium/90";
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
