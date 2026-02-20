"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { getDefaultEscSettings } from "@/lib/bess/mock-data";
import type { BessEscSettings } from "@/lib/bess/types";

interface BessEscSettingsTabProps {
  escId: string;
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

export function BessEscSettingsTab({ escId }: BessEscSettingsTabProps) {
  const defaults = getDefaultEscSettings(escId);
  const [settings, setSettings] = useState<BessEscSettings>(defaults);

  const update = (field: keyof BessEscSettings, value: string | number | boolean) => {
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
          <FormField label="Model">
            <Input value={settings.model} disabled className="bg-muted" />
          </FormField>
          <FormField label="Serial Number">
            <Input value={settings.serialNumber} disabled className="bg-muted" />
          </FormField>
          <FormField label="Firmware">
            <Input value={settings.firmware} disabled className="bg-muted" />
          </FormField>
          <FormField label="Protocol">
            <Input
              value={settings.protocol}
              onChange={(e) => update("protocol", e.target.value)}
            />
          </FormField>
          <FormField label="Modbus Address">
            <Input
              type="number"
              value={settings.modbusAddress}
              onChange={(e) => update("modbusAddress", Number(e.target.value))}
            />
          </FormField>
        </div>

        <SectionTitle title="Protection" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField label="Isolation Resistance Threshold (kΩ)">
            <Input
              type="number"
              value={settings.isolationResistanceThreshold}
              onChange={(e) => update("isolationResistanceThreshold", Number(e.target.value))}
            />
          </FormField>
          <FormField label="Max Power Limit (kW)">
            <Input
              type="number"
              value={settings.maxPowerLimit}
              onChange={(e) => update("maxPowerLimit", Number(e.target.value))}
            />
          </FormField>
          <div className="flex items-center justify-between py-2">
            <Label className="text-sm">Auto Restart</Label>
            <Switch
              checked={settings.autoRestart}
              onCheckedChange={(v) => update("autoRestart", v)}
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
