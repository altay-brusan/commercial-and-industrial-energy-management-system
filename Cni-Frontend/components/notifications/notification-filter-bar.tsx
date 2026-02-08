"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import type { NotificationFilters, NotificationType } from "@/lib/alarms/types";

interface NotificationFilterBarProps {
  filters: NotificationFilters;
  onFiltersChange: (filters: NotificationFilters) => void;
}

const typeOptions: { value: NotificationType | "all"; label: string }[] = [
  { value: "all", label: "All Types" },
  { value: "system_alert", label: "System Alerts" },
  { value: "maintenance_completed", label: "Maintenance" },
  { value: "alarm_resolved", label: "Alarm Resolved" },
  { value: "schedule_reminder", label: "Reminders" },
  { value: "system_update", label: "Updates" },
];

export function NotificationFilterBar({
  filters,
  onFiltersChange,
}: NotificationFilterBarProps) {
  return (
    <div className="flex items-center gap-3">
      <Select
        value={filters.type || "all"}
        onValueChange={(v) =>
          onFiltersChange({
            ...filters,
            type: v === "all" ? undefined : (v as NotificationType),
          })
        }
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="All Types" />
        </SelectTrigger>
        <SelectContent>
          {typeOptions.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <ToggleGroup
        type="single"
        value={filters.readStatus || "all"}
        onValueChange={(v) =>
          onFiltersChange({
            ...filters,
            readStatus: (v || "all") as NotificationFilters["readStatus"],
          })
        }
      >
        <ToggleGroupItem value="all" className="text-xs px-3">
          All
        </ToggleGroupItem>
        <ToggleGroupItem value="unread" className="text-xs px-3">
          Unread
        </ToggleGroupItem>
        <ToggleGroupItem value="read" className="text-xs px-3">
          Read
        </ToggleGroupItem>
      </ToggleGroup>
    </div>
  );
}
