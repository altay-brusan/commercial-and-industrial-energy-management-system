"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { PcsUnit } from "@/lib/pcs/types";
import { getPcsFaultFlagsForUnit } from "@/lib/pcs/mock-data";

interface PcsOverviewTableProps {
  units: PcsUnit[];
  onUnitSelect: (unit: PcsUnit) => void;
}

type SortField = "name" | "status" | "linkedBess" | "gridMode" | "activePower" | "gridFreq" | "igbtTemp" | "faultCount";
type SortDir = "asc" | "desc";

function statusBadge(status: PcsUnit["status"]) {
  const styles: Record<PcsUnit["status"], string> = {
    online: "bg-sky-500/15 text-sky-600 border-sky-500/30",
    offline: "bg-muted text-muted-foreground",
    fault: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    standby: "bg-yellow-500/15 text-yellow-600 border-yellow-500/30",
  };
  return <Badge className={styles[status]} >{status}</Badge>;
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

export function PcsOverviewTable({ units, onUnitSelect }: PcsOverviewTableProps) {
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

  const faultCounts = useMemo(() => {
    const map = new Map<string, number>();
    units.forEach((u) => {
      const flags = getPcsFaultFlagsForUnit(u.id);
      map.set(u.id, flags.filter((f) => f.active).length);
    });
    return map;
  }, [units]);

  const sorted = useMemo(() => {
    return [...units].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "name": return a.name.localeCompare(b.name) * dir;
        case "status": return a.status.localeCompare(b.status) * dir;
        case "linkedBess": return a.linkedBessUnitName.localeCompare(b.linkedBessUnitName) * dir;
        case "gridMode": return a.pcsStatus.gridMode.localeCompare(b.pcsStatus.gridMode) * dir;
        case "activePower": return (a.power.totalActiveKw - b.power.totalActiveKw) * dir;
        case "gridFreq": return (a.pcsStatus.gridFreqHz - b.pcsStatus.gridFreqHz) * dir;
        case "igbtTemp": return (a.temperatures.igbtTempC - b.temperatures.igbtTempC) * dir;
        case "faultCount": return ((faultCounts.get(a.id) ?? 0) - (faultCounts.get(b.id) ?? 0)) * dir;
        default: return 0;
      }
    });
  }, [units, sortField, sortDir, faultCounts]);

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
          {sorted.map((unit) => {
            const fc = faultCounts.get(unit.id) ?? 0;
            return (
              <tr
                key={unit.id}
                className="border-b hover:bg-muted/30 transition-colors cursor-pointer"
                onClick={() => onUnitSelect(unit)}
              >
                <td className="px-6 py-3 font-medium">{unit.name}</td>
                <td className="px-4 py-3 text-muted-foreground">{unit.linkedBessUnitName}</td>
                <td className="px-4 py-3">{statusBadge(unit.status)}</td>
                <td className="px-4 py-3 text-muted-foreground">{unit.pcsStatus.gridMode}</td>
                <td className="px-4 py-3 text-right font-mono">{unit.power.totalActiveKw} kW</td>
                <td className="px-4 py-3 text-right font-mono">{unit.pcsStatus.gridFreqHz} Hz</td>
                <td className="px-4 py-3 text-right font-mono">{unit.temperatures.igbtTempC} °C</td>
                <td className="px-4 py-3 text-right">
                  {fc > 0 ? (
                    <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30 text-xs">{fc}</Badge>
                  ) : (
                    <span className="text-muted-foreground">0</span>
                  )}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>

      {/* Footer */}
      <div className="sticky bottom-0 border-t bg-muted/30 px-6 py-2.5 text-xs text-muted-foreground">
        {units.length} PCS units
      </div>
    </div>
  );
}
