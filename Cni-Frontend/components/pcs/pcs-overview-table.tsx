"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { PcsUnitListItemDto, PcsAggregatesDto } from "@/lib/pcs/api-types";

interface PcsOverviewTableProps {
  units: PcsUnitListItemDto[];
  aggregates: PcsAggregatesDto;
  onUnitSelect: (unit: PcsUnitListItemDto) => void;
}

type SortField = "name" | "status" | "linkedBess" | "gridMode" | "activePower" | "gridFreq" | "igbtTemp" | "faultCount";
type SortDir = "asc" | "desc";

function badgeColorToClass(color: string): string {
  const map: Record<string, string> = {
    green: "bg-status-operational/15 text-status-operational border-status-operational/30",
    red: "bg-status-critical/15 text-status-critical border-status-critical/30",
    orange: "bg-severity-high/15 text-severity-high border-severity-high/30",
    yellow: "bg-status-standby/15 text-status-standby border-status-standby/30",
    grey: "bg-muted text-muted-foreground",
    gray: "bg-muted text-muted-foreground",
  };
  return map[color] ?? "bg-muted text-muted-foreground";
}

function SortableHeader({
  label,
  field,
  current,
  dir,
  onSort,
}: {
  label: string;
  field: SortField;
  current: SortField;
  dir: SortDir;
  onSort: (f: SortField) => void;
}) {
  const Icon = current === field ? (dir === "asc" ? ArrowUp : ArrowDown) : ArrowUpDown;
  return (
    <button
      className="flex items-center gap-1 text-xs font-medium text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
      onClick={() => onSort(field)}
    >
      {label}
      <Icon className="h-3 w-3" />
    </button>
  );
}

export function PcsOverviewTable({ units, aggregates, onUnitSelect }: PcsOverviewTableProps) {
  const [sortField, setSortField] = useState<SortField>("name");
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir("asc");
    }
  };

  const sorted = useMemo(() => {
    return [...units].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "name": return a.name.localeCompare(b.name) * dir;
        case "status": return a.status.value.localeCompare(b.status.value) * dir;
        case "linkedBess": return (a.linkedBessUnit?.name ?? "").localeCompare(b.linkedBessUnit?.name ?? "") * dir;
        case "gridMode": return a.gridMode.localeCompare(b.gridMode) * dir;
        case "activePower": return (a.activePowerKw - b.activePowerKw) * dir;
        case "gridFreq": return (a.gridFreqHz - b.gridFreqHz) * dir;
        case "igbtTemp": return (a.igbtTempC - b.igbtTempC) * dir;
        case "faultCount": return (a.activeFaultCount - b.activeFaultCount) * dir;
        default: return 0;
      }
    });
  }, [units, sortField, sortDir]);

  return (
    <div className="flex-1 overflow-auto">
      <table className="w-full text-sm">
        <thead className="sticky top-0 bg-muted/50 backdrop-blur-sm z-10">
          <tr className="border-b">
            <th className="text-left px-6 py-2.5"><SortableHeader label="Name" field="name" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-left px-4 py-2.5"><SortableHeader label="Linked BESS" field="linkedBess" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-left px-4 py-2.5"><SortableHeader label="Status" field="status" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-left px-4 py-2.5"><SortableHeader label="Grid Mode" field="gridMode" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-right px-4 py-2.5"><SortableHeader label="Active Power" field="activePower" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-right px-4 py-2.5"><SortableHeader label="Grid Freq" field="gridFreq" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-right px-4 py-2.5"><SortableHeader label="IGBT Temp" field="igbtTemp" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            <th className="text-right px-4 py-2.5"><SortableHeader label="Faults" field="faultCount" current={sortField} dir={sortDir} onSort={handleSort} /></th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((unit) => (
            <tr
              key={unit.id}
              className="border-b hover:bg-muted/30 transition-colors cursor-pointer"
              onClick={() => onUnitSelect(unit)}
            >
              <td className="px-6 py-3 font-medium">{unit.name}</td>
              <td className="px-4 py-3 text-muted-foreground">{unit.linkedBessUnit?.name ?? "—"}</td>
              <td className="px-4 py-3">
                <Badge className={badgeColorToClass(unit.status.color)}>{unit.status.label}</Badge>
              </td>
              <td className="px-4 py-3 text-muted-foreground">{unit.gridMode}</td>
              <td className="px-4 py-3 text-right font-mono">{unit.activePowerKw} kW</td>
              <td className="px-4 py-3 text-right font-mono">{unit.gridFreqHz} Hz</td>
              <td className="px-4 py-3 text-right font-mono">{unit.igbtTempC} °C</td>
              <td className="px-4 py-3 text-right">
                {unit.activeFaultCount > 0 ? (
                  <Badge className="bg-severity-critical/15 text-severity-critical border-severity-critical/30 text-xs">{unit.activeFaultCount}</Badge>
                ) : (
                  <span className="text-muted-foreground">0</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Footer */}
      <div className="sticky bottom-0 border-t bg-muted/30 px-6 py-2.5 text-xs text-muted-foreground">
        {aggregates.totalUnits} PCS units — {aggregates.onlineCount} online, {aggregates.faultCount} faulted — Total: {aggregates.totalActivePowerKw} kW
      </div>
    </div>
  );
}
