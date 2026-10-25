export type ScadaNodeId = "load" | "battery" | "solar" | "grid";

export interface ScadaNodeData {
  id: ScadaNodeId;
  name: string;
  realPower: number;
  unit: string;
  secondaryLabel?: string;
  secondaryValue?: string;
  tertiaryLabel?: string;
  tertiaryValue?: string;
  color: string;
}

export type FlowDirection = "forward" | "reverse" | "idle";

export interface FlowConnection {
  from: ScadaNodeId;
  to: ScadaNodeId;
  power: number;
  direction: FlowDirection;
}

export interface ScadaMetric {
  label: string;
  value: string;
  unit?: string;
}

export interface ScadaMetricsSection {
  id: ScadaNodeId;
  title: string;
  color: string;
  metrics: ScadaMetric[];
}

export interface ScadaState {
  nodes: ScadaNodeData[];
  connections: FlowConnection[];
  metricsSections: ScadaMetricsSection[];
  lastUpdated: Date;
}
