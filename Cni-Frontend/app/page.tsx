"use client";

import { TopBar } from "@/components/layout/top-bar";
import { Badge } from "@/components/ui/badge";
import { TotalEnergyUsage } from "@/components/dashboard-2/total-energy-usage";
import { CurrentEnergySource } from "@/components/dashboard-2/current-energy-source";
import { BESSStateOfCharge } from "@/components/dashboard-2/bess-state-of-charge";
import { PCSState } from "@/components/dashboard-2/pcs-state";
import { SolarEnergyCapacity } from "@/components/dashboard-2/solar-energy-capacity";
import { EnergyConsumptionChart } from "@/components/dashboard-2/energy-consumption-chart";
import { CO2Summary } from "@/components/dashboard-2/co2-summary";
import { SourceDistributionChart } from "@/components/dashboard-2/source-distribution-chart";
import { CostSavingsChart } from "@/components/dashboard-2/cost-savings-chart";
import { EnergyFlowCard } from "@/components/dashboard-2/energy-flow-card";
import {
  totalEnergyUsage,
  energySourceData,
  dailyEnergyCost,
  bessStateData,
} from "@/lib/dashboard-2/mock-data";

const formatTL = (value: number) =>
  value.toLocaleString("tr-TR", { minimumFractionDigits: 2 });

export default function Dashboard() {
  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="dashboard" />
      <main className="flex-1 overflow-y-auto">
        {/* Stats Bar */}
        <div className="grid grid-cols-2 md:grid-cols-4 border-b border-border">
          <div className="px-6 py-5 border-r border-border">
            <p className="text-sm text-muted-foreground mb-1">Total Energy</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">
                {totalEnergyUsage.totalKWh}
              </span>
              <span className="text-sm text-muted-foreground">kWh</span>
            </div>
          </div>
          <div className="px-6 py-5 border-r border-border">
            <p className="text-sm text-muted-foreground mb-1">Current Power</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">
                {energySourceData.totalKW}
              </span>
              <span className="text-sm text-muted-foreground">kW</span>
            </div>
          </div>
          <div className="px-6 py-5 border-r border-border">
            <p className="text-sm text-muted-foreground mb-1">Daily Cost</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">
                {formatTL(dailyEnergyCost.totalCostTL)}
              </span>
              <span className="text-sm text-muted-foreground">TL</span>
            </div>
            <Badge variant="secondary" className="mt-2 text-xs font-normal text-energy-grid">
              +5%
            </Badge>
          </div>
          <div className="px-6 py-5">
            <p className="text-sm text-muted-foreground mb-1">BESS SOC</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">
                {bessStateData.socPercent.toFixed(1)}
              </span>
              <span className="text-sm text-muted-foreground">%</span>
            </div>
            <Badge variant="secondary" className="mt-2 text-xs font-normal text-energy-solar">
              Full
            </Badge>
          </div>
        </div>

        {/* Energy Overview Section */}
        <div className="grid lg:grid-cols-3 border-b border-border">
          <div className="border-r border-border">
            <EnergyFlowCard />
          </div>
          <div className="border-r border-border">
            <TotalEnergyUsage />
          </div>
          <div>
            <CurrentEnergySource />
          </div>
        </div>

        {/* System Status Section */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">System Status</h2>
            <p className="text-sm text-muted-foreground">
              Live device overview
            </p>
          </div>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
            <BESSStateOfCharge />
            <PCSState />
            <SolarEnergyCapacity />
          </div>
        </div>

        {/* Analytics Section */}
        <div className="px-6 py-6 space-y-4">
          <div className="grid gap-4 lg:grid-cols-4">
            <div className="lg:col-span-3">
              <EnergyConsumptionChart />
            </div>
            <CO2Summary />
          </div>
          <div className="grid gap-4 lg:grid-cols-2">
            <SourceDistributionChart />
            <CostSavingsChart />
          </div>
        </div>
      </main>
    </div>
  );
}
