"use client";

interface PowerSource {
  id: string;
  name: string;
  value: number;
  unit: string;
  status: "generating" | "consuming" | "idle";
  color: string;
  hasDetail: boolean;
}

const powerSources: PowerSource[] = [
  {
    id: "solar",
    name: "Solar Panels",
    value: 4.2,
    unit: "kW",
    status: "generating",
    color: "bg-energy-solar",
    hasDetail: true,
  },
  {
    id: "grid",
    name: "Grid Import",
    value: 1.8,
    unit: "kW",
    status: "consuming",
    color: "bg-energy-grid",
    hasDetail: false,
  },
  {
    id: "battery",
    name: "Battery",
    value: 0.5,
    unit: "kW",
    status: "generating",
    color: "bg-energy-battery",
    hasDetail: false,
  },
  {
    id: "load",
    name: "Home Load",
    value: 6.5,
    unit: "kW",
    status: "consuming",
    color: "bg-foreground",
    hasDetail: false,
  },
];

interface PowerFlowProps {
  onSourceClick?: (sourceId: string) => void;
}

export function PowerFlow({ onSourceClick }: PowerFlowProps) {
  return (
    <div className="px-6 py-6 border-t border-border">
      <div className="mb-4">
        <h2 className="text-lg font-semibold">Live Power Flow</h2>
        <p className="text-sm text-muted-foreground">
          Current power distribution
        </p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {powerSources.map((source) => (
          <div
            key={source.name}
            onClick={() => source.hasDetail && onSourceClick?.(source.id)}
            className={`flex flex-col items-center p-4 rounded-lg bg-muted/30 transition-colors ${
              source.hasDetail
                ? "cursor-pointer hover:bg-muted/50 hover:ring-1 hover:ring-border"
                : ""
            }`}
          >
            <div className={`w-3 h-3 rounded-full ${source.color} mb-3`} />
            <span className="text-2xl font-semibold">
              {source.value}
              <span className="text-sm font-normal text-muted-foreground ml-1">
                {source.unit}
              </span>
            </span>
            <span className="text-sm text-muted-foreground mt-1">
              {source.name}
            </span>
            <span
              className={`text-xs mt-2 ${
                source.status === "generating"
                  ? "text-energy-solar"
                  : source.status === "consuming"
                  ? "text-energy-grid"
                  : "text-muted-foreground"
              }`}
            >
              {source.status}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
