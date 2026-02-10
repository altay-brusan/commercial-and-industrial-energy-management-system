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
import type { BessUnit, BessOverviewSortField, SortDirection } from "@/lib/bess/types";

interface BessOverviewTableProps {
  units: BessUnit[];
  onUnitSelect: (unit: BessUnit) => void;
}

interface SortableHeaderProps {
  label: React.ReactNode;
  field: BessOverviewSortField;
  currentField: BessOverviewSortField | null;
  direction: SortDirection;
  onSort: (field: BessOverviewSortField) => void;
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

function UnitStatusBadge({ status }: { status: BessUnit["status"] }) {
  switch (status) {
    case "online":
      return (
        <Badge className="bg-sky-500/15 text-sky-600 border-sky-500/30 hover:bg-sky-500/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-sky-500" />
          Online
        </Badge>
      );
    case "offline":
      return (
        <Badge variant="secondary" className="gap-1">
          <span className="h-2 w-2 rounded-full bg-muted-foreground" />
          Offline
        </Badge>
      );
    case "fault":
      return (
        <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30 hover:bg-energy-grid/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-grid" />
          Fault
        </Badge>
      );
    case "standby":
      return (
        <Badge className="bg-yellow-500/15 text-yellow-600 border-yellow-500/30 hover:bg-yellow-500/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-yellow-500" />
          Standby
        </Badge>
      );
  }
}

function ModeBadge({ mode }: { mode: BessUnit["operatingMode"] }) {
  const labels: Record<BessUnit["operatingMode"], string> = {
    charging: "Charging",
    discharging: "Discharging",
    idle: "Idle",
    standby: "Standby",
    balancing: "Balancing",
  };
  const colors: Record<BessUnit["operatingMode"], string> = {
    charging: "text-energy-solar",
    discharging: "text-energy-battery",
    idle: "text-muted-foreground",
    standby: "text-energy-consumption",
    balancing: "text-energy-battery",
  };
  return (
    <Badge variant="secondary" className={colors[mode]}>
      {labels[mode]}
    </Badge>
  );
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

function PowerCell({ power, mode }: { power: number; mode: BessUnit["operatingMode"] }) {
  if (mode === "standby" || mode === "idle") {
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

export function BessOverviewTable({ units, onUnitSelect }: BessOverviewTableProps) {
  const [sortField, setSortField] = useState<BessOverviewSortField | null>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [searchQuery, setSearchQuery] = useState("");

  const handleSort = (field: BessOverviewSortField) => {
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
          u.nameplate.serialNumber.toLowerCase().includes(q)
      );
    }

    if (!sortField) return result;

    return [...result].sort((a, b) => {
      let cmp = 0;
      switch (sortField) {
        case "name":
          cmp = a.name.localeCompare(b.name);
          break;
        case "status": {
          const order = { online: 0, standby: 1, fault: 2, offline: 3 };
          cmp = order[a.status] - order[b.status];
          break;
        }
        case "mode": {
          const mOrder = { discharging: 0, charging: 1, balancing: 2, idle: 3, standby: 4 };
          cmp = mOrder[a.operatingMode] - mOrder[b.operatingMode];
          break;
        }
        case "soc":
          cmp = a.live.soc - b.live.soc;
          break;
        case "soh":
          cmp = a.live.soh - b.live.soh;
          break;
        case "power":
          cmp = Math.abs(a.live.power) - Math.abs(b.live.power);
          break;
        case "capacity":
          cmp = a.nameplate.capacityKWh - b.nameplate.capacityKWh;
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
                  <UnitStatusBadge status={unit.status} />
                </TableCell>
                <TableCell className="py-3.5">
                  <ModeBadge mode={unit.operatingMode} />
                </TableCell>
                <TableCell className="py-3.5">
                  <div className="flex items-center gap-2">
                    <Progress
                      value={unit.live.soc}
                      className={cn("h-2 w-16", getSOCColor(unit.live.soc))}
                    />
                    <span className="text-sm text-muted-foreground">{unit.live.soc}%</span>
                  </div>
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {unit.live.soh}%
                </TableCell>
                <TableCell className="py-3.5 text-sm">
                  <PowerCell power={unit.live.power} mode={unit.operatingMode} />
                </TableCell>
                <TableCell className="py-3.5 text-sm text-muted-foreground">
                  {unit.nameplate.capacityKWh} kWh
                </TableCell>
                <TableCell className="py-3.5 pr-6">
                  {unit.activeAlarmCount > 0 ? (
                    <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30">
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
        <span>Total {filteredAndSorted.length}</span>
      </div>
    </div>
  );
}
