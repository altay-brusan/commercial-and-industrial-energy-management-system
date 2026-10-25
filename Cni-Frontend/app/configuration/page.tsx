"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { Slider } from "@/components/ui/slider";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { CheckCircle2 } from "lucide-react";

export default function ConfigurationPage() {
  const [minSoCForDischarge, setMinSoCForDischarge] = useState(20);
  const [maxSoCForCharge, setMaxSoCForCharge] = useState(95);
  const [distributionCost, setDistributionCost] = useState(0.025);

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="configuration" />
      <div className="flex flex-1 overflow-hidden">
        <ConfigurationSidebar />
        <div className="flex-1 flex flex-col overflow-hidden">
          <div className="shrink-0 flex items-center justify-between px-6 py-3 border-b border-border">
            <div>
              <h1 className="text-lg font-semibold">Basic Settings</h1>
              <p className="text-sm text-muted-foreground">General dispatch and battery configuration</p>
            </div>
            <Button size="sm">
              <CheckCircle2 className="h-4 w-4 mr-1.5" />
              Save
            </Button>
          </div>
          <main className="flex-1 overflow-auto p-6">
            <div className="max-w-xl space-y-6">
              {/* Battery Settings */}
              <section className="border border-border rounded-lg">
                <div className="px-5 py-3 border-b border-border">
                  <h2 className="text-sm font-semibold">Battery Settings</h2>
                  <p className="text-xs text-muted-foreground">SoC limits for charging and discharging</p>
                </div>
                <div className="px-5 py-4 space-y-5">
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <Label className="text-sm">Min SoC Discharge</Label>
                      <span className="text-xs text-muted-foreground">{minSoCForDischarge}%</span>
                    </div>
                    <Slider value={[minSoCForDischarge]} min={0} max={100} step={1} onValueChange={([v]) => setMinSoCForDischarge(v)} />
                  </div>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <Label className="text-sm">Max SoC Charge</Label>
                      <span className="text-xs text-muted-foreground">{maxSoCForCharge}%</span>
                    </div>
                    <Slider value={[maxSoCForCharge]} min={0} max={100} step={1} onValueChange={([v]) => setMaxSoCForCharge(v)} />
                  </div>
                </div>
              </section>

              {/* Price Settings */}
              <section className="border border-border rounded-lg">
                <div className="px-5 py-3 border-b border-border">
                  <h2 className="text-sm font-semibold">Price Settings</h2>
                  <p className="text-xs text-muted-foreground">Cost parameters for energy pricing</p>
                </div>
                <div className="px-5 py-4 space-y-5">
                  <div className="space-y-2">
                    <Label className="text-sm">Distribution Cost ($/kWh)</Label>
                    <Input
                      type="number"
                      step="0.001"
                      value={distributionCost}
                      onChange={(e) => setDistributionCost(Number(e.target.value))}
                      className="h-8 text-sm"
                      placeholder="0.000"
                    />
                  </div>
                </div>
              </section>
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
