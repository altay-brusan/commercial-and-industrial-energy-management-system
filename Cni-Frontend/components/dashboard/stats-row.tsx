import { Badge } from "@/components/ui/badge";

interface StatItem {
  label: string;
  value: string;
  unit?: string;
  change?: {
    value: string;
    trend: "up" | "down" | "neutral";
  };
}

const stats: StatItem[] = [
  {
    label: "Total Consumption",
    value: "2,847",
    unit: "kWh",
    change: { value: "+12%", trend: "up" },
  },
  {
    label: "Current Power",
    value: "4.2",
    unit: "kW",
    change: { value: "-3%", trend: "down" },
  },
  {
    label: "Daily Cost",
    value: "$127.50",
    change: { value: "+5%", trend: "up" },
  },
  {
    label: "Peak Demand",
    value: "8.7",
    unit: "kW",
    change: { value: "same", trend: "neutral" },
  },
];

export function StatsRow() {
  return (
    <div className="grid grid-cols-2 md:grid-cols-4 border-b border-border">
      {stats.map((stat, index) => (
        <div
          key={stat.label}
          className={`px-6 py-5 ${
            index < stats.length - 1 ? "border-r border-border" : ""
          }`}
        >
          <p className="text-sm text-muted-foreground mb-1">{stat.label}</p>
          <div className="flex items-baseline gap-2">
            <span className="text-3xl font-semibold tracking-tight">
              {stat.value}
            </span>
            {stat.unit && (
              <span className="text-sm text-muted-foreground">{stat.unit}</span>
            )}
          </div>
          {stat.change && (
            <Badge
              variant="secondary"
              className={`mt-2 text-xs font-normal ${
                stat.change.trend === "up"
                  ? "text-energy-grid"
                  : stat.change.trend === "down"
                  ? "text-energy-solar"
                  : "text-muted-foreground"
              }`}
            >
              {stat.change.value}
            </Badge>
          )}
        </div>
      ))}
    </div>
  );
}
