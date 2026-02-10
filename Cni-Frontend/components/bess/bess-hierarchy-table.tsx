"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
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
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";
import type {
  BessRack,
  BessPack,
  BessModule,
  BessCell,
  BessComponentStatus,
  HierarchySortField,
  SortDirection,
  HierarchyLevel,
} from "@/lib/bess/types";

type HierarchyItem = BessRack | BessPack | BessModule | BessCell;

interface BessHierarchyTableProps {
  level: Exclude<HierarchyLevel, "unit" | "cell">;
  items: HierarchyItem[];
  onItemSelect: (item: HierarchyItem) => void;
}

interface SortableHeaderProps {
  label: React.ReactNode;
  field: HierarchySortField;
  currentField: HierarchySortField | null;
  direction: SortDirection;
  onSort: (field: HierarchySortField) => void;
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

function ComponentStatusBadge({ status }: { status: BessComponentStatus }) {
  switch (status) {
    case "normal":
      return (
        <Badge className="bg-energy-solar/15 text-energy-solar border-energy-solar/30 hover:bg-energy-solar/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-solar" />
          Normal
        </Badge>
      );
    case "warning":
      return (
        <Badge className="bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30 hover:bg-energy-consumption/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-consumption" />
          Warning
        </Badge>
      );
    case "fault":
      return (
        <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30 hover:bg-energy-grid/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-grid" />
          Fault
        </Badge>
      );
    case "offline":
      return (
        <Badge variant="secondary" className="gap-1">
          <span className="h-2 w-2 rounded-full bg-muted-foreground" />
          Offline
        </Badge>
      );
  }
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30) return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

function getLabel(item: HierarchyItem): string {
  if ("rackNumber" in item) return `Rack ${item.rackNumber}`;
  if ("packNumber" in item) return `Pack ${item.packNumber}`;
  if ("moduleNumber" in item) return `Module ${item.moduleNumber}`;
  return `Cell ${item.cellNumber}`;
}

function isCell(item: HierarchyItem): item is BessCell {
  return "cellNumber" in item;
}

function getSortValue(item: HierarchyItem, field: HierarchySortField): number | string {
  switch (field) {
    case "label":
      return getLabel(item);
    case "status": {
      const order: Record<BessComponentStatus, number> = { normal: 0, warning: 1, fault: 2, offline: 3 };
      return order[item.status];
    }
    case "soc":
      return item.soc;
    case "soh":
      return item.soh;
    case "voltage":
      return item.voltage;
    case "current":
      return item.current;
    case "temperature":
      return isCell(item) ? item.temperature : 0;
    case "tempMin":
      return isCell(item) ? item.temperature : ("tempMin" in item ? item.tempMin : 0);
    case "tempMax":
      return isCell(item) ? item.temperature : ("tempMax" in item ? item.tempMax : 0);
    case "tempAvg":
      return isCell(item) ? item.temperature : ("tempAvg" in item ? item.tempAvg : 0);
  }
}

export function BessHierarchyTable({ level, items, onItemSelect }: BessHierarchyTableProps) {
  const [sortField, setSortField] = useState<HierarchySortField | null>("label");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");

  const handleSort = (field: HierarchySortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sorted = useMemo(() => {
    if (!sortField) return items;
    return [...items].sort((a, b) => {
      const va = getSortValue(a, sortField);
      const vb = getSortValue(b, sortField);
      let cmp = 0;
      if (typeof va === "string" && typeof vb === "string") {
        cmp = va.localeCompare(vb, undefined, { numeric: true });
      } else {
        cmp = (va as number) - (vb as number);
      }
      return sortDirection === "asc" ? cmp : -cmp;
    });
  }, [items, sortField, sortDirection]);

  const isItemCell = items.length > 0 && isCell(items[0]);
  const levelLabel = level === "rack" ? "Racks" : level === "pack" ? "Packs" : level === "module" ? "Modules" : "Cells";

  return (
    <div className="flex flex-col">
      <div className="overflow-y-auto">
        <Table className="table-fixed">
          <TableHeader>
            <TableRow className="hover:bg-transparent">
              <SortableHeader label="Name" field="label" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pl-6 w-[14%]" />
              <SortableHeader label="Status" field="status" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[12%]" />
              <SortableHeader label={<AbbrTooltip label="SoC" title="State of Charge" description="Current battery charge level as a percentage of total capacity" />} field="soc" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[16%]" />
              <SortableHeader label={<AbbrTooltip label="SoH" title="State of Health" description="Battery's current capacity compared to its original rated capacity" />} field="soh" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[8%]" />
              <SortableHeader label="Voltage" field="voltage" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[12%]" />
              <SortableHeader label="Current" field="current" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[10%]" />
              {isItemCell ? (
                <SortableHeader label="Temp" field="temperature" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pr-6 w-[10%]" />
              ) : (
                <>
                  <SortableHeader label="T Min" field="tempMin" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[9%]" />
                  <SortableHeader label="T Max" field="tempMax" currentField={sortField} direction={sortDirection} onSort={handleSort} className="w-[9%]" />
                  <SortableHeader label="T Avg" field="tempAvg" currentField={sortField} direction={sortDirection} onSort={handleSort} className="pr-6 w-[9%]" />
                </>
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {sorted.map((item) => (
              <TableRow
                key={item.id}
                className="cursor-pointer transition-colors"
                onClick={() => onItemSelect(item)}
              >
                <TableCell className="py-3 pl-6 font-medium text-sm">
                  {getLabel(item)}
                </TableCell>
                <TableCell className="py-3">
                  <ComponentStatusBadge status={item.status} />
                </TableCell>
                <TableCell className="py-3">
                  <div className="flex items-center gap-2">
                    <Progress value={item.soc} className={cn("h-2 w-12", getSOCColor(item.soc))} />
                    <span className="text-xs text-muted-foreground">{item.soc}%</span>
                  </div>
                </TableCell>
                <TableCell className="py-3 text-sm text-muted-foreground">
                  {item.soh}%
                </TableCell>
                <TableCell className="py-3 text-sm text-muted-foreground">
                  {item.voltage} V
                </TableCell>
                <TableCell className="py-3 text-sm text-muted-foreground">
                  {item.current} A
                </TableCell>
                {isCell(item) ? (
                  <TableCell className="py-3 pr-6 text-sm text-muted-foreground">
                    {item.temperature}°C
                  </TableCell>
                ) : (
                  <>
                    <TableCell className="py-3 text-sm text-muted-foreground">
                      {"tempMin" in item ? `${item.tempMin}°C` : "--"}
                    </TableCell>
                    <TableCell className="py-3 text-sm text-muted-foreground">
                      {"tempMax" in item ? `${item.tempMax}°C` : "--"}
                    </TableCell>
                    <TableCell className="py-3 pr-6 text-sm text-muted-foreground">
                      {"tempAvg" in item ? `${item.tempAvg}°C` : "--"}
                    </TableCell>
                  </>
                )}
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="shrink-0 px-6 py-2 border-t text-xs text-muted-foreground">
        Total {sorted.length} {levelLabel.toLowerCase()}
      </div>
    </div>
  );
}
