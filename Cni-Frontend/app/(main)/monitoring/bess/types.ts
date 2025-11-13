export interface BESSData {
  id: string;
  stateOfCharge: number;
  powerOutput: string;
  temperature: number;
  soh: number;
  cooling: string;
  cabinStatus: string[];
  type?: string;
  nominalCapacity?: string;
  batteryCapacity?: string;
  cellNumber?: number;
  weight?: string;
  batteryCycles?: number;
  mode?: 'charging' | 'discharging' | 'standby';
  source?: string;
  power?: string;
  color?: string;
}

export enum BESSStatus {
    STANDBY = 0,
    CHARGING = 1,
    DISCHARGING = 2,
    ERROR = 3
}