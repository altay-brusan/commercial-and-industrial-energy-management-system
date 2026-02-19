"use client";

import { useState, useMemo } from "react";
import { Search, ChevronDown, ChevronUp } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { Input } from "@/components/ui/input";
import { AlarmSummaryStats } from "@/components/alarms/alarm-summary-stats";
import { AlarmFilterToggle, AlarmFilterPanel } from "@/components/alarms/alarm-filter-panel";
import { AlarmTable } from "@/components/alarms/alarm-table";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { alarms as allAlarms, getRelatedMaintenanceTask } from "@/lib/alarms/mock-data";
import type { Alarm, AlarmFilters } from "@/lib/alarms/types";

export default function AlarmsPage() {
  const [filters, setFilters] = useState<AlarmFilters>({});
  const [filterOpen, setFilterOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);
  const [showMobileStats, setShowMobileStats] = useState(false);

  const filteredAlarms = useMemo(() => {
    return allAlarms.filter((alarm) => {
      // Severity filter
      if (filters.severity && filters.severity.length > 0) {
        if (!filters.severity.includes(alarm.severity)) return false;
      }

      // Status filter
      if (filters.status && filters.status.length > 0) {
        if (!filters.status.includes(alarm.status)) return false;
      }

      // Source type filter
      if (filters.sourceType && filters.sourceType.length > 0) {
        if (!filters.sourceType.includes(alarm.sourceType)) return false;
      }

      // Alarm code filter
      if (filters.alarmCode) {
        if (!alarm.code.toLowerCase().includes(filters.alarmCode.toLowerCase())) return false;
      }

      // Location filter
      if (filters.location) {
        if (alarm.location !== filters.location) return false;
      }

      // Maintenance task status filter
      if (filters.maintenanceTaskStatus) {
        const hasTask = !!alarm.relatedMaintenanceTaskId;
        switch (filters.maintenanceTaskStatus) {
          case "has_task":
            if (!hasTask) return false;
            break;
          case "no_task":
            if (hasTask) return false;
            break;
          case "task_in_progress": {
            if (!hasTask) return false;
            const task = getRelatedMaintenanceTask(alarm.relatedMaintenanceTaskId!);
            if (!task || task.status !== "in_progress") return false;
            break;
          }
          case "task_completed": {
            if (!hasTask) return false;
            const task = getRelatedMaintenanceTask(alarm.relatedMaintenanceTaskId!);
            if (!task || task.status !== "completed") return false;
            break;
          }
        }
      }

      // Search query
      if (searchQuery) {
        const q = searchQuery.toLowerCase();
        const match =
          alarm.code.toLowerCase().includes(q) ||
          alarm.message.toLowerCase().includes(q) ||
          alarm.source.toLowerCase().includes(q) ||
          (alarm.location?.toLowerCase().includes(q) ?? false);
        if (!match) return false;
      }

      return true;
    });
  }, [filters, searchQuery]);

  const handleAcknowledge = (alarm: Alarm) => {
    console.log("Acknowledging alarm:", alarm.id);
  };

  const handleResolve = (alarm: Alarm) => {
    console.log("Resolving alarm:", alarm.id);
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="alarms" />

      <div className="flex flex-1 overflow-hidden">
        {/* Main Content */}
        <main className="flex-1 flex flex-col overflow-hidden">
          {/* Header */}
          <div className="shrink-0 bg-background">
            <div className="px-6 py-5">
              <h1 className="text-xl font-semibold">Alarms</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Monitor and manage system alarms across all devices
              </p>
            </div>

            {/* Mobile toggle link */}
            <button
              type="button"
              className="md:hidden flex items-center gap-1 px-6 pb-2 text-xs text-muted-foreground hover:text-foreground transition-colors"
              onClick={() => setShowMobileStats(!showMobileStats)}
            >
              {showMobileStats ? "Hide stats" : "Show stats"}
              {showMobileStats ? (
                <ChevronUp className="h-3 w-3" />
              ) : (
                <ChevronDown className="h-3 w-3" />
              )}
            </button>

            {/* Stats: always visible on desktop, toggleable on mobile */}
            <div className={`${showMobileStats ? "block" : "hidden"} md:block`}>
              <AlarmSummaryStats alarms={allAlarms} />
            </div>

            <div className="flex items-center gap-3 px-6 py-4">
              <AlarmFilterToggle
                filters={filters}
                open={filterOpen}
                onOpenChange={setFilterOpen}
              />
              <div className="relative flex-1 max-w-sm">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder="Search alarms..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
            </div>
          </div>

          {/* Scrollable content area */}
          <div className="flex-1 overflow-y-auto">
            {/* Inline Filter Panel */}
            <AlarmFilterPanel
              filters={filters}
              onFiltersChange={setFilters}
              open={filterOpen}
              onOpenChange={setFilterOpen}
            />

            {/* Alarm Table */}
            {filteredAlarms.length > 0 ? (
              <AlarmTable
                alarms={filteredAlarms}
                selectedAlarmId={selectedAlarm?.id}
                onAlarmSelect={setSelectedAlarm}
              />
            ) : (
              <div className="py-12 text-center text-muted-foreground">
                No alarms found matching your filters.
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="shrink-0 px-6 py-3 border-t text-xs text-muted-foreground">
            Showing {filteredAlarms.length} of {allAlarms.length} alarms
          </div>
        </main>

        {/* Detail Panel */}
        {selectedAlarm && (
          <AlarmDetailPanel
            alarm={selectedAlarm}
            onClose={() => setSelectedAlarm(null)}
            onAcknowledge={handleAcknowledge}
            onResolve={handleResolve}
          />
        )}
      </div>
    </div>
  );
}
