import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { AbbrTooltip } from "@/components/ui/abbr-tooltip";

type GridQuality = "excellent" | "good" | "fair" | "poor";
type ConnectionStatus = "connected" | "islanded" | "fault";

interface GridData {
  connectionStatus: ConnectionStatus;
  quality: GridQuality;
  importPower: number;
  exportPower: number;
  frequency: number;
  voltage: number;
  powerFactor: number;
  tariff: {
    current: string;
    rate: number;
    nextChange: string;
  };
}

const gridData: GridData = {
  connectionStatus: "connected",
  quality: "good",
  importPower: 0,
  exportPower: 23.4,
  frequency: 50.02,
  voltage: 238,
  powerFactor: 0.97,
  tariff: {
    current: "Off-Peak",
    rate: 0.08,
    nextChange: "14:00",
  },
};

function getConnectionLabel(status: ConnectionStatus): string {
  switch (status) {
    case "connected":
      return "Connected";
    case "islanded":
      return "Islanded";
    case "fault":
      return "Fault";
  }
}

function getConnectionColor(status: ConnectionStatus): string {
  switch (status) {
    case "connected":
      return "text-energy-solar";
    case "islanded":
      return "text-energy-consumption";
    case "fault":
      return "text-energy-grid";
  }
}

function getQualityColor(quality: GridQuality): string {
  switch (quality) {
    case "excellent":
    case "good":
      return "text-energy-solar";
    case "fair":
      return "text-energy-consumption";
    case "poor":
      return "text-energy-grid";
  }
}

export function GridStatus() {
  const isExporting = gridData.exportPower > 0;
  const powerFlow = isExporting ? gridData.exportPower : gridData.importPower;
  const flowDirection = isExporting ? "Export" : "Import";

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Grid Connection</CardTitle>
          <Badge
            variant="secondary"
            className={getConnectionColor(gridData.connectionStatus)}
          >
            {getConnectionLabel(gridData.connectionStatus)}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1">
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted-foreground">Quality</span>
            <span className={`font-medium ${getQualityColor(gridData.quality)}`}>
              {gridData.quality.charAt(0).toUpperCase() + gridData.quality.slice(1)}
            </span>
          </div>
          <div className="flex items-baseline justify-between">
            <div>
              <span className="text-3xl font-semibold">{powerFlow}</span>
              <span className="text-lg text-muted-foreground ml-1">kW</span>
            </div>
            <Badge
              variant="outline"
              className={isExporting ? "text-energy-solar" : "text-energy-grid"}
            >
              {flowDirection}
            </Badge>
          </div>
        </div>

        <Separator />

        <div className="grid grid-cols-3 gap-3 text-sm">
          <div>
            <p className="text-muted-foreground text-xs">Frequency</p>
            <p className="font-medium">{gridData.frequency} Hz</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">Voltage</p>
            <p className="font-medium">{gridData.voltage} V</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs"><AbbrTooltip label="PF" title="Power Factor" description="Ratio of real power to apparent power, indicating electrical efficiency" /></p>
            <p className="font-medium">{gridData.powerFactor}</p>
          </div>
        </div>

        <Separator />

        <div className="space-y-1 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">Current Tariff</span>
            <Badge variant="secondary">{gridData.tariff.current}</Badge>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">Rate</span>
            <span className="font-medium">
              ${gridData.tariff.rate.toFixed(2)}/kWh
            </span>
          </div>
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Next change</span>
            <span>{gridData.tariff.nextChange}</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
