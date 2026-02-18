"use client";

import { useState } from "react";
import { Filter, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import type { TaskFilters, TaskStatus, TaskPriority } from "@/lib/maintenance/types";
import { users } from "@/lib/maintenance/mock-data";

interface MaintenanceFilterPanelProps {
  filters: TaskFilters;
  onFiltersChange: (filters: TaskFilters) => void;
}

const statusOptions: { value: TaskStatus; label: string }[] = [
  { value: "scheduled", label: "Scheduled" },
  { value: "pending", label: "Pending" },
  { value: "in_progress", label: "In Progress" },
  { value: "completed", label: "Completed" },
  { value: "closed", label: "Closed" },
  { value: "overdue", label: "Overdue" },
];

const priorityOptions: { value: TaskPriority; label: string }[] = [
  { value: "critical", label: "Critical" },
  { value: "high", label: "High" },
  { value: "medium", label: "Medium" },
  { value: "low", label: "Low" },
];

export function MaintenanceFilterPanel({
  filters,
  onFiltersChange,
}: MaintenanceFilterPanelProps) {
  const [open, setOpen] = useState(false);

  const hasActiveFilters =
    (filters.status && filters.status.length > 0) ||
    (filters.priority && filters.priority.length > 0) ||
    filters.assignedTo;

  const activeFilterCount =
    (filters.status?.length || 0) +
    (filters.priority?.length || 0) +
    (filters.assignedTo ? 1 : 0);

  const handleStatusChange = (status: TaskStatus, checked: boolean) => {
    const currentStatuses = filters.status || [];
    const newStatuses = checked
      ? [...currentStatuses, status]
      : currentStatuses.filter((s) => s !== status);

    onFiltersChange({
      ...filters,
      status: newStatuses.length > 0 ? newStatuses : undefined,
    });
  };

  const handlePriorityChange = (priority: TaskPriority, checked: boolean) => {
    const currentPriorities = filters.priority || [];
    const newPriorities = checked
      ? [...currentPriorities, priority]
      : currentPriorities.filter((p) => p !== priority);

    onFiltersChange({
      ...filters,
      priority: newPriorities.length > 0 ? newPriorities : undefined,
    });
  };

  const handleAssignedToChange = (userId: string) => {
    onFiltersChange({
      ...filters,
      assignedTo: userId === "all" ? undefined : userId,
    });
  };

  const clearFilters = () => {
    onFiltersChange({});
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="outline" className="gap-2">
          <Filter className="h-4 w-4" />
          Filter
          {hasActiveFilters && (
            <span className="ml-1 rounded-full bg-primary px-1.5 py-0.5 text-[10px] text-primary-foreground">
              {activeFilterCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80" align="start">
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h4 className="font-medium">Filters</h4>
            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="sm"
                className="h-auto p-1 text-xs text-muted-foreground"
                onClick={clearFilters}
              >
                <X className="mr-1 h-3 w-3" />
                Clear all
              </Button>
            )}
          </div>

          <Separator />

          {/* Status Filters */}
          <div className="space-y-2">
            <Label className="text-sm font-medium">Status</Label>
            <div className="grid grid-cols-2 gap-2">
              {statusOptions.map((option) => (
                <div key={option.value} className="flex items-center gap-2">
                  <Checkbox
                    id={`status-${option.value}`}
                    checked={filters.status?.includes(option.value) || false}
                    onCheckedChange={(checked) =>
                      handleStatusChange(option.value, checked as boolean)
                    }
                  />
                  <Label
                    htmlFor={`status-${option.value}`}
                    className="text-sm font-normal cursor-pointer"
                  >
                    {option.label}
                  </Label>
                </div>
              ))}
            </div>
          </div>

          <Separator />

          {/* Priority Filters */}
          <div className="space-y-2">
            <Label className="text-sm font-medium">Priority</Label>
            <div className="grid grid-cols-2 gap-2">
              {priorityOptions.map((option) => (
                <div key={option.value} className="flex items-center gap-2">
                  <Checkbox
                    id={`priority-${option.value}`}
                    checked={filters.priority?.includes(option.value) || false}
                    onCheckedChange={(checked) =>
                      handlePriorityChange(option.value, checked as boolean)
                    }
                  />
                  <Label
                    htmlFor={`priority-${option.value}`}
                    className="text-sm font-normal cursor-pointer"
                  >
                    {option.label}
                  </Label>
                </div>
              ))}
            </div>
          </div>

          <Separator />

          {/* Assigned To Filter */}
          <div className="space-y-2">
            <Label className="text-sm font-medium">Assigned To</Label>
            <Select
              value={filters.assignedTo || "all"}
              onValueChange={handleAssignedToChange}
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="All users" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All users</SelectItem>
                {users.map((user) => (
                  <SelectItem key={user.id} value={user.id}>
                    {user.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
}
