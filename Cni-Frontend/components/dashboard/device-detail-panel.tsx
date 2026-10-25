"use client";

import { formatDistanceToNow, format } from "date-fns";
import { X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { getDeviceById } from "@/lib/dashboard/mock-data";
import { cn } from "@/lib/utils";
import type { DashboardDevice } from "@/lib/dashboard/types";

interface DeviceDetailPanelProps {
  deviceId: string;
  onClose: () => void;
}

interface InfoItem {
  label: string;
  value: string;
}

function InfoSection({ title, items }: { title: string; items: InfoItem[] }) {
  return (
    <div>
      <h3 className="text-sm font-semibold text-foreground mb-3 pb-2 border-b border-border">
        {title}
      </h3>
      <div className="space-y-2">
        {items.map((item) => (
          <div key={item.label} className="flex justify-between items-baseline">
            <span className="text-sm text-muted-foreground">{item.label}</span>
            <span className="text-sm font-medium text-foreground">{item.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
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
      return "Battery Controller";
    case "grid_meter":
      return "Grid Meter";
    case "load_meter":
      return "Load Meter";
    case "weather_station":
      return "Weather Station";
  }
}

export function DeviceDetailPanel({ deviceId, onClose }: DeviceDetailPanelProps) {
  const device = getDeviceById(deviceId);

  if (!device) {
    return (
      <aside className="w-[420px] shrink-0 border-l border-border flex flex-col bg-background overflow-hidden">
        <div className="shrink-0 flex items-center justify-between px-5 py-4 border-b border-border bg-background">
          <h2 className="text-lg font-semibold">Device Not Found</h2>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>
      </aside>
    );
  }

  const specItems: InfoItem[] = Object.entries(device.specifications).map(
    ([label, value]) => ({ label, value })
  );

  return (
    <aside className="w-[420px] shrink-0 border-l border-border flex flex-col bg-background overflow-hidden">
      <div className="shrink-0 flex items-center justify-between px-5 py-4 border-b border-border bg-background">
        <div>
          <h2 className="text-lg font-semibold">{device.name}</h2>
          <p className="text-xs text-muted-foreground">{getDeviceTypeLabel(device.type)}</p>
        </div>
        <Button variant="ghost" size="icon" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto p-5 space-y-6">
        {/* Status */}
        <div className="space-y-2">
          <Badge variant="outline" className={cn("capitalize", getStatusColor(device.status))}>
            {device.status}
          </Badge>
          <p className="text-sm text-muted-foreground">
            Last seen {formatDistanceToNow(device.lastSeen, { addSuffix: true })}
          </p>
        </div>

        <InfoSection
          title="Device Information"
          items={[
            { label: "Name", value: device.name },
            { label: "Type", value: getDeviceTypeLabel(device.type) },
            { label: "Manufacturer", value: device.manufacturer },
            { label: "Model", value: device.model },
            { label: "Serial Number", value: device.serialNumber },
            { label: "Location", value: device.location },
          ]}
        />

        <InfoSection
          title="Status"
          items={[
            { label: "Status", value: device.status.charAt(0).toUpperCase() + device.status.slice(1) },
            { label: "Last Seen", value: format(device.lastSeen, "MMM d, yyyy HH:mm") },
            { label: "Uptime", value: device.status === "online" ? "99.8%" : "N/A" },
          ]}
        />

        <InfoSection title="Specifications" items={specItems} />

        <InfoSection
          title="Maintenance"
          items={[
            { label: "Last Maintenance", value: "January 10, 2026" },
            { label: "Next Scheduled", value: "April 10, 2026" },
          ]}
        />
      </div>
    </aside>
  );
}
