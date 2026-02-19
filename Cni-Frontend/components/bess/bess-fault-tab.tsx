"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Filter, X } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  getBatteryFaultFlagsForUnit,
  getChillerAlarmFlagsForUnit,
} from "@/lib/bess/mock-data";
import type { BatteryFaultFlag, ChillerAlarmFlag } from "@/lib/bess/types";

interface BessFaultTabProps {
  unitId: string;
}

// Unified row type for flat table
interface FaultRow {
  key: string;
  label: string;
  source: "Battery" | "Chiller/TMS";
  category: string;
  severity: "danger" | "warning";
  active: boolean;
}

type SortField = "label" | "source" | "category" | "severity" | "status";
type SortDir = "asc" | "desc";

function buildRows(unitId: string): FaultRow[] {
  const batteryFlags = getBatteryFaultFlagsForUnit(unitId);
  const chillerFlags = getChillerAlarmFlagsForUnit(unitId);

  const batteryRows: FaultRow[] = batteryFlags.map((f) => ({
    key: f.key,
    label: f.label,
    source: "Battery",
    category: f.category,
    severity: f.severity,
    active: f.active,
  }));

  const chillerRows: FaultRow[] = chillerFlags.map((f) => ({
    key: f.key,
    label: f.label,
    source: "Chiller/TMS",
    category: f.category,
    severity: "warning" as const,
    active: f.active,
  }));

  return [...batteryRows, ...chillerRows];
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

export function BessFaultTab({ unitId }: BessFaultTabProps) {
  const allRows = useMemo(() => buildRows(unitId), [unitId]);

  // Filters
  const [sourceFilter, setSourceFilter] = useState<string[]>([]);
  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string[]>([]);

  // Sort
  const [sortField, setSortField] = useState<SortField>("status");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const allSources = useMemo(() => [...new Set(allRows.map((r) => r.source))], [allRows]);
  const allCategories = useMemo(() => [...new Set(allRows.map((r) => r.category))].sort(), [allRows]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir(field === "status" ? "desc" : "asc");
    }
  };

  const filteredRows = useMemo(() => {
    let rows = allRows;
    if (sourceFilter.length > 0) rows = rows.filter((r) => sourceFilter.includes(r.source));
    if (categoryFilter.length > 0) rows = rows.filter((r) => categoryFilter.includes(r.category));
    if (statusFilter.length > 0) {
      rows = rows.filter((r) => {
        const statusStr = r.active ? "Active" : "Clear";
        return statusFilter.includes(statusStr);
      });
    }

    rows = [...rows].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "label":
          return a.label.localeCompare(b.label) * dir;
        case "source":
          return a.source.localeCompare(b.source) * dir;
        case "category":
          return a.category.localeCompare(b.category) * dir;
        case "severity": {
          const sevOrder = { danger: 0, warning: 1 };
          return (sevOrder[a.severity] - sevOrder[b.severity]) * dir;
        }
        case "status": {
          const statOrder = (r: FaultRow) => (r.active ? 0 : 1);
          const diff = statOrder(a) - statOrder(b);
          if (diff !== 0) return diff * dir;
          // Secondary: severity
          const sevOrder2 = { danger: 0, warning: 1 };
          return (sevOrder2[a.severity] - sevOrder2[b.severity]) * dir;
        }
        default:
          return 0;
      }
    });

    return rows;
  }, [allRows, sourceFilter, categoryFilter, statusFilter, sortField, sortDir]);

  const activeCount = filteredRows.filter((r) => r.active).length;
  const totalCount = filteredRows.length;
  const hasActiveFilters = sourceFilter.length > 0 || categoryFilter.length > 0 || statusFilter.length > 0;

  const clearFilters = () => {
    setSourceFilter([]);
    setCategoryFilter([]);
    setStatusFilter([]);
  };

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      {/* Filter bar */}
      <div className="shrink-0 border-b bg-background px-6 py-3 flex items-center gap-3 flex-wrap">
        <Filter className="h-4 w-4 text-muted-foreground" />

        {/* Source filter */}
        <MultiSelectFilter
          label="Source"
          options={allSources}
          selected={sourceFilter}
          onChange={setSourceFilter}
        />

        {/* Category filter */}
        <MultiSelectFilter
          label="Category"
          options={allCategories}
          selected={categoryFilter}
          onChange={setCategoryFilter}
          capitalize
        />

        {/* Status filter */}
        <MultiSelectFilter
          label="Status"
          options={["Active", "Clear"]}
          selected={statusFilter}
          onChange={setStatusFilter}
        />

        {hasActiveFilters && (
          <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={clearFilters}>
            <X className="h-3 w-3 mr-1" />
            Clear
          </Button>
        )}
      </div>

      {/* Table */}
      <div className="flex-1 overflow-auto">
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-muted/50 backdrop-blur-sm z-10">
            <tr className="border-b">
              <th className="text-left px-6 py-2.5">
                <SortableHeader label="Fault Flag" field="label" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-2.5">
                <SortableHeader label="Source" field="source" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-2.5">
                <SortableHeader label="Category" field="category" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-2.5">
                <SortableHeader label="Severity" field="severity" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-2.5">
                <SortableHeader label="Status" field="status" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
            </tr>
          </thead>
          <tbody>
            {filteredRows.map((row) => (
              <tr key={row.key} className={`border-b hover:bg-muted/30 transition-colors ${row.active ? "bg-energy-grid/5" : ""}`}>
                <td className="px-6 py-2.5 font-medium">{row.label}</td>
                <td className="px-4 py-2.5 text-muted-foreground">{row.source}</td>
                <td className="px-4 py-2.5">
                  <Badge variant="outline" className="capitalize text-xs font-normal">
                    {row.category.replace(/_/g, " ")}
                  </Badge>
                </td>
                <td className="px-4 py-2.5">
                  {row.severity === "danger" ? (
                    <Badge className="bg-energy-grid/15 text-energy-grid border-energy-grid/30 text-xs">Danger</Badge>
                  ) : (
                    <Badge className="bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30 text-xs">Warning</Badge>
                  )}
                </td>
                <td className="px-4 py-2.5">
                  {row.active ? (
                    <div className="flex items-center gap-2">
                      <span className="relative flex h-2.5 w-2.5">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-energy-grid opacity-75" />
                        <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-energy-grid" />
                      </span>
                      <span className="text-energy-grid font-medium">Active</span>
                    </div>
                  ) : (
                    <div className="flex items-center gap-2">
                      <span className="inline-flex rounded-full h-2.5 w-2.5 bg-muted-foreground/30" />
                      <span className="text-muted-foreground">Clear</span>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Footer */}
      <div className="shrink-0 border-t bg-muted/30 px-6 py-2.5 text-xs text-muted-foreground">
        {activeCount} active / {totalCount} total fault flags
      </div>
    </div>
  );
}

// --- Reusable Multi-select Filter Popover ---
function MultiSelectFilter({
  label,
  options,
  selected,
  onChange,
  capitalize,
}: {
  label: string;
  options: string[];
  selected: string[];
  onChange: (v: string[]) => void;
  capitalize?: boolean;
}) {
  const toggle = (val: string) => {
    onChange(selected.includes(val) ? selected.filter((s) => s !== val) : [...selected, val]);
  };

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" size="sm" className="h-7 text-xs gap-1">
          {label}
          {selected.length > 0 && (
            <Badge variant="secondary" className="ml-1 h-4 px-1 text-[10px]">
              {selected.length}
            </Badge>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-52 p-3" align="start">
        <div className="space-y-2">
          {options.map((opt) => (
            <div key={opt} className="flex items-center gap-2">
              <Checkbox
                id={`filter-${label}-${opt}`}
                checked={selected.includes(opt)}
                onCheckedChange={() => toggle(opt)}
              />
              <Label htmlFor={`filter-${label}-${opt}`} className={`text-sm cursor-pointer ${capitalize ? "capitalize" : ""}`}>
                {capitalize ? opt.replace(/_/g, " ") : opt}
              </Label>
            </div>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  );
}
