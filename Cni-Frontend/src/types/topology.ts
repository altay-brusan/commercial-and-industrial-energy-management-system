export interface NodeData {
  id: string;
  label?: string;
  soc?: number;
  temperature?: number;
  state?: string;
  performance?: number;
  generation?: number;
  powerConsumption?: number;
  powerFactor?: number;
  value?: string;
  status?: string;
}

export interface TopologyNode {
  id: string;
  type: string;
  extent: string | null;
  parentId: string;
  data: NodeData;
  position: {
    x: number;
    y: number;
  };
  style: any;
  targetHandleCount: number | null;
  sourceHandleCount: number | null;
}

export interface Edge {
  id: string;
  source: string;
  target: string;
  type: string;
  data: {
    id: string;
  };
  style: any;
  sourceHandle: string | null;
  targetHandle: string | null;
}

export interface Topology {
  nodes: TopologyNode[];
  edges: Edge[];
}
