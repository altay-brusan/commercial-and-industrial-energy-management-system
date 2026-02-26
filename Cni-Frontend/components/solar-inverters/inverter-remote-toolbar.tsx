"use client";

import { useState } from "react";
import { RefreshCw, Zap, Power } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Slider } from "@/components/ui/slider";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Separator } from "@/components/ui/separator";
import type { SolarInverter } from "@/lib/solar-inverters/types";

interface InverterRemoteToolbarProps {
  inverter: SolarInverter;
  isAdmin: boolean;
}

// --- Confirm Action Dialog ---
function ConfirmActionDialog({
  open,
  onOpenChange,
  title,
  description,
  onConfirm,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  onConfirm: () => void;
}) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[420px]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              onConfirm();
              onOpenChange(false);
            }}
          >
            Confirm
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// --- Active Power Setting Dialog ---
function ActivePowerSettingDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [value, setValue] = useState(100);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[420px]">
        <DialogHeader>
          <DialogTitle>Active Power Setting</DialogTitle>
          <DialogDescription>
            Set the active power output limit as a percentage of rated power.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Active Power Limit (%)</Label>
            <div className="flex items-center gap-4">
              <Slider
                value={[value]}
                onValueChange={([v]) => setValue(v)}
                min={0}
                max={100}
                step={1}
                className="flex-1"
              />
              <Input
                type="number"
                value={value}
                onChange={(e) => {
                  const v = Math.min(100, Math.max(0, Number(e.target.value)));
                  setValue(v);
                }}
                className="w-20"
                min={0}
                max={100}
              />
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              toast.success(
                `Active power limit set to ${value}%`
              );
              onOpenChange(false);
            }}
          >
            Apply
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// --- Reactive Power Setting Dialog ---
function ReactivePowerSettingDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [mode, setMode] = useState("kVar");
  const [value, setValue] = useState("0");

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[420px]">
        <DialogHeader>
          <DialogTitle>Reactive Power Setting</DialogTitle>
          <DialogDescription>
            Configure reactive power compensation mode and value.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Mode</Label>
            <Select value={mode} onValueChange={setMode}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="kVar">kVar</SelectItem>
                <SelectItem value="cosPhi">cos(φ)</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label>
              Value ({mode === "kVar" ? "kVar" : "cos(φ)"})
            </Label>
            <Input
              type="number"
              value={value}
              onChange={(e) => setValue(e.target.value)}
              step={mode === "cosPhi" ? "0.01" : "1"}
              min={mode === "cosPhi" ? "0" : undefined}
              max={mode === "cosPhi" ? "1" : undefined}
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              toast.success(
                `Reactive power set to ${value} ${mode}`
              );
              onOpenChange(false);
            }}
          >
            Apply
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// --- Power Factor Setting Dialog ---
function PowerFactorSettingDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [pf, setPf] = useState("1.00");
  const [direction, setDirection] = useState("lagging");

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-[420px]">
        <DialogHeader>
          <DialogTitle>Power Factor Setting</DialogTitle>
          <DialogDescription>
            Set the target power factor and direction.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4 py-4">
          <div className="space-y-2">
            <Label>Power Factor (0.80 - 1.00)</Label>
            <Input
              type="number"
              value={pf}
              onChange={(e) => setPf(e.target.value)}
              step="0.01"
              min="0.80"
              max="1.00"
            />
          </div>
          <div className="space-y-2">
            <Label>Direction</Label>
            <RadioGroup value={direction} onValueChange={setDirection}>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="leading" id="leading" />
                <Label htmlFor="leading" className="font-normal">
                  Leading
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="lagging" id="lagging" />
                <Label htmlFor="lagging" className="font-normal">
                  Lagging
                </Label>
              </div>
            </RadioGroup>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={() => {
              toast.success(
                `Power factor set to ${pf} (${direction})`
              );
              onOpenChange(false);
            }}
          >
            Apply
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// --- Main Toolbar ---
export function InverterRemoteToolbar({
  inverter,
  isAdmin,
}: InverterRemoteToolbarProps) {
  const [isOn, setIsOn] = useState(inverter.status !== "offline");
  const [confirmToggle, setConfirmToggle] = useState(false);
  const [confirmReset, setConfirmReset] = useState(false);
  const [confirmArcClear, setConfirmArcClear] = useState(false);
  const [activePowerOpen, setActivePowerOpen] = useState(false);
  const [reactivePowerOpen, setReactivePowerOpen] = useState(false);
  const [powerFactorOpen, setPowerFactorOpen] = useState(false);

  if (!isAdmin) return null;

  const isOffline = inverter.status === "offline";
  const hasActiveDcArcFault = false; // mock: no active DC arc fault

  const disabledWrapper = (children: React.ReactNode, message: string) => {
    if (!isOffline) return children;
    return (
      <Tooltip>
        <TooltipTrigger asChild>
          <span className="inline-flex">{children}</span>
        </TooltipTrigger>
        <TooltipContent>{message}</TooltipContent>
      </Tooltip>
    );
  };

  return (
    <TooltipProvider>
      <div className="shrink-0 border-b bg-muted/30 px-6 py-2">
        <div className="flex items-center gap-3">
          {/* On/Off Toggle */}
          <div className="flex items-center gap-2">
            <Power
              className={`h-4 w-4 ${isOn ? "text-status-operational" : "text-muted-foreground"}`}
            />
            <Switch
              checked={isOn}
              onCheckedChange={() => setConfirmToggle(true)}
              disabled={false}
              className="data-[state=checked]:bg-status-operational data-[state=unchecked]:bg-severity-critical/60"
            />
            <span
              className={`text-xs font-medium ${isOn ? "text-status-operational" : "text-severity-critical"}`}
            >
              {isOn ? "ON" : "OFF"}
            </span>
          </div>

          <Separator orientation="vertical" className="h-6" />

          {/* Reset */}
          {disabledWrapper(
            <Button
              variant="outline"
              size="sm"
              className="h-7 gap-1.5 text-xs"
              onClick={() => setConfirmReset(true)}
              disabled={isOffline}
            >
              <RefreshCw className="h-3.5 w-3.5" />
              Reset
            </Button>,
            "Device offline"
          )}

          {/* DC Arc Fault Clear */}
          {disabledWrapper(
            <Button
              variant="outline"
              size="sm"
              className="h-7 gap-1.5 text-xs"
              onClick={() => setConfirmArcClear(true)}
              disabled={isOffline || !hasActiveDcArcFault}
            >
              <Zap className="h-3.5 w-3.5" />
              DC Arc Fault Clear
            </Button>,
            isOffline ? "Device offline" : "No active DC arc fault"
          )}

          <Separator orientation="vertical" className="h-6" />

          {/* Active Power */}
          {disabledWrapper(
            <Button
              variant="outline"
              size="sm"
              className="h-7 text-xs"
              onClick={() => setActivePowerOpen(true)}
              disabled={isOffline}
            >
              Active Power
            </Button>,
            "Device offline"
          )}

          {/* Reactive Power */}
          {disabledWrapper(
            <Button
              variant="outline"
              size="sm"
              className="h-7 text-xs"
              onClick={() => setReactivePowerOpen(true)}
              disabled={isOffline}
            >
              Reactive Power
            </Button>,
            "Device offline"
          )}

          {/* Power Factor */}
          {disabledWrapper(
            <Button
              variant="outline"
              size="sm"
              className="h-7 text-xs"
              onClick={() => setPowerFactorOpen(true)}
              disabled={isOffline}
            >
              Power Factor
            </Button>,
            "Device offline"
          )}
        </div>
      </div>

      {/* Dialogs */}
      <ConfirmActionDialog
        open={confirmToggle}
        onOpenChange={setConfirmToggle}
        title={isOn ? "Turn Off Inverter" : "Turn On Inverter"}
        description={
          isOn
            ? `Are you sure you want to turn off ${inverter.deviceName}? This will stop power generation.`
            : `Are you sure you want to turn on ${inverter.deviceName}?`
        }
        onConfirm={() => {
          setIsOn(!isOn);
          toast.success(
            `${inverter.deviceName} ${isOn ? "turned off" : "turned on"} successfully`
          );
        }}
      />
      <ConfirmActionDialog
        open={confirmReset}
        onOpenChange={setConfirmReset}
        title="Reset Inverter"
        description={`Are you sure you want to reset ${inverter.deviceName}? This will restart the device.`}
        onConfirm={() => {
          toast.success(
            `${inverter.deviceName} reset command sent successfully`
          );
        }}
      />
      <ConfirmActionDialog
        open={confirmArcClear}
        onOpenChange={setConfirmArcClear}
        title="Clear DC Arc Fault"
        description={`Are you sure you want to clear DC arc fault on ${inverter.deviceName}?`}
        onConfirm={() => {
          toast.success(
            `DC arc fault cleared on ${inverter.deviceName}`
          );
        }}
      />
      <ActivePowerSettingDialog
        open={activePowerOpen}
        onOpenChange={setActivePowerOpen}
      />
      <ReactivePowerSettingDialog
        open={reactivePowerOpen}
        onOpenChange={setReactivePowerOpen}
      />
      <PowerFactorSettingDialog
        open={powerFactorOpen}
        onOpenChange={setPowerFactorOpen}
      />
    </TooltipProvider>
  );
}
