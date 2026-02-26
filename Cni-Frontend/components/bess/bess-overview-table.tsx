"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Search, ArrowUpRight, ArrowDownLeft, Minus } from "lucide-react";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";
import type { BessUnitListItemDto, BessAggregatesDto, StatusBadgeDto } from "@/lib/bess/api-types";

type SortField = "name" | "status" | "mode" | "soc" | "soh" | "power" | "capacity" | "alarms";
type SortDirection = "asc" | "desc";

interface BessOverviewTableProps {
  units: BessUnitListItemDto[];
  aggregates: BessAggregatesDto;
  onUnitSelect: (unit: BessUnitListItemDto) => void;
}

interface SortableHeaderProps {
  label: React.ReactNode;
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

function StatusBadgeDisplay({ badge }: { badge: StatusBadgeDto }) {
  const colorMap: Record<string, string> = {
    green: "bg-status-operational/15 text-status-operational border-status-operational/30",
    red: "bg-status-critical/15 text-status-critical border-status-critical/30",
    orange: "bg-severity-high/15 text-severity-high border-severity-high/30",
    yellow: "bg-status-standby/15 text-status-standby border-status-standby/30",
    purple: "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30",
    grey: "bg-muted text-muted-foreground",
    gray: "bg-muted text-muted-foreground",
  };
  const cls = colorMap[badge.color] ?? "bg-muted text-muted-foreground";
  return (
    <Badge className={cn("gap-1", cls)}>
      <span className={cn("h-2 w-2 rounded-full", cls.split(" ")[0]?.replace("/15", ""))} />
      {badge.label}
    </Badge>
  );
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-status-operational";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-severity-high";
  return "[&>[data-slot=progress-indicator]]:bg-severity-critical";
}

function PowerCell({ power }: { power: number | null }) {
  if (power === null || power === 0) {
    return (
      <div className="flex items-center gap-1.5 text-muted-foreground">
        <Minus className="h-3.5 w-3.5" />
        <span>0 kW</span>
      </div>
    );
  }
  const isCharging = power < 0;
  return (
    <div className={cn("flex items-center gap-1.5", isCharging ? "text-energy-solar" : "text-energy-battery")}>
      {isCharging ? (
        <ArrowDownLeft className="h-3.5 w-3.5" />
      ) : (
        <ArrowUpRight className="h-3.5 w-3.5" />
      )}
      <span>{Math.abs(power)} kW</span>
    </div>
  );
}

export function BessOverviewTable({ units, aggregates, onUnitSelect }: BessOverviewTableProps) {
  const [sortField, setSortField] = useState<SortField | null>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [searchQuery, setSearchQuery] = useState("");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const filteredAndSorted = useMemo(() => {
    let result = units;

    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      result = result.filter(
        (u) =>
          u.name.toLowerCase().includes(q) ||
          (u.serialNumber && u.serialNumber.toLowerCase().includes(q))
      );
    }

    if (!sortField) return result;

    return [...result].sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case "name":
          cmp = a.name.localeCompare(b.name);
          break;
        case "status":
          cmp = a.status.label.localeCompare(b.status.label);
          break;
        case "mode":
          cmp = a.operatingMode.label.localeCompare(b.operatingMode.label);
          break;
        case "soc":
          cmp = (a.soc ?? 0) - (b.soc ?? 0);
          break;
        case "soh":
          cmp = (a.soh ?? 0) - (b.soh ?? 0);
          break;
        case "power":
          cmp = Math.abs(a.power ?? 0) - Math.abs(b.power ?? 0);
          break;
        case "capacity":
          cmp = (a.capacityKwh ?? 0) - (b.capacityKwh ?? 0);
          break;
        case "alarms":
          cmp = a.activeAlarmCount - b.activeAlarmCount;
          break;
      }
      return sortDirection === "asc" ? cmp : -cmp;
    });
  }, [units, sortField, sortDirection, searchQuery]);

  return (
    <div className="flex flex-col h-full">
      {/* Search */}
      <div className="shrink-0 flex items-center gap-3 px-6 py-4 border-b">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Unit name / Serial number"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
      </div>

      {/* Table */}
      <div className="flex-1 overflow-y-auto">
        <Table className="table-fixed">
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <SortableHeader label="Name" field="name" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pl-6 w-[15%]" />
              <SortableHeader label="Status" field="status" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[10%]" />
              <SortableHeader label="Mode" field="mode" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[12%]" />
              <SortableHeader label={<AbbrTooltip label="SoC" title="State of Charge" description="Current battery charge level as a percentage of total capacity" />} field="soc" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[15%]" />
              <SortableHeader label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} field="soh" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[8%]" />
              <SortableHeader label="Power" field="power" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[14%]" />
              <SortableHeader label="Capacity" field="capacity" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[10%]" />
              <SortableHeader label="Alarms" field="alarms" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pr-6 w-[10%]" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredAndSorted.map((unit) => (
              <TableRow
                key={unit.id}
                className="cursor-pointer transition-colors"
                onClick={() => onUnitSelect(unit)}
              >
                <TableCell className="py-3.5 pl-6 font-medium">{unit.name}</TableCell>
                <TableCell className="py-3.5">
                  <StatusBadgeDisplay badge={unit.status} />
                </TableCell>
                <TableCell className="py-3.5">
                  <StatusBadgeDisplay badge={unit.operatingMode} />
                </TableCell>
                <TableCell className="py-3.5">
                  {unit.soc !== null ? (
                    <div className="flex items-center gap-2">
                      <Progress
                        value={unit.soc}
                        className={cn("h-2 w-16", getSOCColor(unit.soc))}
                      />
                      <span className="text-sm text-muted-foreground">{unit.soc}%</span>
                    </div>
                  ) : (
                    <span className="text-sm text-muted-foreground">--</span>
                  )}
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {unit.soh !== null ? `${unit.soh}%` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-sm">
                  <PowerCell power={unit.power} />
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {unit.capacityKwh !== null ? `${unit.capacityKwh} kWh` : "--"}
                </TableCell>
                <TableCell className="py-3.5 pr-6">
                  {unit.activeAlarmCount > 0 ? (
                    <Badge className="bg-severity-critical/15 text-severity-critical border-severity-critical/30">
                      {unit.activeAlarmCount}
                    </Badge>
                  ) : (
                    <span className="text-sm text-muted-foreground">0</span>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Footer */}
      <div className="shrink-0 px-6 py-3 border-t text-xs text-muted-foreground flex items-center justify-between">
        <span>Total {aggregates.totalUnits} units</span>
        <div className="flex items-center gap-4">
          {aggregates.totalCapacityKwh !== null && (
            <span>{aggregates.totalCapacityKwh} kWh total</span>
          )}
          {aggregates.averageSoc !== null && (
            <span>Avg SoC: {aggregates.averageSoc}%</span>
          )}
          {aggregates.totalActiveAlarms > 0 && (
            <span className="text-severity-critical">{aggregates.totalActiveAlarms} active alarms</span>
          )}
        </div>
      </div>
    </div>
  );
}
