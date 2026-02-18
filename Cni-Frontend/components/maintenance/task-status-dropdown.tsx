"use client";

import { ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { getStatusColor, getStatusLabel } from "./task-status-badge";
import { cn } from "@/lib/utils";
import type { TaskStatus } from "@/lib/maintenance/types";

interface TaskStatusDropdownProps {
  currentStatus: TaskStatus;
  onStatusChange?: (newStatus: TaskStatus) => void;
}

const statusOptions: TaskStatus[] = [
  "scheduled",
  "pending",
  "in_progress",
  "completed",
  "overdue",
];

// Get dot color for dropdown items
function getStatusDotColor(status: TaskStatus): string {
  switch (status) {
    case "scheduled":
      return "bg-muted-foreground";
    case "pending":
      return "bg-energy-battery";
    case "in_progress":
      return "bg-energy-consumption";
    case "completed":
      return "bg-energy-solar";
    case "closed":
      return "bg-muted-foreground";
    case "overdue":
      return "bg-energy-grid";
  }
}

export function TaskStatusDropdown({
  currentStatus,
  onStatusChange,
}: TaskStatusDropdownProps) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="outline"
          className={cn("gap-2", getStatusColor(currentStatus))}
        >
          {getStatusLabel(currentStatus)}
          <ChevronDown className="h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        {statusOptions.map((status) => (
          <DropdownMenuItem
            key={status}
            onClick={() => onStatusChange?.(status)}
            className={cn(
              "gap-2 cursor-pointer",
              currentStatus === status && "bg-accent"
            )}
          >
            <span
              className={cn("h-2 w-2 rounded-full", getStatusDotColor(status))}
            />
            {getStatusLabel(status)}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
