"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Wrench } from "lucide-react";
import { format } from "date-fns";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { AlarmSeverityBadge } from "./alarm-severity-badge";
import { AlarmStatusBadge } from "./alarm-status-badge";
import { cn } from "@/lib/utils";
import type { Alarm } from "@/lib/alarms/types";
import type { AlarmSeverity } from "@/lib/maintenance/types";

interface AlarmTableProps {
  alarms: Alarm[];
  selectedAlarmId?: string;
  onAlarmSelect: (alarm: Alarm) => void;
}

type SortField = "severity" | "code" | "message" | "source" | "location" | "triggeredAt" | "status" | "task";
type SortDirection = "asc" | "desc";

const severityOrder: Record<AlarmSeverity, number> = {
  critical: 0,
  high: 1,
  medium: 2,
  low: 3,
};

interface SortableHeaderProps {
  label: string;
  field: SortField;
  currentField: SortField | null;
  direction: SortDirection;
  onSort: (field: SortField) => void;
  className?: string;
}

function SortableHeader({ label, field, currentField, direction, onSort, className }: SortableHeaderProps) {
  const isActive = currentField === field;

  return (
    <TableHead
      className={cn(
        "cursor-pointer select-none hover:bg-muted/80 transition-colors",
        "sticky top-0 z-20 bg-muted shadow-[0_1px_2px_rgba(0,0,0,0.05)]",
        className
      )}
      onClick={() => onSort(field)}
    >
      <div className="flex items-center gap-1.5">
        {label}
        {isActive ? (
          direction === "asc" ? (
            <ArrowUp className="h-3.5 w-3.5 text-foreground" />
          ) : (
            <ArrowDown className="h-3.5 w-3.5 text-foreground" />
          )
        ) : (
          <ArrowUpDown className="h-3.5 w-3.5 text-muted-foreground/50" />
        )}
      </div>
    </TableHead>
  );
}

export function AlarmTable({ alarms, selectedAlarmId, onAlarmSelect }: AlarmTableProps) {
  const [sortField, setSortField] = useState<SortField | null>("triggeredAt");
  const [sortDirection, setSortDirection] = useState<SortDirection>("desc");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sortedAlarms = useMemo(() => {
    if (!sortField) return alarms;

    return [...alarms].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "severity":
          comparison = severityOrder[a.severity] - severityOrder[b.severity];
          break;
        case "code":
          comparison = a.code.localeCompare(b.code);
          break;
        case "message":
          comparison = a.message.localeCompare(b.message);
          break;
        case "source":
          comparison = a.source.localeCompare(b.source);
          break;
        case "location":
          comparison = (a.location || "").localeCompare(b.location || "");
          break;
        case "triggeredAt":
          comparison = a.triggeredAt.getTime() - b.triggeredAt.getTime();
          break;
        case "status":
          const statusOrder = { active: 0, acknowledged: 1, resolved: 2 };
          comparison = statusOrder[a.status] - statusOrder[b.status];
          break;
        case "task":
          const aTask = a.relatedMaintenanceTaskId ? 0 : 1;
          const bTask = b.relatedMaintenanceTaskId ? 0 : 1;
          comparison = aTask - bTask;
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [alarms, sortField, sortDirection]);

  return (
    <Table>
      <TableHeader>
        <TableRow className="hover:bg-transparent">
          <SortableHeader
            label="Severity"
            field="severity"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pl-6 w-[100px]"
          />
          <SortableHeader
            label="Code"
            field="code"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[140px]"
          />
          <SortableHeader
            label="Message"
            field="message"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Source"
            field="source"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[150px]"
          />
          <SortableHeader
            label="Location"
            field="location"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[150px]"
          />
          <SortableHeader
            label="Time"
            field="triggeredAt"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[130px]"
          />
          <SortableHeader
            label="Status"
            field="status"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[120px]"
          />
          <SortableHeader
            label="Task"
            field="task"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pr-6 w-[80px]"
          />
        </TableRow>
      </TableHeader>
      <TableBody>
        {sortedAlarms.map((alarm) => (
          <TableRow
            key={alarm.id}
            className={cn(
              "cursor-pointer transition-colors",
              selectedAlarmId === alarm.id && "bg-accent"
            )}
            onClick={() => onAlarmSelect(alarm)}
          >
            <TableCell className="py-3 pl-6">
              <AlarmSeverityBadge severity={alarm.severity} />
            </TableCell>
            <TableCell className="py-3">
              <span className="font-mono text-xs">{alarm.code}</span>
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm truncate max-w-[300px] block">
                {alarm.message}
              </span>
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm">{alarm.source}</span>
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm text-muted-foreground">
                {alarm.location || "-"}
              </span>
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm text-muted-foreground">
                {format(alarm.triggeredAt, "MMM d, HH:mm")}
              </span>
            </TableCell>
            <TableCell className="py-3">
              <AlarmStatusBadge status={alarm.status} />
            </TableCell>
            <TableCell className="py-3 pr-6">
              {alarm.relatedMaintenanceTaskId ? (
                <Badge variant="outline" className="gap-1 text-xs">
                  <Wrench className="h-3 w-3" />
                </Badge>
              ) : (
                <span className="text-muted-foreground text-sm">-</span>
              )}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
