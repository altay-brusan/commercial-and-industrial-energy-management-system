import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

type SystemStatus = "operational" | "warning" | "critical" | "offline";

interface Subsystem {
  name: string;
  status: SystemStatus;
  details?: string;
}

interface SystemHealthData {
  overallStatus: SystemStatus;
  healthScore: number;
  uptime: string;
  lastUpdate: string;
  subsystems: Subsystem[];
}

const systemData: SystemHealthData = {
  overallStatus: "operational",
  healthScore: 96,
  uptime: "45d 12h 32m",
  lastUpdate: "2 sec ago",
  subsystems: [
    { name: "BESS", status: "operational", details: "All units online" },
    { name: "Solar", status: "operational", details: "Normal production" },
    { name: "Grid", status: "warning", details: "Voltage fluctuation" },
    { name: "PCS", status: "operational", details: "Running at 98%" },
    { name: "Comms", status: "operational", details: "All connections active" },
  ],
};

function getStatusColor(status: SystemStatus): string {
  switch (status) {
    case "operational":
      return "bg-status-operational";
    case "warning":
      return "bg-severity-high";
    case "critical":
      return "bg-severity-critical";
    case "offline":
      return "bg-muted-foreground";
  }
}

function getStatusTextColor(status: SystemStatus): string {
  switch (status) {
    case "operational":
      return "text-status-operational";
    case "warning":
      return "text-severity-high";
    case "critical":
      return "text-severity-critical";
    case "offline":
      return "text-muted-foreground";
  }
}

function getStatusLabel(status: SystemStatus): string {
  switch (status) {
    case "operational":
      return "Operational";
    case "warning":
      return "Warning";
    case "critical":
      return "Critical";
    case "offline":
      return "Offline";
  }
}

export function SystemHealth() {
  const hasIssues = systemData.subsystems.some(
    (s) => s.status !== "operational"
  );

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">System Health</CardTitle>
          <Badge
            variant="secondary"
            className={getStatusTextColor(systemData.overallStatus)}
          >
            {getStatusLabel(systemData.overallStatus)}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Health Score</span>
            <span className="font-semibold">{systemData.healthScore}%</span>
          </div>
          <Progress
            value={systemData.healthScore}
            className={`h-2 ${
              systemData.healthScore >= 90
                ? "[&>[data-slot=progress-indicator]]:bg-status-operational"
                : systemData.healthScore >= 70
                ? "[&>[data-slot=progress-indicator]]:bg-severity-high"
                : "[&>[data-slot=progress-indicator]]:bg-severity-critical"
            }`}
          />
        </div>

        <div className="space-y-2">
          <p className="text-xs text-muted-foreground">Subsystems</p>
          <TooltipProvider>
            <div className="flex gap-2 flex-wrap">
              {systemData.subsystems.map((subsystem) => (
                <Tooltip key={subsystem.name}>
                  <TooltipTrigger asChild>
                    <div className="flex items-center gap-1.5 px-2 py-1 rounded-md bg-secondary/50 cursor-default">
                      <div
                        className={`w-2 h-2 rounded-full ${getStatusColor(
                          subsystem.status
                        )}`}
                      />
                      <span className="text-xs font-medium">
                        {subsystem.name}
                      </span>
                    </div>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p className="font-medium">{subsystem.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {subsystem.details}
                    </p>
                  </TooltipContent>
                </Tooltip>
              ))}
            </div>
          </TooltipProvider>
        </div>

        <div className="flex items-center justify-between text-xs text-muted-foreground border-t pt-3">
          <span>Uptime: {systemData.uptime}</span>
          <span>Updated: {systemData.lastUpdate}</span>
        </div>

        {hasIssues && (
          <div className="text-xs text-severity-high bg-severity-high/10 rounded-md p-2">
            1 subsystem requires attention
          </div>
        )}
      </CardContent>
    </Card>
  );
}
