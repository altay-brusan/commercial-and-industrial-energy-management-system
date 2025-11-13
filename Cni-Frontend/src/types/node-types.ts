export interface CustomNodeData {
  id?: string;
  label?: string;
  type?: 'bess' | 'gess' | 'pcs' | 'grid' | 'facility' | 'ev-charger' | 'heat-pump';
  performance?: number;
  temperature?: number;
  state?: string;
  generation?: number;
  powerConsumption?: number;
  powerFactor?: number;
  capacity?: number;
  value?: number;
  segments?: ColorSegment[];
  soc?: number;
  status?: string;
  reactivePower?: number;
  efficiency?: number;
  acPower?: number;
  dcPower?: number;
}

export interface ColorSegment {
  color: string;
  percentage: number;
}

export interface CustomNode {
  id: string;
  type: string;
  position: { x: number; y: number };
  data: CustomNodeData;
}

export enum BESSStatus {
    STANDBY = 0,
    CHARGING = 1,
    DISCHARGING = 2,
    ERROR = 3
}