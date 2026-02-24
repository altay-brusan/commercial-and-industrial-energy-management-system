"use client";

import { useState } from "react";
import { Power } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { PeakShavingPanel } from "@/components/control/peak-shaving-panel";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";

export default function CapacityLimitsPage() {
  const [enabled, setEnabled] = useState(true);

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="configuration" />
      <div className="flex flex-1 overflow-hidden">
        <ConfigurationSidebar />
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Enable/disable bar */}
          <div className="shrink-0 flex items-center px-6 py-3.5 border-b border-border">
            <div className="flex items-center gap-3">
              <Switch checked={enabled} onCheckedChange={setEnabled} />
              <Label className="text-sm font-medium">Peak Shaving</Label>
            </div>
          </div>

          {/* Title bar */}
          {enabled && (
            <div className="shrink-0 px-6 py-3 border-b border-border">
              <h1 className="text-lg font-semibold">Peak Shaving</h1>
              <p className="text-sm text-muted-foreground">Configure demand threshold and BESS response for peak shaving</p>
            </div>
          )}

          {/* Content */}
          <div className="flex flex-1 overflow-hidden">
            {enabled ? (
              <PeakShavingPanel loadLabel="Demand" />
            ) : (
              <main className="flex-1 flex items-center justify-center">
                <div className="text-center text-muted-foreground">
                  <Power className="h-12 w-12 mx-auto mb-3 opacity-30" />
                  <p className="text-sm">Peak Shaving is disabled</p>
                  <p className="text-xs mt-1">Enable the toggle above to configure</p>
                </div>
              </main>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
