"use client";

import { useState } from "react";
import { CalendarDays, Repeat, Clock, ChevronRight } from "lucide-react";
import { Calendar } from "@/components/ui/calendar";
import { cn } from "@/lib/utils";
import {
  scheduledMaintenances,
  getScheduledMaintenancesForDate,
} from "@/lib/maintenance/mock-data";
import type { ScheduledMaintenance } from "@/lib/maintenance/types";

function formatSelectedDate(date: Date): string {
  const today = new Date();
  const tomorrow = new Date(today);
  tomorrow.setDate(tomorrow.getDate() + 1);

  if (date.toDateString() === today.toDateString()) {
    return "Today";
  }
  if (date.toDateString() === tomorrow.toDateString()) {
    return "Tomorrow";
  }
  return date.toLocaleDateString("en-US", {
    weekday: "short",
    month: "short",
    day: "numeric",
  });
}

function getRecurrenceIcon(recurrence?: string): string {
  switch (recurrence) {
    case "daily":
      return "D";
    case "weekly":
      return "W";
    case "monthly":
      return "M";
    case "quarterly":
      return "Q";
    case "yearly":
      return "Y";
    default:
      return "";
  }
}

function getDaysUntil(date: Date): string {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(date);
  target.setHours(0, 0, 0, 0);
  const diff = Math.ceil((target.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

  if (diff === 0) return "Today";
  if (diff === 1) return "Tomorrow";
  if (diff < 7) return `${diff} days`;
  if (diff < 14) return "1 week";
  return `${Math.floor(diff / 7)} weeks`;
}

export function MaintenanceSidebar() {
  const [selectedDate, setSelectedDate] = useState<Date | undefined>(new Date());

  // Get dates that have scheduled maintenances
  const maintenanceDates = scheduledMaintenances.map((m) => m.scheduledDate);

  // Get maintenances for selected date
  const selectedDateMaintenances = selectedDate
    ? getScheduledMaintenancesForDate(selectedDate)
    : [];

  // Upcoming maintenances (next 8)
  const upcomingMaintenances = scheduledMaintenances
    .filter((m) => m.scheduledDate >= new Date())
    .sort((a, b) => a.scheduledDate.getTime() - b.scheduledDate.getTime())
    .slice(0, 8);

  return (
    <div className="w-[380px] shrink-0 border-l border-border bg-muted/30 overflow-y-auto">
      {/* Calendar Section */}
      <div className="p-4">
        <Calendar
          mode="single"
          selected={selectedDate}
          onSelect={setSelectedDate}
          className="w-full [&_.rdp-root]:w-full [&_.rdp-months]:w-full [&_.rdp-month]:w-full [&_.rdp-nav]:!right-0 [&_.rdp-nav]:!left-auto [&_.rdp-nav]:!justify-end [&_.rdp-nav]:!w-auto [&_.rdp-weekdays]:w-full [&_.rdp-weekday]:flex-1 [&_.rdp-week]:w-full [&_.rdp-day]:flex-1"
          modifiers={{
            hasMaintenance: maintenanceDates,
          }}
          modifiersClassNames={{
            hasMaintenance: "relative after:absolute after:bottom-1 after:left-1/2 after:-translate-x-1/2 after:h-1 after:w-1 after:rounded-full after:bg-energy-solar",
          }}
        />
      </div>

      {/* Selected Date Section */}
      <div className="px-4 pb-4">
        <div className="flex items-center gap-2 mb-3">
          <CalendarDays className="h-4 w-4 text-muted-foreground" />
          <span className="text-sm font-medium">
            {selectedDate ? formatSelectedDate(selectedDate) : "Select a date"}
          </span>
          {selectedDateMaintenances.length > 0 && (
            <span className="ml-auto text-xs text-muted-foreground">
              {selectedDateMaintenances.length} scheduled
            </span>
          )}
        </div>

        {selectedDateMaintenances.length > 0 ? (
          <div className="space-y-1">
            {selectedDateMaintenances.map((maintenance) => (
              <SelectedDateItem key={maintenance.id} maintenance={maintenance} />
            ))}
          </div>
        ) : (
          <div className="py-6 text-center">
            <div className="text-muted-foreground/50 text-xs">No scheduled work</div>
          </div>
        )}
      </div>

      {/* Upcoming Section */}
      <div className="border-t border-border">
        <div className="px-4 py-3 flex items-center gap-2">
          <Clock className="h-4 w-4 text-muted-foreground" />
          <span className="text-sm font-medium">Upcoming</span>
        </div>

        <div className="relative">
          {/* Timeline line */}
          <div className="absolute left-[23px] top-0 bottom-4 w-px bg-border" />

          <div className="px-4 pb-4 space-y-0">
            {upcomingMaintenances.map((maintenance, index) => (
              <UpcomingItem
                key={maintenance.id}
                maintenance={maintenance}
                isFirst={index === 0}
                isLast={index === upcomingMaintenances.length - 1}
              />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

interface SelectedDateItemProps {
  maintenance: ScheduledMaintenance;
}

function SelectedDateItem({ maintenance }: SelectedDateItemProps) {
  return (
    <div className="group flex items-center gap-3 py-2 px-3 -mx-3 rounded-lg hover:bg-background cursor-pointer transition-colors">
      <div className="h-1.5 w-1.5 rounded-full bg-energy-solar shrink-0" />
      <div className="flex-1 min-w-0">
        <p className="text-sm truncate">{maintenance.title}</p>
        <p className="text-xs text-muted-foreground truncate">{maintenance.assetName}</p>
      </div>
      {maintenance.recurrence && (
        <div className="h-5 w-5 rounded bg-muted flex items-center justify-center shrink-0">
          <Repeat className="h-3 w-3 text-muted-foreground" />
        </div>
      )}
      <ChevronRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity shrink-0" />
    </div>
  );
}

interface UpcomingItemProps {
  maintenance: ScheduledMaintenance;
  isFirst: boolean;
  isLast: boolean;
}

function UpcomingItem({ maintenance, isFirst, isLast }: UpcomingItemProps) {
  const daysUntil = getDaysUntil(maintenance.scheduledDate);
  const isToday = daysUntil === "Today";
  const isTomorrow = daysUntil === "Tomorrow";

  return (
    <div className="group relative flex gap-3 py-2 hover:bg-background/50 rounded-lg px-1 -mx-1 cursor-pointer transition-colors">
      {/* Timeline dot */}
      <div className="relative z-10 flex items-center justify-center w-5 shrink-0">
        <div className={cn(
          "h-2 w-2 rounded-full ring-2 ring-background",
          isToday ? "bg-energy-solar" : isTomorrow ? "bg-energy-battery" : "bg-muted-foreground/30"
        )} />
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0 py-0.5">
        <div className="flex items-baseline gap-2">
          <span className={cn(
            "text-xs font-medium",
            isToday ? "text-energy-solar" : isTomorrow ? "text-energy-battery" : "text-muted-foreground"
          )}>
            {daysUntil}
          </span>
          {maintenance.recurrence && (
            <span className="text-[10px] text-muted-foreground/70 font-mono">
              {getRecurrenceIcon(maintenance.recurrence)}
            </span>
          )}
        </div>
        <p className="text-sm mt-0.5 truncate">{maintenance.title}</p>
        <p className="text-xs text-muted-foreground truncate">{maintenance.assetName}</p>
      </div>

      {/* Arrow on hover */}
      <ChevronRight className="h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity shrink-0 mt-2" />
    </div>
  );
}
