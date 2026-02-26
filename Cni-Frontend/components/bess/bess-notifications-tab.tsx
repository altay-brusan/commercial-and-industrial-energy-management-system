"use client";

import { useState, useEffect, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown, Loader2 } from "lucide-react";
import { format } from "date-fns";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import * as bessService from "@/lib/bess/bess-service";
import type { FaultRecordDto } from "@/lib/bess/api-types";

interface BessNotificationsTabProps {
  deviceId: string;
}

type SortField = "code" | "name" | "severity" | "source" | "occurredAt" | "recoveredAt" | "status";
type SortDir = "asc" | "desc";

function statusBadge(status: { value: string; label: string; color: string }) {
  const map: Record<string, string> = {
    red: "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
    orange: "bg-severity-high/15 text-severity-high border-severity-high/30",
    yellow: "bg-severity-medium/15 text-severity-medium border-severity-medium/30",
    green: "bg-status-operational/15 text-status-operational border-status-operational/30",
    grey: "bg-muted text-muted-foreground",
    gray: "bg-muted text-muted-foreground",
  };
  const cls = map[status.color] ?? "bg-muted text-muted-foreground";
  return <Badge className={cn("text-xs gap-1.5", cls)}>{status.label}</Badge>;
}

function SortableHeader({
  label, field, current, dir, onSort,
}: {
  label: string; field: SortField; current: SortField; dir: SortDir; onSort: (f: SortField) => void;
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

export function BessNotificationsTab({ deviceId }: BessNotificationsTabProps) {
  const [faults, setFaults] = useState<FaultRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [sortField, setSortField] = useState<SortField>("occurredAt");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  useEffect(() => {
    setLoading(true);
    bessService.getDeviceNotifications(deviceId, { pageSize: 200 }).then((res) => {
      setFaults(res.faults);
    }).catch(() => {}).finally(() => setLoading(false));
  }, [deviceId]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir(field === "occurredAt" ? "desc" : "asc");
    }
  };

  const sorted = useMemo(() => {
    return [...faults].sort((a, b) => {
      const dir = sortDir === "asc" ? 1 : -1;
      switch (sortField) {
        case "code": return a.code.localeCompare(b.code) * dir;
        case "name": return a.name.localeCompare(b.name) * dir;
        case "severity": return a.severity.label.localeCompare(b.severity.label) * dir;
        case "source": return (a.source ?? "").localeCompare(b.source ?? "") * dir;
        case "occurredAt": return a.occurredAt.localeCompare(b.occurredAt) * dir;
        case "recoveredAt": return (a.recoveredAt ?? "").localeCompare(b.recoveredAt ?? "") * dir;
        case "status": return a.status.label.localeCompare(b.status.label) * dir;
        default: return 0;
      }
    });
  }, [faults, sortField, sortDir]);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (faults.length === 0) {
    return (
      <div className="flex-1 flex items-center justify-center text-muted-foreground py-20">
        No fault records found.
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col overflow-hidden">
      <div className="flex-1 overflow-auto">
        <table className="w-full text-sm">
          <thead className="sticky top-0 bg-muted/50 backdrop-blur-sm z-10">
            <tr className="border-b">
              <th className="text-left px-6 py-2.5"><SortableHeader label="Code" field="code" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Name" field="name" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Severity" field="severity" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Source" field="source" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Occurred" field="occurredAt" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Recovered" field="recoveredAt" current={sortField} dir={sortDir} onSort={handleSort} /></th>
              <th className="text-left px-4 py-2.5"><SortableHeader label="Status" field="status" current={sortField} dir={sortDir} onSort={handleSort} /></th>
            </tr>
          </thead>
          <tbody>
            {sorted.map((fault) => (
              <tr key={fault.id} className="border-b hover:bg-muted/30 transition-colors">
                <td className="px-6 py-2.5 font-mono text-xs">{fault.code}</td>
                <td className="px-4 py-2.5 font-medium">{fault.name}</td>
                <td className="px-4 py-2.5">{statusBadge(fault.severity)}</td>
                <td className="px-4 py-2.5 text-muted-foreground">{fault.source ?? "--"}</td>
                <td className="px-4 py-2.5 text-muted-foreground">
                  {format(new Date(fault.occurredAt), "dd/MMM/yyyy HH:mm")}
                </td>
                <td className="px-4 py-2.5 text-muted-foreground">
                  {fault.recoveredAt ? format(new Date(fault.recoveredAt), "dd/MMM/yyyy HH:mm") : "--"}
                </td>
                <td className="px-4 py-2.5">{statusBadge(fault.status)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="shrink-0 border-t bg-muted/30 px-6 py-2.5 text-xs text-muted-foreground">
        {faults.length} fault record{faults.length !== 1 ? "s" : ""}
      </div>
    </div>
  );
}
