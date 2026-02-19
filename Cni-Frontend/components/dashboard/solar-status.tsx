import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";

interface SolarData {
  currentPower: number;
  capacity: number;
  todayProduction: number;
  yesterdayProduction: number;
  remainingEstimate: number;
  performanceRatio: number;
  status: "producing" | "curtailed" | "night" | "fault";
}

const solarData: SolarData = {
  currentPower: 68.5,
  capacity: 100,
  todayProduction: 245.8,
  yesterdayProduction: 312.4,
  remainingEstimate: 89.2,
  performanceRatio: 94.2,
  status: "producing",
};

function getStatusLabel(status: SolarData["status"]): string {
  switch (status) {
    case "producing":
      return "Producing";
    case "curtailed":
      return "Curtailed";
    case "night":
      return "Night";
    case "fault":
      return "Fault";
  }
}

function getStatusColor(status: SolarData["status"]): string {
  switch (status) {
    case "producing":
      return "text-energy-solar";
    case "curtailed":
      return "text-energy-consumption";
    case "night":
      return "text-muted-foreground";
    case "fault":
      return "text-energy-grid";
  }
}

export function SolarStatus() {
  const utilizationPercent = (solarData.currentPower / solarData.capacity) * 100;
  const vsYesterdayPercent =
    ((solarData.todayProduction - solarData.yesterdayProduction) /
      solarData.yesterdayProduction) *
    100;

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Solar Production</CardTitle>
          <Badge variant="secondary" className={getStatusColor(solarData.status)}>
            {getStatusLabel(solarData.status)}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="flex items-baseline justify-between">
            <div>
              <span className="text-3xl font-semibold">
                {solarData.currentPower}
              </span>
              <span className="text-lg text-muted-foreground ml-1">kW</span>
            </div>
            <span className="text-sm text-muted-foreground">
              / {solarData.capacity} kW
            </span>
          </div>
          <Progress
            value={utilizationPercent}
            className="h-2 [&>[data-slot=progress-indicator]]:bg-energy-solar"
          />
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-muted-foreground text-xs">Today</p>
            <p className="font-semibold text-energy-solar">
              {solarData.todayProduction} kWh
            </p>
            <p
              className={`text-xs ${
                vsYesterdayPercent >= 0
                  ? "text-energy-solar"
                  : "text-energy-grid"
              }`}
            >
              {vsYesterdayPercent >= 0 ? "+" : ""}
              {vsYesterdayPercent.toFixed(1)}% vs yesterday
            </p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">Remaining Est.</p>
            <p className="font-semibold">{solarData.remainingEstimate} kWh</p>
            <p className="text-xs text-muted-foreground">until sunset</p>
          </div>
        </div>

        <div className="flex items-center justify-between text-sm border-t pt-3">
          <span className="text-muted-foreground">Performance Ratio</span>
          <span
            className={`font-medium ${
              solarData.performanceRatio >= 90
                ? "text-energy-solar"
                : solarData.performanceRatio >= 80
                ? "text-energy-consumption"
                : "text-energy-grid"
            }`}
          >
            {solarData.performanceRatio}%
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
