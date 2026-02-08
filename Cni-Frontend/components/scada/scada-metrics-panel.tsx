"use client";

import { useEffect, useRef } from "react";
import type { ScadaMetricsSection, ScadaNodeId } from "@/lib/scada/types";
import { format } from "date-fns";

interface ScadaMetricsPanelProps {
  metricsSections: ScadaMetricsSection[];
  selectedNode: ScadaNodeId | null;
  lastUpdated: Date;
}

export function ScadaMetricsPanel({
  metricsSections,
  selectedNode,
  lastUpdated,
}: ScadaMetricsPanelProps) {
  const sectionRefs = useRef<Record<string, HTMLDivElement | null>>({});

  useEffect(() => {
    if (selectedNode && sectionRefs.current[selectedNode]) {
      sectionRefs.current[selectedNode]?.scrollIntoView({
        behavior: "smooth",
        block: "start",
      });
    }
  }, [selectedNode]);

  return (
    <aside className="w-[420px] shrink-0 border-l border-border bg-background overflow-y-auto">
      {/* Header */}
      <div className="sticky top-0 bg-background border-b border-border px-6 py-4 z-10">
        <h2 className="text-lg font-semibold">System Metrics</h2>
        <p className="text-xs text-muted-foreground mt-1" suppressHydrationWarning>
          Last updated: {lastUpdated ? format(lastUpdated, "HH:mm:ss") : "—"}
        </p>
      </div>

      {/* Sections */}
      <div className="p-6">
        {metricsSections.map((section) => {
          const isSelected = selectedNode === section.id;
          return (
            <div
              key={section.id}
              ref={(el) => { sectionRefs.current[section.id] = el; }}
              className={`mb-6 rounded-lg transition-all ${isSelected ? "p-3 -mx-3" : ""}`}
              style={isSelected ? { outline: `1.5px solid var(--${section.color})`, outlineOffset: "2px" } : undefined}
            >
              <h3
                className="text-sm font-semibold mb-3 pb-2 border-b border-border"
                style={{ color: `var(--${section.color})` }}
              >
                {section.title}
              </h3>
              <div className="space-y-2">
                {section.metrics.map((metric) => (
                  <div
                    key={metric.label}
                    className="flex justify-between items-baseline"
                  >
                    <span className="text-sm text-muted-foreground">
                      {metric.label}
                    </span>
                    <span className="text-sm font-medium text-foreground">
                      {metric.value}
                      {metric.unit ? (
                        <span className="text-xs text-muted-foreground ml-1">
                          {metric.unit}
                        </span>
                      ) : null}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </aside>
  );
}
