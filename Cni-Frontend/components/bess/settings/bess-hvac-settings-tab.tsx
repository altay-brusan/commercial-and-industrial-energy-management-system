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
import { getDefaultHvacSettings } from "@/lib/bess/mock-data";
import type { BessHvacSettings } from "@/lib/bess/types";

interface BessHvacSettingsTabProps {
  hvacId: string;
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

export function BessHvacSettingsTab({ hvacId }: BessHvacSettingsTabProps) {
  const defaults = getDefaultHvacSettings(hvacId);
  const [settings, setSettings] = useState<BessHvacSettings>(defaults);

  const update = (field: keyof BessHvacSettings, value: string | number | boolean) => {
    setSettings((prev) => ({ ...prev, [field]: value }));
  };

  const handleReset = () => setSettings(defaults);

  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-y-auto px-6 py-4">
        <SectionTitle title="Operating Mode" />
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
                <SelectItem value="Cooling">Cooling</SelectItem>
                <SelectItem value="Heating">Heating</SelectItem>
                <SelectItem value="Standby">Standby</SelectItem>
                <SelectItem value="Auto">Auto</SelectItem>
              </SelectContent>
            </Select>
          </FormField>
        </div>

        <SectionTitle title="Temperature Setpoints" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Coolant Temperature Setpoint (°C)">
            <Input
              type="number"
              value={settings.coolantTempSetpoint}
              onChange={(e) => update("coolantTempSetpoint", Number(e.target.value))}
            />
          </FormField>
          <FormField label="High Temperature Alarm (°C)">
            <Input
              type="number"
              value={settings.highTempAlarm}
              onChange={(e) => update("highTempAlarm", Number(e.target.value))}
            />
          </FormField>
          <FormField label="Low Temperature Alarm (°C)">
            <Input
              type="number"
              value={settings.lowTempAlarm}
              onChange={(e) => update("lowTempAlarm", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Component Limits" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Compressor Max Speed (%)">
            <Input
              type="number"
              value={settings.compressorMaxSpeed}
              onChange={(e) => update("compressorMaxSpeed", Number(e.target.value))}
            />
          </FormField>
          <FormField label="Pump Max Speed (%)">
            <Input
              type="number"
              value={settings.pumpMaxSpeed}
              onChange={(e) => update("pumpMaxSpeed", Number(e.target.value))}
            />
          </FormField>
          <div className="flex items-center justify-between py-2">
            <Label className="text-sm">Auto Defrost</Label>
            <Switch
              checked={settings.autoDefrost}
              onCheckedChange={(v) => update("autoDefrost", v)}
            />
          </div>
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
