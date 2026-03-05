"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { EnergyFlowCanvas } from "@/components/scada/energy-flow-canvas";
import { ScadaMetricsPanel } from "@/components/scada/scada-metrics-panel";
import { useScadaSimulation } from "@/hooks/use-scada-simulation";
import type { ScadaNodeId } from "@/lib/scada/types";

export default function Monitoring() {
  const [selectedNode, setSelectedNode] = useState<ScadaNodeId | null>(null);
  const scadaData = useScadaSimulation(5000);

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />

        {/* Main Diagram Area */}
        <main className="flex-1 flex items-center justify-center overflow-hidden bg-background">
          <EnergyFlowCanvas
            nodes={scadaData.nodes}
            connections={scadaData.connections}
            selectedNode={selectedNode}
            onNodeClick={setSelectedNode}
          />
        </main>

        {/* Metrics Sidebar */}
        <ScadaMetricsPanel
          metricsSections={scadaData.metricsSections}
          selectedNode={selectedNode}
          lastUpdated={scadaData.lastUpdated}
        />
      </div>
    </div>
  );
}
