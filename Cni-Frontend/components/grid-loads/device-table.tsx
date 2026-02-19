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
import type { GridDevice, GridDeviceSortField, SortDirection, GridDeviceType } from "@/lib/grid-loads/types";

interface DeviceTableProps {
  devices: GridDevice[];
  onDeviceSelect: (device: GridDevice) => void;
}

interface SortableHeaderProps {
  label: string;
  field: GridDeviceSortField;
  currentField: GridDeviceSortField | null;
  direction: SortDirection;
  onSort: (field: GridDeviceSortField) => void;
  className?: string;
}

const deviceTypeLabels: Record<GridDeviceType, string> = {
  "connection-point": "Grid Connection",
  "transformer": "Transformer",
  "power-meter": "Power Meter",
  "protection-relay": "Protection Relay",
  "circuit-breaker": "Circuit Breaker",
  "switchgear": "Switchgear",
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

function DeviceStatusBadge({ status }: { status: GridDevice["status"] }) {
  switch (status) {
    case "normal":
      return (
        <Badge className="bg-energy-solar/15 text-energy-solar border-energy-solar/30 hover:bg-energy-solar/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-solar" />
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
    case "tripped":
      return (
        <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30 hover:bg-energy-grid/20 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-grid animate-pulse" />
          Tripped
        </Badge>
      );
  }
}

export function DeviceTable({ devices, onDeviceSelect }: DeviceTableProps) {
  const [sortField, setSortField] = useState<GridDeviceSortField | null>("deviceName");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [searchQuery, setSearchQuery] = useState("");

  const handleSort = (field: GridDeviceSortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const filteredAndSorted = useMemo(() => {
    let result = devices;

    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      result = result.filter(
        (d) =>
          d.deviceName.toLowerCase().includes(q) ||
          d.serialNumber.toLowerCase().includes(q) ||
          d.deviceModel.toLowerCase().includes(q) ||
          deviceTypeLabels[d.deviceType].toLowerCase().includes(q)
      );
    }

    if (!sortField) return result;

    return [...result].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "deviceName":
          comparison = a.deviceName.localeCompare(b.deviceName);
          break;
        case "deviceType":
          comparison = deviceTypeLabels[a.deviceType].localeCompare(deviceTypeLabels[b.deviceType]);
          break;
        case "deviceModel":
          comparison = a.deviceModel.localeCompare(b.deviceModel);
          break;
        case "status": {
          const statusOrder = { normal: 0, warning: 1, fault: 2, tripped: 3, offline: 4 };
          comparison = statusOrder[a.status] - statusOrder[b.status];
          break;
        }
        case "activePower":
          comparison = (a.activePower ?? -1) - (b.activePower ?? -1);
          break;
        case "powerFactor":
          comparison = (a.powerFactor ?? -1) - (b.powerFactor ?? -1);
          break;
        case "location":
          comparison = a.location.localeCompare(b.location);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [devices, sortField, sortDirection, searchQuery]);

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
                className="pl-6 w-[13%]"
              />
              <SortableHeader
                label="Device type"
                field="deviceType"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[14%]"
              />
              <SortableHeader
                label="Device model"
                field="deviceModel"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[18%]"
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
                label="Active power (kW)"
                field="activePower"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[14%]"
              />
              <SortableHeader
                label="Power factor"
                field="powerFactor"
                currentField={sortField}
                direction={sortDirection}
                onSort={handleSort}
                className="w-[12%]"
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
            {filteredAndSorted.map((device) => (
              <TableRow
                key={device.id}
                className="cursor-pointer transition-colors"
                onClick={() => onDeviceSelect(device)}
              >
                <TableCell className="py-3.5 pl-6 font-medium">
                  {device.deviceName}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {deviceTypeLabels[device.deviceType]}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {device.deviceModel}
                </TableCell>
                <TableCell className="py-3.5">
                  <DeviceStatusBadge status={device.status} />
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {device.activePower !== null ? `${device.activePower.toLocaleString("en-US")} kW` : "--"}
                </TableCell>
                <TableCell className="py-3.5 text-muted-foreground">
                  {device.powerFactor !== null ? device.powerFactor.toFixed(2) : "--"}
                </TableCell>
                <TableCell className="py-3.5 pr-6 text-muted-foreground">
                  {device.location}
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
