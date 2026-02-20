"use client";

import { Activity } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import type { BessHvac, BessChillerState } from "@/lib/bess/types";

interface BessHvacMonitoringTabProps {
  hvac: BessHvac;
}

function Metric({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        <span className="text-sm font-medium">{value}</span>
        <Activity className="h-3.5 w-3.5 text-muted-foreground/40" />
      </div>
    </div>
  );
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-2 first:mt-0">
      {title}
    </h3>
  );
}

function StatusBadge({ active, onLabel, offLabel }: { active: boolean; onLabel?: string; offLabel?: string }) {
  return active ? (
    <Badge className="bg-status-operational/15 text-status-operational border-status-operational/30 text-xs">{onLabel ?? "On"}</Badge>
  ) : (
    <Badge variant="outline" className="text-muted-foreground text-xs">{offLabel ?? "Off"}</Badge>
  );
}

export function BessHvacMonitoringTab({ hvac }: BessHvacMonitoringTabProps) {
  const cs = hvac.chillerState;

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Operating Mode */}
      <SectionTitle title="Operating Status" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Operating Mode</span>
          <Badge variant="secondary" className="text-xs">{cs.operatingModeStr}</Badge>
        </div>
        <Metric label="Ambient Temp" value={`${cs.ambientTempC}°C`} />
      </div>

      {/* Coolant Temperatures */}
      <SectionTitle title="Coolant System" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <Metric label="Outlet Coolant Temp" value={`${cs.outletCoolantTempC}°C`} />
        <Metric label="Inlet Coolant Temp" value={`${cs.inletCoolantTempC}°C`} />
        <Metric
          label="Delta T"
          value={`${Math.abs(cs.inletCoolantTempC - cs.outletCoolantTempC).toFixed(1)}°C`}
        />
        <Metric label="Inlet Pressure" value={`${cs.inletCoolantPressure} bar`} />
        <Metric label="Outlet Pressure" value={`${cs.outletCoolantPressure} bar`} />
      </div>

      {/* Setpoints */}
      <SectionTitle title="Setpoints" />
      <div className="border rounded-md p-4 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Coolant Setpoint</span>
            <span className="text-sm font-medium">{hvac.setpoints.coolantTempSetpoint}°C</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">High Alarm</span>
            <span className="text-sm font-medium text-severity-high">{hvac.setpoints.highAlarm}°C</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Low Alarm</span>
            <span className="text-sm font-medium text-severity-high">{hvac.setpoints.lowAlarm}°C</span>
          </div>
        </div>
      </div>

      {/* Component Status */}
      <SectionTitle title="Components" />
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Compressor</span>
          <div className="flex items-center gap-2">
            <StatusBadge active={cs.compressorStatus} />
            <span className="text-xs text-muted-foreground">{cs.compressorSpeed}%</span>
          </div>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Pump</span>
          <div className="flex items-center gap-2">
            <StatusBadge active={cs.pumpStatus} />
            <span className="text-xs text-muted-foreground">{cs.pumpSpeed}%</span>
          </div>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Fan / Heater</span>
          <div className="flex items-center gap-2">
            <StatusBadge active={cs.fanRunStatus} onLabel="Fan" offLabel="Fan" />
            <StatusBadge active={cs.electricHeaterStatus} onLabel="Heater" offLabel="Heater" />
          </div>
        </div>
      </div>

      {/* Compressor Speed Progress */}
      <SectionTitle title="Compressor Load" />
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Speed</span>
          <span className="text-sm font-semibold">{cs.compressorSpeed}%</span>
        </div>
        <Progress value={cs.compressorSpeed} className="h-2" />
      </div>
      <div className="mt-4 space-y-2">
        <div className="flex items-center justify-between">
          <span className="text-sm text-muted-foreground">Pump Speed</span>
          <span className="text-sm font-semibold">{cs.pumpSpeed}%</span>
        </div>
        <Progress value={cs.pumpSpeed} className="h-2" />
      </div>

      <div className="h-6" />
    </div>
  );
}
