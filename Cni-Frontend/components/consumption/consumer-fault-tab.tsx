"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, ExternalLink } from "lucide-react";
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
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getFaultsForConsumer } from "@/lib/consumption/mock-data";
import type { FaultSeverity, FaultStatus } from "@/lib/consumption/types";

interface ConsumerFaultTabProps {
  consumerId: string;
  onAlarmOpen: (alarmId: string) => void;
}

type FaultSortField = "code" | "name" | "severity" | "occurredAt" | "recoveredAt" | "status";
type SortDir = "asc" | "desc";

const severityOrder: Record<FaultSeverity, number> = {
  critical: 0,
  major: 1,
  minor: 2,
  warning: 3,
};

function FaultSeverityBadge({ severity }: { severity: FaultSeverity }) {
  const styles: Record<FaultSeverity, string> = {
    critical: "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
    major: "bg-severity-high/15 text-severity-high border-severity-high/30",
    minor: "bg-severity-minor/15 text-severity-minor border-severity-minor/30",
    warning: "bg-status-standby/15 text-status-standby border-status-standby/30",
  };

  return (
    <Badge className={cn("capitalize", styles[severity])}>
      {severity}
    </Badge>
  );
}

function FaultStatusBadge({ status }: { status: FaultStatus }) {
  if (status === "active") {
    return (
      <Badge className="bg-severity-critical/15 text-severity-critical border-severity-critical/30 gap-1">
        <span className="h-1.5 w-1.5 rounded-full bg-severity-critical animate-pulse" />
        Active
      </Badge>
    );
  }
  return (
    <Badge variant="outline" className="text-muted-foreground gap-1">
      <span className="h-1.5 w-1.5 rounded-full bg-muted-foreground" />
      Recovered
    </Badge>
  );
}

interface SortableHeaderProps {
  label: string;
  field: FaultSortField;
  currentField: FaultSortField | null;
  direction: SortDir;
  onSort: (field: FaultSortField) => void;
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

export function ConsumerFaultTab({ consumerId, onAlarmOpen }: ConsumerFaultTabProps) {
  const faults = useMemo(() => getFaultsForConsumer(consumerId), [consumerId]);
  const [sortField, setSortField] = useState<FaultSortField | null>("occurredAt");
  const [sortDirection, setSortDirection] = useState<SortDir>("desc");

  const handleSort = (field: FaultSortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sorted = useMemo(() => {
    if (!sortField) return faults;
    return [...faults].sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case "code":
          cmp = a.code.localeCompare(b.code);
          break;
        case "name":
          cmp = a.name.localeCompare(b.name);
          break;
        case "severity":
          cmp = severityOrder[a.severity] - severityOrder[b.severity];
          break;
        case "occurredAt":
          cmp = a.occurredAt.getTime() - b.occurredAt.getTime();
          break;
        case "recoveredAt":
          cmp = (a.recoveredAt?.getTime() ?? 0) - (b.recoveredAt?.getTime() ?? 0);
          break;
        case "status": {
          const so: Record<FaultStatus, number> = { active: 0, recovered: 1 };
          cmp = so[a.status] - so[b.status];
          break;
        }
      }
      return sortDirection === "asc" ? cmp : -cmp;
    });
  }, [faults, sortField, sortDirection]);

  if (faults.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No fault records found for this consumer.
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-y-auto">
        <Table>
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <SortableHeader label="Fault code" field="code" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pl-6 w-[120px]" />
              <SortableHeader label="Fault name" field="name" currentField={sortField} direction={sortDirection} onSort={handleSort} />
              <SortableHeader label="Severity" field="severity" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[120px]" />
              <SortableHeader label="Occurred at" field="occurredAt" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[170px]" />
              <SortableHeader label="Recovered at" field="recoveredAt" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[170px]" />
              <SortableHeader label="Status" field="status" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[120px]" />
              <TableHead className="sticky top-0 z-20 bg-muted shadow-[0_1px_2px_rgba(0,0,0,0.05)] pr-6 w-[80px]">
                Alarm
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {sorted.map((fault) => (
              <TableRow key={fault.id} className="group">
                <TableCell className="py-3.5 pl-6 font-mono text-xs">{fault.code}</TableCell>
                <TableCell className="py-3.5 text-sm">{fault.name}</TableCell>
                <TableCell className="py-3.5">
                  <FaultSeverityBadge severity={fault.severity} />
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {format(fault.occurredAt, "dd/MMM/yyyy HH:mm")}
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {fault.recoveredAt ? format(fault.recoveredAt, "dd/MMM/yyyy HH:mm") : "--"}
                </TableCell>
                <TableCell className="py-3.5">
                  <FaultStatusBadge status={fault.status} />
                </TableCell>
                <TableCell className="py-3.5 pr-6">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7 opacity-60 group-hover:opacity-100 transition-opacity"
                    onClick={() => onAlarmOpen(fault.relatedAlarmId)}
                    title="View alarm detail"
                  >
                    <ExternalLink className="h-3.5 w-3.5" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="shrink-0 px-6 py-3 border-t text-xs text-muted-foreground">
        Total {sorted.length} fault record{sorted.length !== 1 ? "s" : ""}
      </div>
    </div>
  );
}
