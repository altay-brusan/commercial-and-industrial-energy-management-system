"use client";

import { useState, useEffect, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Filter, X, Loader2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import * as pcsService from "@/lib/pcs/pcs-service";
import type { FaultFlagDto, FaultFlagsResponseDto } from "@/lib/pcs/api-types";

interface PcsFaultTabProps {
  unitId: string;
}

type SortField = "label" | "source" | "category" | "severity" | "status";
type SortDir = "asc" | "desc";

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

function severityBadge(severity: string) {
  const map: Record<string, string> = {
    critical: "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
    high: "bg-severity-high/15 text-severity-high border-severity-high/30",
    danger: "bg-energy-grid/15 text-energy-grid border-energy-grid/30",
    medium: "bg-severity-medium/15 text-severity-medium border-severity-medium/30",
    warning: "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30",
    low: "bg-muted text-muted-foreground",
  };
  const cls = map[severity.toLowerCase()] ?? "bg-muted text-muted-foreground";
  return <Badge className={`text-xs ${cls}`}>{severity}</Badge>;
}

function MultiSelectFilter({
  label, options, selected, onChange, capitalize,
}: {
  label: string; options: string[]; selected: string[]; onChange: (v: string[]) => void; capitalize?: boolean;
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
            <Badge variant="secondary" className="ml-1 h-4 px-1 text-[10px]">{selected.length}</Badge>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-52 p-3" align="start">
        <div className="space-y-2">
          {options.map((opt) => (
            <div key={opt} className="flex items-center gap-2">
              <Checkbox id={`pcs-filter-${label}-${opt}`} checked={selected.includes(opt)} onCheckedChange={() => toggle(opt)} />
              <Label htmlFor={`pcs-filter-${label}-${opt}`} className={`text-sm cursor-pointer ${capitalize ? "capitalize" : ""}`}>
                {capitalize ? opt.replace(/_/g, " ") : opt}
              </Label>
            </div>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  );
}

export function PcsFaultTab({ unitId }: PcsFaultTabProps) {
  const [data, setData] = useState<FaultFlagsResponseDto | null>(null);
  const [loading, setLoading] = useState(true);

  const [sourceFilter, setSourceFilter] = useState<string[]>([]);
  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string[]>([]);
  const [sortField, setSortField] = useState<SortField>("status");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  useEffect(() => {
    setLoading(true);
    pcsService.getUnitFaultFlags(unitId).then((res) => {
      setData(res);
    }).catch(() => {}).finally(() => setLoading(false));
  }, [unitId]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir(field === "status" ? "desc" : "asc");
    }
  };

  const flags = data?.flags ?? [];
  const allSources = useMemo(() => [...new Set(flags.map((f) => f.source).filter(Boolean) as string[])], [flags]);
  const allCategories = useMemo(() => [...new Set(flags.map((f) => f.category).filter(Boolean) as string[])].sort(), [flags]);

  const filteredRows = useMemo(() => {
    let rows = flags;
    if (sourceFilter.length > 0) rows = rows.filter((r) => r.source && sourceFilter.includes(r.source));
    if (categoryFilter.length > 0) rows = rows.filter((r) => r.category && categoryFilter.includes(r.category));
    if (statusFilter.length > 0) {
      rows = rows.filter((r) => {
        const s = r.isActive ? "Active" : "Clear";
        return statusFilter.includes(s);
      });
    }

    return [...rows].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "label": return a.label.localeCompare(b.label) * dir;
        case "source": return (a.source ?? "").localeCompare(b.source ?? "") * dir;
        case "category": return (a.category ?? "").localeCompare(b.category ?? "") * dir;
        case "severity": return a.severity.localeCompare(b.severity) * dir;
        case "status": {
          const sa = a.isActive ? 0 : 1;
          const sb = b.isActive ? 0 : 1;
          return (sa - sb) * dir;
        }
        default: return 0;
      }
    });
  }, [flags, sourceFilter, categoryFilter, statusFilter, sortField, sortDir]);

  const activeCount = filteredRows.filter((r) => r.isActive).length;
  const hasActiveFilters = sourceFilter.length > 0 || categoryFilter.length > 0 || statusFilter.length > 0;
  const clearFilters = () => { setSourceFilter([]); setCategoryFilter([]); setStatusFilter([]); };

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (flags.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No fault flags for this unit.
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      {/* Filter bar */}
      <div className="shrink-0 border-b bg-background px-6 py-3 flex items-center gap-3 flex-wrap">
        <Filter className="h-4 w-4 text-muted-foreground" />
        {allSources.length > 1 && (
          <MultiSelectFilter label="Source" options={allSources} selected={sourceFilter} onChange={setSourceFilter} />
        )}
        {allCategories.length > 1 && (
          <MultiSelectFilter label="Category" options={allCategories} selected={categoryFilter} onChange={setCategoryFilter} capitalize />
        )}
        <MultiSelectFilter label="Status" options={["Active", "Clear"]} selected={statusFilter} onChange={setStatusFilter} />
        {hasActiveFilters && (
          <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={clearFilters}>
            <X className="h-3 w-3 mr-1" /> Clear
          </Button>
        )}
      </div>

      {/* Table */}
      <div className="flex-1 overflow-auto">
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-muted/50 backdrop-blur-sm z-10">
            <tr className="border-b">
              <th className="text-left px-6 py-2.5"><SortableHeader label="Fault Flag" field="label" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Source" field="source" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Category" field="category" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Severity" field="severity" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Status" field="status" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            </tr>
          </thead>
          <tbody>
            {filteredRows.map((row) => (
              <tr key={row.key} className={`border-b hover:bg-muted/30 transition-colors ${row.isActive ? "bg-severity-critical/5" : ""}`}>
                <td className="px-6 py-2.5 font-medium">{row.label}</td>
                <td className="px-4 py-2.5 text-muted-foreground">{row.source ?? "--"}</td>
                <td className="px-4 py-2.5">
                  {row.category ? (
                    <Badge variant="outline" className="capitalize text-xs font-normal">{row.category.replace(/_/g, " ")}</Badge>
                  ) : "--"}
                </td>
                <td className="px-4 py-2.5">{severityBadge(row.severity)}</td>
                <td className="px-4 py-2.5">
                  {row.isActive ? (
                    <div className="flex items-center gap-2">
                      <span className="relative flex h-2.5 w-2.5">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-severity-critical opacity-75" />
                        <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-severity-critical" />
                      </span>
                      <span className="text-severity-critical font-medium">Active</span>
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
        {activeCount} active / {filteredRows.length} total fault flags
      </div>
    </div>
  );
}
