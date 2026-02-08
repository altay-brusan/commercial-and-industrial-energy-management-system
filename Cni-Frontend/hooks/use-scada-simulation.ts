"use client";

import { useState, useEffect, useCallback } from "react";
import type { ScadaState } from "@/lib/scada/types";
import { initialScadaState } from "@/lib/scada/mock-data";

function vary(base: number, pct: number): number {
  const change = base * pct * (Math.random() * 2 - 1);
  return Math.max(0, +(base + change).toFixed(2));
}

function clamp(val: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, val));
}

export function useScadaSimulation(intervalMs = 5000): ScadaState {
  const [state, setState] = useState<ScadaState>(() => ({
    ...initialScadaState,
    lastUpdated: new Date(),
  }));

  const tick = useCallback(() => {
    setState((prev) => {
      const nodes = prev.nodes.map((node) => {
        const n = { ...node };
        switch (n.id) {
          case "solar":
            n.realPower = vary(n.realPower, 0.03);
            break;
          case "load":
            n.realPower = vary(n.realPower, 0.02);
            n.secondaryValue = `${vary(parseFloat(n.secondaryValue || "69.2"), 0.01)} MWh`;
            break;
          case "battery": {
            n.realPower = vary(n.realPower, 0.05);
            const prevSoe = parseFloat(n.tertiaryValue || "76.6");
            const newSoe = clamp(vary(prevSoe, 0.005), 10, 100);
            n.tertiaryValue = `${newSoe.toFixed(1)}%`;
            n.secondaryValue = `${vary(parseFloat(n.secondaryValue || "8.3"), 0.01)} MWh`;
            break;
          }
          case "grid":
            n.realPower = vary(n.realPower, 0.04);
            break;
        }
        return n;
      });

      const solarNode = nodes.find((n) => n.id === "solar")!;
      const loadNode = nodes.find((n) => n.id === "load")!;
      const batteryNode = nodes.find((n) => n.id === "battery")!;
      const gridNode = nodes.find((n) => n.id === "grid")!;

      const connections = prev.connections.map((c) => {
        const conn = { ...c };
        if (c.from === "solar" && c.to === "load") {
          conn.power = +(solarNode.realPower * 0.89).toFixed(2);
        } else if (c.from === "solar" && c.to === "battery") {
          conn.power = +(solarNode.realPower * 0.06).toFixed(2);
        } else if (c.from === "grid" && c.to === "load") {
          conn.power = gridNode.realPower;
        } else if (c.from === "battery" && c.to === "load") {
          conn.power = +(loadNode.realPower - solarNode.realPower * 0.89 - gridNode.realPower).toFixed(2);
          if (conn.power < 0) conn.power = 0;
        }
        conn.direction = conn.power > 0.01 ? "forward" : "idle";
        return conn;
      });

      const metricsSections = prev.metricsSections.map((section) => {
        const s = { ...section, metrics: [...section.metrics] };
        switch (s.id) {
          case "battery":
            s.metrics = s.metrics.map((m) => {
              if (m.label === "Real Power") return { ...m, value: batteryNode.realPower.toFixed(1) };
              if (m.label === "State of Energy") return { ...m, value: (parseFloat(batteryNode.tertiaryValue || "76.6")).toFixed(1) };
              if (m.label === "Stored Energy") return { ...m, value: parseFloat(batteryNode.secondaryValue || "8.3").toFixed(1) };
              if (m.label === "Temperature") return { ...m, value: vary(32.4, 0.02).toFixed(1) };
              return m;
            });
            break;
          case "solar":
            s.metrics = s.metrics.map((m) => {
              if (m.label === "Real Power") return { ...m, value: solarNode.realPower.toFixed(1) };
              if (m.label === "Irradiance") return { ...m, value: String(Math.round(vary(842, 0.02))) };
              return m;
            });
            break;
          case "grid":
            s.metrics = s.metrics.map((m) => {
              if (m.label === "Import Power") return { ...m, value: gridNode.realPower.toFixed(1) };
              if (m.label === "Frequency") return { ...m, value: vary(50.01, 0.001).toFixed(2) };
              return m;
            });
            break;
          case "load":
            s.metrics = s.metrics.map((m) => {
              if (m.label === "Real Power") return { ...m, value: loadNode.realPower.toFixed(1) };
              if (m.label === "Energy Today") return { ...m, value: parseFloat(loadNode.secondaryValue || "69.2").toFixed(1) };
              return m;
            });
            break;
        }
        return s;
      });

      return {
        nodes,
        connections,
        metricsSections,
        lastUpdated: new Date(),
      };
    });
  }, []);

  useEffect(() => {
    const id = setInterval(tick, intervalMs);
    return () => clearInterval(id);
  }, [tick, intervalMs]);

  return state;
}
