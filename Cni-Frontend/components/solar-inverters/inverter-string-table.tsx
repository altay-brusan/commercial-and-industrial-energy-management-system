"use client";

import React, { useState } from "react";
import { ChevronDown } from "lucide-react";
import { Area, AreaChart, ResponsiveContainer } from "recharts";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";

interface InverterStringTableProps {
  stringCurrents: (number | null)[];
  stringVoltages: (number | null)[];
}

function getStringStatus(current: number | null): {
  label: string;
  className: string;
} {
  if (current === null)
    return {
      label: "--",
      className: "bg-muted text-muted-foreground",
    };
  if (current === 0)
    return {
      label: "Disconnected",
      className:
        "bg-severity-critical/15 text-severity-critical border-severity-critical/30",
    };
  if (current < 0.5)
    return {
      label: "Low",
      className:
        "bg-severity-high/15 text-severity-high border-severity-high/30",
    };
  return {
    label: "Normal",
    className:
      "bg-status-operational/15 text-status-operational border-status-operational/30",
  };
}

// Generate simple sparkline mock data (24h pattern)
function generateSparklineData(baseCurrent: number | null) {
  if (baseCurrent === null || baseCurrent === 0) return [];
  const points = [];
  for (let i = 0; i < 24; i++) {
    const normalizedH = (i - 12) / 5;
    const solarFactor = Math.max(
      0,
      Math.exp(-0.5 * normalizedH * normalizedH)
    );
    points.push({
      h: i,
      value: +(baseCurrent * solarFactor * (0.9 + Math.random() * 0.2)).toFixed(
        2
      ),
    });
  }
  return points;
}

export function InverterStringTable({
  stringCurrents,
  stringVoltages,
}: InverterStringTableProps) {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const toggleExpand = (index: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(index)) {
        next.delete(index);
      } else {
        next.add(index);
      }
      return next;
    });
  };

  return (
    <div className="border rounded-md overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="hover:bg-transparent">
            <TableHead className="bg-muted w-[40px]" />
            <TableHead className="bg-muted w-[100px]">String</TableHead>
            <TableHead className="bg-muted w-[120px]">Current (A)</TableHead>
            <TableHead className="bg-muted w-[120px]">Voltage (V)</TableHead>
            <TableHead className="bg-muted w-[120px]">Power (W)</TableHead>
            <TableHead className="bg-muted">Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {stringCurrents.map((current, index) => {
            const voltage = stringVoltages[index] ?? null;
            const power =
              current !== null && voltage !== null
                ? +(current * voltage).toFixed(1)
                : null;
            const status = getStringStatus(current);
            const isExpanded = expanded.has(index);
            const sparklineData = generateSparklineData(current);

            // Calculate expanded detail stats
            const avg =
              current !== null ? +(current * 0.65).toFixed(2) : null;
            const min = current !== null ? +(current * 0.1).toFixed(2) : null;
            const max = current !== null ? +(current * 1.05).toFixed(2) : null;
            const panelCount = current !== null && current > 0 ? 18 : 0;

            return (
              <React.Fragment key={index}>
                <TableRow
                  className="cursor-pointer"
                  onClick={() => toggleExpand(index)}
                >
                  <TableCell className="px-2">
                    <ChevronDown
                      className={`h-4 w-4 text-muted-foreground transition-transform ${
                        isExpanded ? "rotate-0" : "-rotate-90"
                      }`}
                    />
                  </TableCell>
                  <TableCell className="font-medium">
                    String {index + 1}
                  </TableCell>
                  <TableCell>
                    {current !== null ? current.toFixed(2) : "--"}
                  </TableCell>
                  <TableCell>
                    {voltage !== null ? voltage.toFixed(1) : "--"}
                  </TableCell>
                  <TableCell>
                    {power !== null ? power.toFixed(1) : "--"}
                  </TableCell>
                  <TableCell>
                    <Badge className={status.className}>
                      {status.label}
                    </Badge>
                  </TableCell>
                </TableRow>
                {isExpanded && (
                  <TableRow className="bg-muted/30 hover:bg-muted/30">
                    <TableCell colSpan={6} className="p-4">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-end">
                        <div className="space-y-2">
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-muted-foreground">
                              Connected panels
                            </span>
                            <span className="font-medium">{panelCount}</span>
                          </div>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-muted-foreground">
                              24h Avg current
                            </span>
                            <span className="font-medium">
                              {avg !== null ? `${avg} A` : "--"}
                            </span>
                          </div>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-muted-foreground">
                              24h Min current
                            </span>
                            <span className="font-medium">
                              {min !== null ? `${min} A` : "--"}
                            </span>
                          </div>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-muted-foreground">
                              24h Max current
                            </span>
                            <span className="font-medium">
                              {max !== null ? `${max} A` : "--"}
                            </span>
                          </div>
                        </div>
                        {sparklineData.length > 0 && (
                          <div className="h-[80px]">
                            <ResponsiveContainer width="100%" height="100%">
                              <AreaChart data={sparklineData}>
                                <Area
                                  type="monotone"
                                  dataKey="value"
                                  stroke="var(--color-energy-solar)"
                                  fill="var(--color-energy-solar)"
                                  fillOpacity={0.2}
                                  strokeWidth={1.5}
                                />
                              </AreaChart>
                            </ResponsiveContainer>
                          </div>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                )}
              </React.Fragment>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
