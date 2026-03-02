"use client";

import { EnergyFlowCanvas } from "@/components/scada/energy-flow-canvas";
import { useScadaSimulation } from "@/hooks/use-scada-simulation";

export function EnergyFlowCard() {
  const scadaData = useScadaSimulation(5000);

  return (
    <div className="flex flex-col overflow-hidden h-full">
      <EnergyFlowCanvas
        nodes={scadaData.nodes}
        connections={scadaData.connections}
        selectedNode={null}
        onNodeClick={() => {}}
        showGrid={false}
        compact
      />
    </div>
  );
}
