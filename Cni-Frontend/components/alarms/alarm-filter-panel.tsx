"use client";

import { Filter, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import type { AlarmSeverity } from "@/lib/maintenance/types";
import type { AlarmFilters, AlarmStatus, AlarmSourceType } from "@/lib/alarms/types";
import { getAlarmLocations } from "@/lib/alarms/mock-data";

interface AlarmFilterPanelProps {
  filters: AlarmFilters;
  onFiltersChange: (filters: AlarmFilters) => void;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const severityOptions: { value: AlarmSeverity; label: string }[] = [
  { value: "critical", label: "Critical" },
  { value: "high", label: "High" },
  { value: "medium", label: "Medium" },
  { value: "low", label: "Low" },
];

const statusOptions: { value: AlarmStatus; label: string }[] = [
  { value: "active", label: "Active" },
  { value: "acknowledged", label: "Acknowledged" },
  { value: "resolved", label: "Resolved" },
];

const sourceTypeOptions: { value: AlarmSourceType; label: string }[] = [
  { value: "solar", label: "Solar" },
  { value: "bess", label: "BESS" },
  { value: "grid", label: "Grid" },
  { value: "inverter", label: "Inverter" },
  { value: "system", label: "System" },
];

export function AlarmFilterToggle({
  filters,
  open,
  onOpenChange,
}: {
  filters: AlarmFilters;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const activeFilterCount =
    (filters.severity?.length || 0) +
    (filters.status?.length || 0) +
    (filters.sourceType?.length || 0) +
    (filters.alarmCode ? 1 : 0) +
    (filters.maintenanceTaskStatus ? 1 : 0) +
    (filters.location ? 1 : 0);

  const hasActiveFilters = activeFilterCount > 0;

  return (
    <Button
      variant={open ? "secondary" : "outline"}
      className="gap-2"
      onClick={() => onOpenChange(!open)}
    >
      <Filter className="h-4 w-4" />
      Filter
      {hasActiveFilters && (
        <span className="ml-1 rounded-full bg-primary px-1.5 py-0.5 text-[10px] text-primary-foreground">
          {activeFilterCount}
        </span>
      )}
    </Button>
  );
}

export function AlarmFilterPanel({
  filters,
  onFiltersChange,
  open,
  onOpenChange,
}: AlarmFilterPanelProps) {
  const locations = getAlarmLocations();

  const activeFilterCount =
    (filters.severity?.length || 0) +
    (filters.status?.length || 0) +
    (filters.sourceType?.length || 0) +
    (filters.alarmCode ? 1 : 0) +
    (filters.maintenanceTaskStatus ? 1 : 0) +
    (filters.location ? 1 : 0);

  const hasActiveFilters = activeFilterCount > 0;

  const handleSeverityChange = (severity: AlarmSeverity, checked: boolean) => {
    const current = filters.severity || [];
    const updated = checked
      ? [...current, severity]
      : current.filter((s) => s !== severity);
    onFiltersChange({ ...filters, severity: updated.length > 0 ? updated : undefined });
  };

  const handleStatusChange = (status: AlarmStatus, checked: boolean) => {
    const current = filters.status || [];
    const updated = checked
      ? [...current, status]
      : current.filter((s) => s !== status);
    onFiltersChange({ ...filters, status: updated.length > 0 ? updated : undefined });
  };

  const handleSourceTypeChange = (sourceType: AlarmSourceType, checked: boolean) => {
    const current = filters.sourceType || [];
    const updated = checked
      ? [...current, sourceType]
      : current.filter((s) => s !== sourceType);
    onFiltersChange({ ...filters, sourceType: updated.length > 0 ? updated : undefined });
  };

  const clearFilters = () => {
    onFiltersChange({});
  };

  if (!open) return null;

  return (
    <div className="border-b border-border bg-muted/30 px-6 py-4 animate-in slide-in-from-top-2 fade-in duration-200">
      {/* Header row */}
      <div className="flex items-center justify-between mb-4">
        <h4 className="text-sm font-medium">Filters</h4>
        <div className="flex items-center gap-2">
          {hasActiveFilters && (
            <Button
              variant="ghost"
              size="sm"
              className="h-auto px-2 py-1 text-xs text-muted-foreground"
              onClick={clearFilters}
            >
              <X className="mr-1 h-3 w-3" />
              Clear all ({activeFilterCount})
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7"
            onClick={() => onOpenChange(false)}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Filter grid - all visible at once */}
      <div className="grid grid-cols-3 lg:grid-cols-6 gap-x-6 gap-y-4">
        {/* Severity */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Severity</Label>
          <div className="space-y-1.5">
            {severityOptions.map((option) => (
              <div key={option.value} className="flex items-center gap-2">
                <Checkbox
                  id={`sev-${option.value}`}
                  checked={filters.severity?.includes(option.value) || false}
                  onCheckedChange={(checked) =>
                    handleSeverityChange(option.value, checked as boolean)
                  }
                />
                <Label
                  htmlFor={`sev-${option.value}`}
                  className="text-sm font-normal cursor-pointer"
                >
                  {option.label}
                </Label>
              </div>
            ))}
          </div>
        </div>

        {/* Status */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Status</Label>
          <div className="space-y-1.5">
            {statusOptions.map((option) => (
              <div key={option.value} className="flex items-center gap-2">
                <Checkbox
                  id={`stat-${option.value}`}
                  checked={filters.status?.includes(option.value) || false}
                  onCheckedChange={(checked) =>
                    handleStatusChange(option.value, checked as boolean)
                  }
                />
                <Label
                  htmlFor={`stat-${option.value}`}
                  className="text-sm font-normal cursor-pointer"
                >
                  {option.label}
                </Label>
              </div>
            ))}
          </div>
        </div>

        {/* Source Type */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Source Type</Label>
          <div className="space-y-1.5">
            {sourceTypeOptions.map((option) => (
              <div key={option.value} className="flex items-center gap-2">
                <Checkbox
                  id={`src-${option.value}`}
                  checked={filters.sourceType?.includes(option.value) || false}
                  onCheckedChange={(checked) =>
                    handleSourceTypeChange(option.value, checked as boolean)
                  }
                />
                <Label
                  htmlFor={`src-${option.value}`}
                  className="text-sm font-normal cursor-pointer"
                >
                  {option.label}
                </Label>
              </div>
            ))}
          </div>
        </div>

        {/* Alarm Code */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Alarm Code</Label>
          <Input
            placeholder="Search code..."
            value={filters.alarmCode || ""}
            onChange={(e) =>
              onFiltersChange({
                ...filters,
                alarmCode: e.target.value || undefined,
              })
            }
            className="h-9"
          />
        </div>

        {/* Maintenance Task Status */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Maintenance</Label>
          <Select
            value={filters.maintenanceTaskStatus || "any"}
            onValueChange={(v) =>
              onFiltersChange({
                ...filters,
                maintenanceTaskStatus: v === "any" ? undefined : v as AlarmFilters["maintenanceTaskStatus"],
              })
            }
          >
            <SelectTrigger className="w-full h-9">
              <SelectValue placeholder="Any" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="any">Any</SelectItem>
              <SelectItem value="has_task">Has Task</SelectItem>
              <SelectItem value="no_task">No Task</SelectItem>
              <SelectItem value="task_in_progress">Task In Progress</SelectItem>
              <SelectItem value="task_completed">Task Completed</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {/* Location */}
        <div className="space-y-2">
          <Label className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Location</Label>
          <Select
            value={filters.location || "all"}
            onValueChange={(v) =>
              onFiltersChange({
                ...filters,
                location: v === "all" ? undefined : v,
              })
            }
          >
            <SelectTrigger className="w-full h-9">
              <SelectValue placeholder="All locations" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All locations</SelectItem>
              {locations.map((loc) => (
                <SelectItem key={loc} value={loc}>
                  {loc}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>
    </div>
  );
}
