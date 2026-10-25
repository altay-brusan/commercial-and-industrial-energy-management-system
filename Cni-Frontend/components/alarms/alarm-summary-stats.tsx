import { Badge } from "@/components/ui/badge";
import type { Alarm } from "@/lib/alarms/types";

interface AlarmSummaryStatsProps {
  alarms: Alarm[];
}

export function AlarmSummaryStats({ alarms }: AlarmSummaryStatsProps) {
  const totalActive = alarms.filter((a) => a.status === "active").length;
  const critical = alarms.filter((a) => a.severity === "critical" && a.status !== "resolved").length;
  const high = alarms.filter((a) => a.severity === "high" && a.status !== "resolved").length;
  const acknowledged = alarms.filter((a) => a.status === "acknowledged").length;

  const stats = [
    {
      label: "Total Active",
      shortLabel: "Active",
      value: totalActive,
      badge: { value: `${totalActive} open`, color: "text-foreground" },
    },
    {
      label: "Critical",
      shortLabel: "Critical",
      value: critical,
      badge: { value: "urgent", color: "text-severity-critical" },
    },
    {
      label: "High",
      shortLabel: "High",
      value: high,
      badge: { value: "warning", color: "text-severity-high" },
    },
    {
      label: "Acknowledged",
      shortLabel: "Ack'd",
      value: acknowledged,
      badge: { value: "in review", color: "text-severity-medium" },
    },
  ];

  return (
    <div className="px-6">
      <div className="grid grid-cols-4 border rounded-lg border-border">
        {stats.map((stat, index) => (
          <div
            key={stat.label}
            className={`px-2 py-2 md:px-5 md:py-4 ${
              index < stats.length - 1 ? "border-r border-border" : ""
            }`}
          >
            <p className="text-[10px] md:text-sm text-muted-foreground mb-0.5 md:mb-1 truncate">
              <span className="md:hidden">{stat.shortLabel}</span>
              <span className="hidden md:inline">{stat.label}</span>
            </p>
            <div className="flex items-center gap-1 md:gap-2">
              <span className="text-lg md:text-3xl font-semibold tracking-tight">
                {stat.value}
              </span>
              {stat.badge && (
                <Badge
                  variant="secondary"
                  className={`hidden md:inline-flex text-xs font-normal ${stat.badge.color}`}
                >
                  {stat.badge.value}
                </Badge>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
