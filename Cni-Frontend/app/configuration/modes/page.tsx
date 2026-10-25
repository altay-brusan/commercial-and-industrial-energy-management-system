"use client";

import { useState } from "react";
import { Ban, CheckCircle2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { SelfConsumptionPanel } from "@/components/control/self-consumption-panel";
import { TouPanel } from "@/components/control/tou-panel";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

type ConfigMode = "none" | "self-consumption" | "tou";

export default function ModesPage() {
  const [activeMode, setActiveMode] = useState<ConfigMode>("tou");

  const handleApply = () => {
    // apply mode logic
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="configuration" />
      <div className="flex flex-1 overflow-hidden">
        <ConfigurationSidebar />

        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Mode selector bar */}
          <div className="shrink-0 flex items-center justify-between px-6 py-2 border-b border-border">
            <div className="flex items-center gap-3">
              <Label className="text-sm font-medium">Mode</Label>
              <Select value={activeMode} onValueChange={(v) => setActiveMode(v as ConfigMode)}>
                <SelectTrigger className="w-[200px] h-9">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None</SelectItem>
                  <SelectItem value="self-consumption">Self Consumption</SelectItem>
                  <SelectItem value="tou">Time of Use</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <Button size="sm" onClick={handleApply} disabled={activeMode === "none"}>
              <CheckCircle2 className="h-4 w-4 mr-1.5" />
              Apply Mode
            </Button>
          </div>

          {/* Title bar */}
          {activeMode !== "none" && (
            <div className="shrink-0 px-6 py-3 border-b border-border">
              <h1 className="text-lg font-semibold">
                {activeMode === "self-consumption" ? "Self Consumption" : "Time of Use"}
              </h1>
              <p className="text-sm text-muted-foreground">
                {activeMode === "self-consumption"
                  ? "Maximize solar usage and minimize grid dependency"
                  : "Optimize charging and discharging based on tariff schedules"}
              </p>
            </div>
          )}

          {/* Content */}
          <div className="flex flex-1 overflow-hidden">
            {activeMode === "none" && (
              <main className="flex-1 flex items-center justify-center">
                <div className="text-center text-muted-foreground">
                  <Ban className="h-12 w-12 mx-auto mb-3 opacity-30" />
                  <p className="text-sm">No mode selected</p>
                  <p className="text-xs mt-1">Select a mode above to configure</p>
                </div>
              </main>
            )}
            {activeMode === "self-consumption" && <SelfConsumptionPanel />}
            {activeMode === "tou" && <TouPanel />}
          </div>
        </div>
      </div>
    </div>
  );
}
