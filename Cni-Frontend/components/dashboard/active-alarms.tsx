"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";

type AlarmPriority = "critical" | "high" | "medium" | "low";

interface Alarm {
  id: string;
  priority: AlarmPriority;
  message: string;
  source: string;
  timestamp: string;
  acknowledged: boolean;
}

const alarms: Alarm[] = [
  {
    id: "ALM-001",
    priority: "critical",
    message: "Grid voltage exceeded upper limit",
    source: "Grid",
    timestamp: "12:45:32",
    acknowledged: false,
  },
  {
    id: "ALM-002",
    priority: "high",
    message: "BESS temperature warning",
    source: "BESS",
    timestamp: "12:42:18",
    acknowledged: false,
  },
  {
    id: "ALM-003",
    priority: "medium",
    message: "Solar inverter communication timeout",
    source: "Solar",
    timestamp: "12:38:05",
    acknowledged: true,
  },
  {
    id: "ALM-004",
    priority: "low",
    message: "Scheduled maintenance reminder",
    source: "System",
    timestamp: "12:30:00",
    acknowledged: true,
  },
  {
    id: "ALM-005",
    priority: "medium",
    message: "Power factor below threshold",
    source: "Grid",
    timestamp: "12:25:41",
    acknowledged: false,
  },
];

function getPriorityColor(priority: AlarmPriority): string {
  switch (priority) {
    case "critical":
      return "bg-energy-grid text-white";
    case "high":
      return "bg-energy-consumption text-white";
    case "medium":
      return "bg-energy-battery text-white";
    case "low":
      return "bg-muted text-muted-foreground";
  }
}

function getPriorityLabel(priority: AlarmPriority): string {
  switch (priority) {
    case "critical":
      return "CRIT";
    case "high":
      return "HIGH";
    case "medium":
      return "MED";
    case "low":
      return "LOW";
  }
}

export function ActiveAlarms() {
  const alarmCounts = {
    critical: alarms.filter((a) => a.priority === "critical" && !a.acknowledged)
      .length,
    high: alarms.filter((a) => a.priority === "high" && !a.acknowledged).length,
    medium: alarms.filter((a) => a.priority === "medium" && !a.acknowledged)
      .length,
    low: alarms.filter((a) => a.priority === "low" && !a.acknowledged).length,
  };

  const totalUnacknowledged = Object.values(alarmCounts).reduce(
    (a, b) => a + b,
    0
  );

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Active Alarms</CardTitle>
          <div className="flex items-center gap-2">
            {alarmCounts.critical > 0 && (
              <Badge className="bg-energy-grid text-white">
                {alarmCounts.critical} Critical
              </Badge>
            )}
            {totalUnacknowledged > 0 && (
              <Badge variant="outline">{totalUnacknowledged} Unack</Badge>
            )}
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <ScrollArea className="h-[280px] pr-4">
          <div className="space-y-2">
            {alarms.map((alarm) => (
              <div
                key={alarm.id}
                className={`flex items-start gap-3 p-3 rounded-lg border ${
                  alarm.acknowledged
                    ? "bg-muted/30 opacity-60"
                    : "bg-background"
                }`}
              >
                <Badge
                  className={`${getPriorityColor(alarm.priority)} text-[10px] px-1.5`}
                >
                  {getPriorityLabel(alarm.priority)}
                </Badge>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{alarm.message}</p>
                  <div className="flex items-center gap-2 mt-1 text-xs text-muted-foreground">
                    <span>{alarm.source}</span>
                    <span>|</span>
                    <span>{alarm.timestamp}</span>
                    <span>|</span>
                    <span>{alarm.id}</span>
                  </div>
                </div>
                {!alarm.acknowledged && (
                  <Button
                    variant="outline"
                    size="xs"
                    className="shrink-0"
                    onClick={() => console.log(`ACK: ${alarm.id}`)}
                  >
                    ACK
                  </Button>
                )}
              </div>
            ))}
          </div>
        </ScrollArea>
        <div className="flex items-center justify-between mt-4 pt-3 border-t text-xs text-muted-foreground">
          <span>Showing {alarms.length} alarms</span>
          <Button variant="link" size="sm" className="h-auto p-0 text-xs">
            View All
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
