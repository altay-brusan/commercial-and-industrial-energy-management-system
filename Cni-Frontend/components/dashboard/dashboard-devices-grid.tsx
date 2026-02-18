"use client";

import { formatDistanceToNow } from "date-fns";
import { Sun, Battery, Zap, Gauge, CloudSun } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { devices } from "@/lib/dashboard/mock-data";
import { cn } from "@/lib/utils";
import type { DetailPanelState, DashboardDevice } from "@/lib/dashboard/types";

interface DashboardDevicesGridProps {
  onPanelChange: (state: DetailPanelState) => void;
}

function getDeviceIcon(type: DashboardDevice["type"]) {
  switch (type) {
    case "solar_inverter":
      return Sun;
    case "battery":
      return Battery;
    case "grid_meter":
      return Zap;
    case "load_meter":
      return Gauge;
    case "weather_station":
      return CloudSun;
    default:
      return Gauge;
  }
}

function getStatusColor(status: DashboardDevice["status"]) {
  switch (status) {
    case "online":
      return "bg-energy-solar/15 text-energy-solar border-energy-solar/30";
    case "warning":
      return "bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30";
    case "error":
      return "bg-energy-grid/15 text-energy-grid border-energy-grid/30";
    case "offline":
      return "bg-muted text-muted-foreground border-border";
  }
}

function getDeviceTypeLabel(type: DashboardDevice["type"]) {
  switch (type) {
    case "solar_inverter":
      return "Solar Inverter";
    case "battery":
      return "Battery";
    case "grid_meter":
      return "Grid Meter";
    case "load_meter":
      return "Load Meter";
    case "weather_station":
      return "Weather Station";
  }
}

export function DashboardDevicesGrid({ onPanelChange }: DashboardDevicesGridProps) {
  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-base font-semibold">Devices</h2>
        <span className="text-sm text-muted-foreground">{devices.length} total</span>
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {devices.map((device) => {
          const Icon = getDeviceIcon(device.type);
          return (
            <Card
              key={device.id}
              className="cursor-pointer hover:ring-1 hover:ring-border hover:bg-muted/50 transition-all"
              onClick={() => onPanelChange({ type: "device", deviceId: device.id })}
            >
              <CardContent className="p-4">
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-2.5">
                    <div className="rounded-md bg-muted p-2">
                      <Icon className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div>
                      <p className="text-sm font-medium leading-tight">{device.name}</p>
                      <p className="text-xs text-muted-foreground">{getDeviceTypeLabel(device.type)}</p>
                    </div>
                  </div>
                  <Badge variant="outline" className={cn("text-[10px] capitalize", getStatusColor(device.status))}>
                    {device.status}
                  </Badge>
                </div>
                <div className="space-y-1 text-xs text-muted-foreground">
                  <p>{device.manufacturer} {device.model}</p>
                  <p>Last seen {formatDistanceToNow(device.lastSeen, { addSuffix: true })}</p>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
