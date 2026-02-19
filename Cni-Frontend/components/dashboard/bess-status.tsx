import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Separator } from "@/components/ui/separator";

type BESSMode = "charging" | "discharging" | "idle" | "standby";

interface BESSData {
  soc: number;
  mode: BESSMode;
  power: number;
  capacity: number;
  temperature: number;
  voltage: number;
  current: number;
  cycleCount: number;
  todayCharged: number;
  todayDischarged: number;
}

const bessData: BESSData = {
  soc: 72,
  mode: "discharging",
  power: 45.2,
  capacity: 100,
  temperature: 28,
  voltage: 408,
  current: 110.8,
  cycleCount: 1247,
  todayCharged: 38.5,
  todayDischarged: 52.3,
};

function getModeLabel(mode: BESSMode): string {
  switch (mode) {
    case "charging":
      return "Charging";
    case "discharging":
      return "Discharging";
    case "idle":
      return "Idle";
    case "standby":
      return "Standby";
  }
}

function getModeColor(mode: BESSMode): string {
  switch (mode) {
    case "charging":
      return "text-energy-solar";
    case "discharging":
      return "text-energy-battery";
    case "idle":
      return "text-muted-foreground";
    case "standby":
      return "text-energy-consumption";
  }
}

function getSOCColor(soc: number): string {
  if (soc >= 60) return "[&>[data-slot=progress-indicator]]:bg-energy-solar";
  if (soc >= 30)
    return "[&>[data-slot=progress-indicator]]:bg-energy-consumption";
  return "[&>[data-slot=progress-indicator]]:bg-energy-grid";
}

export function BESSStatus() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">BESS Status</CardTitle>
          <Badge variant="secondary" className={getModeColor(bessData.mode)}>
            {getModeLabel(bessData.mode)}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-baseline justify-between">
            <span className="text-3xl font-semibold">{bessData.soc}%</span>
            <span className="text-sm text-muted-foreground">
              {bessData.power} kW
            </span>
          </div>
          <Progress value={bessData.soc} className={`h-3 ${getSOCColor(bessData.soc)}`} />
          <div className="flex justify-between text-xs text-muted-foreground">
            <span>0%</span>
            <span>State of Charge</span>
            <span>100%</span>
          </div>
        </div>

        <Separator />

        <div className="grid grid-cols-3 gap-3 text-sm">
          <div>
            <p className="text-muted-foreground text-xs">Temp</p>
            <p className="font-medium">{bessData.temperature}°C</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">Voltage</p>
            <p className="font-medium">{bessData.voltage}V</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">Current</p>
            <p className="font-medium">{bessData.current}A</p>
          </div>
        </div>

        <Separator />

        <div className="space-y-2">
          <p className="text-xs text-muted-foreground">Today</p>
          <div className="grid grid-cols-2 gap-3 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Charged</span>
              <span className="font-medium text-energy-solar">
                +{bessData.todayCharged} kWh
              </span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Discharged</span>
              <span className="font-medium text-energy-battery">
                -{bessData.todayDischarged} kWh
              </span>
            </div>
          </div>
        </div>

        <div className="text-xs text-muted-foreground text-right">
          Cycle count: {bessData.cycleCount.toLocaleString("en-US")}
        </div>
      </CardContent>
    </Card>
  );
}
