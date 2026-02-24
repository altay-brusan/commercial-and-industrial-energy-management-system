"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { getDefaultUnitSettings } from "@/lib/bess/mock-data";
import type { BessUnitSettings } from "@/lib/bess/types";

interface BessUnitSettingsTabProps {
  unitId: string;
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-3 first:mt-0">
      {title}
    </h3>
  );
}

function FormField({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <Label className="text-sm">{label}</Label>
      {children}
    </div>
  );
}

export function BessUnitSettingsTab({ unitId }: BessUnitSettingsTabProps) {
  const defaults = getDefaultUnitSettings(unitId);
  const [settings, setSettings] = useState<BessUnitSettings>(defaults);

  const update = (field: keyof BessUnitSettings, value: string | number | boolean) => {
    setSettings((prev) => ({ ...prev, [field]: value }));
  };

  const handleReset = () => setSettings(defaults);

  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-y-auto px-6 py-4">
        <SectionTitle title="Device Information" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Device Name">
            <Input
              value={settings.deviceName}
              onChange={(e) => update("deviceName", e.target.value)}
            />
          </FormField>
          <FormField label="Protocol">
            <Input
              value={settings.protocol}
              onChange={(e) => update("protocol", e.target.value)}
            />
          </FormField>
          <FormField label="Firmware Version">
            <Input value={settings.firmwareVersion} disabled className="bg-muted" />
          </FormField>
          <FormField label="Serial Number">
            <Input value={settings.serialNumber} disabled className="bg-muted" />
          </FormField>
          <FormField label="Modbus Address">
            <Input
              type="number"
              value={settings.modbusAddress}
              onChange={(e) => update("modbusAddress", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Maintenance" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Maintenance Interval (days)">
            <Input
              type="number"
              value={settings.maintenanceIntervalDays}
              onChange={(e) => update("maintenanceIntervalDays", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Alarm Thresholds" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Temperature Alarm Threshold (°C)">
            <Input
              type="number"
              value={settings.tempAlarmThreshold}
              onChange={(e) => update("tempAlarmThreshold", Number(e.target.value))}
            />
          </FormField>
          <FormField label="SoC Low Alarm (%)">
            <Input
              type="number"
              value={settings.socLowAlarm}
              onChange={(e) => update("socLowAlarm", Number(e.target.value))}
            />
          </FormField>
          <FormField label="SoC High Alarm (%)">
            <Input
              type="number"
              value={settings.socHighAlarm}
              onChange={(e) => update("socHighAlarm", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Rack Configuration — SoC Management" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="SoC Calibration Interval (hours)">
            <Input
              type="number"
              value={settings.socCalibrationIntervalHours}
              onChange={(e) => update("socCalibrationIntervalHours", Number(e.target.value))}
            />
          </FormField>
          <FormField label="Charge Stop SoC (%)">
            <Input
              type="number"
              value={settings.chargeStopSoc}
              onChange={(e) => update("chargeStopSoc", Number(e.target.value))}
            />
          </FormField>
          <FormField label="Discharge Stop SoC (%)">
            <Input
              type="number"
              value={settings.dischargeStopSoc}
              onChange={(e) => update("dischargeStopSoc", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Rack Configuration — Operating Mode" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Operating Mode">
            <Select
              value={settings.operatingMode}
              onValueChange={(v) => update("operatingMode", v)}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="auto">Auto</SelectItem>
                <SelectItem value="charge">Charge Only</SelectItem>
                <SelectItem value="discharge">Discharge Only</SelectItem>
                <SelectItem value="standby">Standby</SelectItem>
              </SelectContent>
            </Select>
          </FormField>
        </div>

        <SectionTitle title="Rack Configuration — Safety" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Insulation Resistance Threshold (k&Omega;)">
            <Input
              type="number"
              value={settings.insulationResistanceThreshold}
              onChange={(e) => update("insulationResistanceThreshold", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Rack Configuration — Cell Balancing" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="flex items-center justify-between py-2">
            <Label className="text-sm">Balancing Enabled</Label>
            <Switch
              checked={settings.balancingEnabled}
              onCheckedChange={(v) => update("balancingEnabled", v)}
            />
          </div>
          <FormField label="Balancing Threshold (mV)">
            <Input
              type="number"
              value={settings.balancingThresholdMv}
              onChange={(e) => update("balancingThresholdMv", Number(e.target.value))}
            />
          </FormField>
        </div>

        <div className="h-6" />
      </div>

      {/* Sticky footer */}
      <div className="shrink-0 px-6 py-3 border-t bg-background flex items-center justify-end gap-3">
        <Button variant="outline" onClick={handleReset}>
          Reset to Defaults
        </Button>
        <Button>Save Changes</Button>
      </div>
    </div>
  );
}
