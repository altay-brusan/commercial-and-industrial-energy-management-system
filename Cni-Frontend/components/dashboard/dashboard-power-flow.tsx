"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Sun, Battery, Zap, Home } from "lucide-react";
import { generatePowerFlowData } from "@/lib/dashboard/mock-data";
import type { PowerFlowData, DetailPanelState } from "@/lib/dashboard/types";

interface DashboardPowerFlowProps {
  onPanelChange: (state: DetailPanelState) => void;
}

export function DashboardPowerFlow({ onPanelChange }: DashboardPowerFlowProps) {
  const [data, setData] = useState<PowerFlowData>(() => generatePowerFlowData());

  useEffect(() => {
    const interval = setInterval(() => {
      setData(generatePowerFlowData());
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  const nodeSize = 80;
  const svgWidth = 600;
  const svgHeight = 320;

  // Node positions
  const solar = { x: svgWidth / 2, y: 30 };
  const bus = { x: svgWidth / 2, y: svgHeight / 2 };
  const battery = { x: 60, y: svgHeight / 2 };
  const grid = { x: svgWidth - 60, y: svgHeight / 2 };
  const load = { x: svgWidth / 2, y: svgHeight - 30 };

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base">Power Flow</CardTitle>
        <p className="text-sm text-muted-foreground">Real-time energy distribution</p>
      </CardHeader>
      <CardContent className="flex items-center justify-center p-4">
        <svg viewBox={`0 0 ${svgWidth} ${svgHeight}`} className="w-full max-w-[500px] h-auto">
          {/* Flow Lines */}
          {/* Solar to Bus */}
          {data.solarPower > 0 && (
            <line
              x1={solar.x} y1={solar.y + nodeSize / 2 - 10}
              x2={bus.x} y2={bus.y - 16}
              className="stroke-energy-solar"
              strokeWidth="2"
              strokeDasharray="6 6"
              style={{ animation: "flow 1s linear infinite" }}
            />
          )}

          {/* Battery to/from Bus */}
          {data.batteryPower !== 0 && (
            <line
              x1={battery.x + nodeSize / 2 - 10} y1={battery.y}
              x2={bus.x - 16} y2={bus.y}
              className={data.batteryPower > 0 ? "stroke-energy-battery" : "stroke-energy-battery"}
              strokeWidth="2"
              strokeDasharray="6 6"
              style={{
                animation: "flow 1s linear infinite",
                animationDirection: data.batteryPower < 0 ? "reverse" : "normal",
              }}
            />
          )}

          {/* Grid to/from Bus */}
          {data.gridPower !== 0 && (
            <line
              x1={bus.x + 16} y1={bus.y}
              x2={grid.x - nodeSize / 2 + 10} y2={grid.y}
              className="stroke-energy-grid"
              strokeWidth="2"
              strokeDasharray="6 6"
              style={{
                animation: "flow 1s linear infinite",
                animationDirection: data.gridPower < 0 ? "reverse" : "normal",
              }}
            />
          )}

          {/* Bus to Load */}
          <line
            x1={bus.x} y1={bus.y + 16}
            x2={load.x} y2={load.y - nodeSize / 2 + 10}
            className="stroke-muted-foreground"
            strokeWidth="2"
            strokeDasharray="6 6"
            style={{ animation: "flow 1s linear infinite" }}
          />

          {/* Bus Node (center) */}
          <circle cx={bus.x} cy={bus.y} r="14" className="fill-muted stroke-border" strokeWidth="1.5" />
          <text x={bus.x} y={bus.y + 4} textAnchor="middle" className="fill-muted-foreground text-[10px] font-medium">
            BUS
          </text>

          {/* Solar Node */}
          <g
            className="cursor-pointer"
            onClick={() => onPanelChange({ type: "solar" })}
          >
            <rect
              x={solar.x - nodeSize / 2} y={solar.y - nodeSize / 2 + 10}
              width={nodeSize} height={nodeSize - 20}
              rx="12"
              className="fill-energy-solar/10 stroke-energy-solar/30 hover:fill-energy-solar/20 transition-colors"
              strokeWidth="1.5"
            />
            <foreignObject x={solar.x - 12} y={solar.y - 22} width="24" height="24">
              <Sun className="h-5 w-5 text-energy-solar" />
            </foreignObject>
            <text x={solar.x} y={solar.y + 8} textAnchor="middle" className="fill-foreground text-[13px] font-semibold">
              {data.solarPower} kW
            </text>
          </g>

          {/* Battery Node */}
          <g
            className="cursor-pointer"
            onClick={() => onPanelChange({ type: "battery" })}
          >
            <rect
              x={battery.x - nodeSize / 2 + 10} y={battery.y - nodeSize / 2 + 10}
              width={nodeSize - 20} height={nodeSize - 20}
              rx="12"
              className="fill-energy-battery/10 stroke-energy-battery/30 hover:fill-energy-battery/20 transition-colors"
              strokeWidth="1.5"
            />
            <foreignObject x={battery.x - 2} y={battery.y - 22} width="24" height="24">
              <Battery className="h-5 w-5 text-energy-battery" />
            </foreignObject>
            <text x={battery.x + 10} y={battery.y + 6} textAnchor="middle" className="fill-foreground text-[12px] font-semibold">
              {Math.abs(data.batteryPower)} kW
            </text>
            <text x={battery.x + 10} y={battery.y + 20} textAnchor="middle" className="fill-muted-foreground text-[10px]">
              {data.batterySoc}%
            </text>
          </g>

          {/* Grid Node */}
          <g
            className="cursor-pointer"
            onClick={() => onPanelChange({ type: "grid" })}
          >
            <rect
              x={grid.x - nodeSize / 2 + 10} y={grid.y - nodeSize / 2 + 10}
              width={nodeSize - 20} height={nodeSize - 20}
              rx="12"
              className="fill-energy-grid/10 stroke-energy-grid/30 hover:fill-energy-grid/20 transition-colors"
              strokeWidth="1.5"
            />
            <foreignObject x={grid.x - 2} y={grid.y - 22} width="24" height="24">
              <Zap className="h-5 w-5 text-energy-grid" />
            </foreignObject>
            <text x={grid.x + 10} y={grid.y + 6} textAnchor="middle" className="fill-foreground text-[12px] font-semibold">
              {Math.abs(data.gridPower)} kW
            </text>
            <text x={grid.x + 10} y={grid.y + 20} textAnchor="middle" className="fill-muted-foreground text-[10px]">
              {data.gridPower >= 0 ? "import" : "export"}
            </text>
          </g>

          {/* Load Node */}
          <g>
            <rect
              x={load.x - nodeSize / 2} y={load.y - nodeSize / 2 + 10}
              width={nodeSize} height={nodeSize - 20}
              rx="12"
              className="fill-muted stroke-border"
              strokeWidth="1.5"
            />
            <foreignObject x={load.x - 12} y={load.y - 22} width="24" height="24">
              <Home className="h-5 w-5 text-muted-foreground" />
            </foreignObject>
            <text x={load.x} y={load.y + 8} textAnchor="middle" className="fill-foreground text-[13px] font-semibold">
              {data.loadPower} kW
            </text>
          </g>
        </svg>
      </CardContent>
    </Card>
  );
}
