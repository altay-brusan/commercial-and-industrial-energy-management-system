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
import type { Consumer, ConsumerSortField, SortDirection, ConsumerType } from "@/lib/consumption/types";

interface ConsumerTableProps {
  consumers: Consumer[];
  onConsumerSelect: (consumer: Consumer) => void;
}

interface SortableHeaderProps {
  label: string;
  field: ConsumerSortField;
  currentField: ConsumerSortField | null;
  direction: SortDirection;
  onSort: (field: ConsumerSortField) => void;
  className?: string;
}

const consumerTypeLabels: Record<ConsumerType, string> = {
  "commercial-building": "Commercial",
  "industrial-facility": "Industrial",
  "ev-charging-station": "EV Charging",
  "hvac-system": "HVAC",
  "lighting-system": "Lighting",
  "data-center": "Data Center",
  "water-pumping": "Water Pump",
  "cold-storage": "Cold Storage",
};

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

function ConsumerStatusBadge({ status }: { status: Consumer["status"] }) {
  switch (status) {
    case "normal":
      return (
        <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 hover:bg-status-operational/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-status-operational" />
          Normal
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
    case "curtailed":
      return (
        <Badge className="bg-severity-medium/15 text-severity-medium border-severity-medium/30 hover:bg-severity-medium/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-severity-medium animate-pulse" />
          Curtailed
        </Badge>
      );
  }
}

export function ConsumerTable({ consumers, onConsumerSelect }: ConsumerTableProps) {
  const [sortField, setSortField] = useState<ConsumerSortField | null>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [searchQuery, setSearchQuery] = useState("");

  const handleSort = (field: ConsumerSortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const filteredAndSorted = useMemo(() => {
    let result = consumers;

    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      result = result.filter(
        (c) =>
          c.name.toLowerCase().includes(q) ||
          c.serialNumber.toLowerCase().includes(q) ||
          c.meterModel.toLowerCase().includes(q) ||
          c.location.toLowerCase().includes(q)
      );
    }

    if (!sortField) return result;

    return [...result].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "name":
          comparison = a.name.localeCompare(b.name);
          break;
        case "type":
          comparison = consumerTypeLabels[a.type].localeCompare(consumerTypeLabels[b.type]);
          break;
        case "status": {
          const statusOrder = { normal: 0, warning: 1, curtailed: 2, fault: 3, offline: 4 };
          comparison = statusOrder[a.status] - statusOrder[b.status];
          break;
        }
        case "activePower":
          comparison = (a.activePower ?? -1) - (b.activePower ?? -1);
          break;
        case "peakDemandToday":
          comparison = (a.peakDemandToday ?? -1) - (b.peakDemandToday ?? -1);
          break;
        case "consumptionToday":
          comparison = (a.consumptionToday ?? -1) - (b.consumptionToday ?? -1);
          break;
        case "loadFactor":
          comparison = (a.loadFactor ?? -1) - (b.loadFactor ?? -1);
          break;
        case "location":
          comparison = a.location.localeCompare(b.location);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [consumers, sortField, sortDirection, searchQuery]);

  return (
    <div className="flex flex-col flex-1 min-h-0">
      {/* Search & filters */}
      <div className="shrink-0 flex items-center gap-3 px-6 py-4 border-b">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Consumer name / S/N / Location"
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
                label="Consumer"
                field="name"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="pl-6 w-[12%]"
              />
              <SortableHeader
                label="Type"
                field="type"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Status"
                field="status"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Active Power"
                field="activePower"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Peak Demand"
                field="peakDemandToday"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
              />
              <SortableHeader
                label="Consumption"
                field="consumptionToday"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[13%]"
              />
              <SortableHeader
                label="Load Factor"
                field="loadFactor"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[11%]"
              />
              <SortableHeader
                label="Location"
                field="location"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="pr-6"
              />
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredAndSorted.map((consumer) => (
              <TableRow
                key={consumer.id}
                className="cursor-pointer transition-colors"
                onClick={() => onConsumerSelect(consumer)}
              >
                <TableCell className="py-3.5 pl-6 font-medium">
                  {consumer.name}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {consumerTypeLabels[consumer.type]}
                </TableCell>
                <TableCell className="py-3.5">
                  <ConsumerStatusBadge status={consumer.status} />
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {consumer.activePower !== null ? `${consumer.activePower.toLocaleString("en-US")} kW` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {consumer.peakDemandToday !== null ? `${consumer.peakDemandToday.toLocaleString("en-US")} kW` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {consumer.consumptionToday !== null ? `${consumer.consumptionToday.toLocaleString("en-US")} kWh` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {consumer.loadFactor !== null ? `${consumer.loadFactor}%` : "--"}
                </TableCell>
                <TableCell className="py-3.5 pr-6 text-muted-foreground">
                  {consumer.location}
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
