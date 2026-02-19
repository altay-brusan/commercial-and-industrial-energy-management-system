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
import { getPcsFaultFlagsForUnit } from "@/lib/pcs/mock-data";
import type { PcsFaultFlag } from "@/lib/pcs/types";

interface PcsFaultTabProps {
  pcsId: string;
}

type SortField = "label" | "category" | "status";
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

function MultiSelectFilter({
  label,
  options,
  selected,
  onChange,
}: {
  label: string;
  options: string[];
  selected: string[];
  onChange: (v: string[]) => void;
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
                id={`pcs-filter-${label}-${opt}`}
                checked={selected.includes(opt)}
                onCheckedChange={() => toggle(opt)}
              />
              <Label htmlFor={`pcs-filter-${label}-${opt}`} className="text-sm cursor-pointer capitalize">
                {opt.replace(/_/g, " ")}
              </Label>
            </div>
          ))}
        </div>
      </PopoverContent>
    </Popover>
  );
}

export function PcsFaultTab({ pcsId }: PcsFaultTabProps) {
  const allFlags = useMemo(() => getPcsFaultFlagsForUnit(pcsId), [pcsId]);

  const [categoryFilter, setCategoryFilter] = useState<string[]>([]);
  const [statusFilter, setStatusFilter] = useState<string[]>([]);
  const [sortField, setSortField] = useState<SortField>("status");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const allCategories = useMemo(() => [...new Set(allFlags.map((f) => f.category))].sort(), [allFlags]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir(field === "status" ? "desc" : "asc");
    }
  };

  const filteredFlags = useMemo(() => {
    let flags = allFlags;
    if (categoryFilter.length > 0) flags = flags.filter((f) => categoryFilter.includes(f.category));
    if (statusFilter.length > 0) {
      flags = flags.filter((f) => {
        const statusStr = f.active ? "Active" : "Clear";
        return statusFilter.includes(statusStr);
      });
    }

    flags = [...flags].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "label": return a.label.localeCompare(b.label) * dir;
        case "category": return a.category.localeCompare(b.category) * dir;
        case "status": {
          const diff = (a.active ? 0 : 1) - (b.active ? 0 : 1);
          return diff !== 0 ? diff * dir : a.category.localeCompare(b.category);
        }
        default: return 0;
      }
    });

    return flags;
  }, [allFlags, categoryFilter, statusFilter, sortField, sortDir]);

  const activeCount = filteredFlags.filter((f) => f.active).length;
  const hasActiveFilters = categoryFilter.length > 0 || statusFilter.length > 0;

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      {/* Filter bar */}
      <div className="shrink-0 border-b bg-background px-6 py-3 flex items-center gap-3 flex-wrap">
        <Filter className="h-4 w-4 text-muted-foreground" />
        <MultiSelectFilter label="Category" options={allCategories} selected={categoryFilter} onChange={setCategoryFilter} />
        <MultiSelectFilter label="Status" options={["Active", "Clear"]} selected={statusFilter} onChange={setStatusFilter} />
        {hasActiveFilters && (
          <Button variant="ghost" size="sm" className="h-7 text-xs" onClick={() => { setCategoryFilter([]); setStatusFilter([]); }}>
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
                <SortableHeader label="Category" field="category" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
              <th className="text-left px-4 py-2.5">
                <SortableHeader label="Status" field="status" current={sortField} dir={sortDir} onSort={handleSort} />
              </th>
            </tr>
          </thead>
          <tbody>
            {filteredFlags.map((flag) => (
              <tr key={flag.key} className={`border-b hover:bg-muted/30 transition-colors ${flag.active ? "bg-severity-critical/5" : ""}`}>
                <td className="px-6 py-2.5 font-medium">{flag.label}</td>
                <td className="px-4 py-2.5">
                  <Badge variant="outline" className="capitalize text-xs font-normal">
                    {flag.category.replace(/_/g, " ")}
                  </Badge>
                </td>
                <td className="px-4 py-2.5">
                  {flag.active ? (
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
        {activeCount} active / {filteredFlags.length} total fault flags
      </div>
    </div>
  );
}
