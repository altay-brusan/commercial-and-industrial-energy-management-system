"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Search } from "lucide-react";
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
import { cn } from "@/lib/utils";
import type { SolarInverter, InverterSortField, SortDirection } from "@/lib/solar-inverters/types";

interface InverterTableProps {
  inverters: SolarInverter[];
  onInverterSelect: (inverter: SolarInverter) => void;
}

interface SortableHeaderProps {
  label: string;
  field: InverterSortField;
  currentField: InverterSortField | null;
  direction: SortDirection;
  onSort: (field: InverterSortField) => void;
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

function InverterStatusBadge({ status }: { status: SolarInverter["status"] }) {
  switch (status) {
    case "online":
      return (
        <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 hover:bg-status-operational/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-status-operational" />
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
    case "warning":
      return (
        <Badge className="bg-status-warning/15 text-status-warning border-status-warning/30 hover:bg-status-warning/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-status-warning" />
          Warning
        </Badge>
      );
    case "fault":
      return (
        <Badge className="bg-status-critical/15 text-status-critical border-status-critical/30 hover:bg-status-critical/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-status-critical" />
          Fault
        </Badge>
      );
  }
}

export function InverterTable({ inverters, onInverterSelect }: InverterTableProps) {
  const [sortField, setSortField] = useState<InverterSortField | null>("deviceName");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [searchQuery, setSearchQuery] = useState("");

  const handleSort = (field: InverterSortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const filteredAndSorted = useMemo(() => {
    let result = inverters;

    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      result = result.filter(
        (inv) =>
          inv.deviceName.toLowerCase().includes(q) ||
          inv.serialNumber.toLowerCase().includes(q) ||
          inv.deviceModel.toLowerCase().includes(q)
      );
    }

    if (!sortField) return result;

    return [...result].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "deviceName":
          comparison = a.deviceName.localeCompare(b.deviceName);
          break;
        case "deviceModel":
          comparison = a.deviceModel.localeCompare(b.deviceModel);
          break;
        case "serialNumber":
          comparison = a.serialNumber.localeCompare(b.serialNumber);
          break;
        case "status": {
          const statusOrder = { online: 0, warning: 1, fault: 2, offline: 3 };
          comparison = statusOrder[a.status] - statusOrder[b.status];
          break;
        }
        case "yieldToday":
          comparison = (a.yieldToday ?? -1) - (b.yieldToday ?? -1);
          break;
        case "totalActivePower":
          comparison = (a.totalActivePower ?? -1) - (b.totalActivePower ?? -1);
          break;
        case "associatedPlant":
          comparison = a.associatedPlant.localeCompare(b.associatedPlant);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [inverters, sortField, sortDirection, searchQuery]);

  return (
    <div className="flex flex-col flex-1 min-h-0">
      {/* Search & filters */}
      <div className="shrink-0 flex items-center gap-3 px-6 py-4 border-b">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Device name / Device S/N"
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
              <SortableHeader
                label="Device name"
                field="deviceName"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="pl-6 w-[15%]"
              />
              <SortableHeader
                label="Device model"
                field="deviceModel"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Device S/N"
                field="serialNumber"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[16%]"
              />
              <SortableHeader
                label="Device status"
                field="status"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[13%]"
              />
              <SortableHeader
                label="Yield today"
                field="yieldToday"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Total active power"
                field="totalActivePower"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[15%]"
              />
              <SortableHeader
                label="Associated plant"
                field="associatedPlant"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="pr-6"
              />
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredAndSorted.map((inverter) => (
              <TableRow
                key={inverter.id}
                className="cursor-pointer transition-colors"
                onClick={() => onInverterSelect(inverter)}
              >
                <TableCell className="py-3.5 pl-6 font-medium">
                  {inverter.deviceName}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {inverter.deviceModel}
                </TableCell>
                <TableCell className="py-3.5 font-mono text-xs text-muted-foreground">
                  {inverter.serialNumber}
                </TableCell>
                <TableCell className="py-3.5">
                  <InverterStatusBadge status={inverter.status} />
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {inverter.yieldToday !== null ? `${inverter.yieldToday} kWh` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {inverter.totalActivePower !== null ? `${inverter.totalActivePower} kW` : "--"}
                </TableCell>
                <TableCell className="py-3.5 pr-6 text-muted-foreground">
                  {inverter.associatedPlant}
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
