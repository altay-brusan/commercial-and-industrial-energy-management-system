// PCS (Power Conversion System) types

export type PcsUnitStatus = "online" | "offline" | "fault" | "standby";

export interface PcsAcMeasurement {
  lineVoltageAbV: number;
  lineVoltageBcV: number;
  lineVoltageCaV: number;
  phaseVoltageAV: number;
  phaseVoltageBV: number;
  phaseVoltageCV: number;
  phaseCurrentAA: number;
  phaseCurrentBA: number;
  phaseCurrentCA: number;
  inductorCurrentAA: number;
  inductorCurrentBA: number;
  inductorCurrentCA: number;
}

export interface PcsDcMeasurement {
  totalBusV: number;
  posBusV: number;
  negBusV: number;
  batV: number;
  batI: number;
  dcPwrKw: number;
  acPwrFactor: number;
  totalDcI: number;
}

export interface PcsPowerPhase {
  activeKw: number;
  apparentKva: number;
  reactiveKvar: number;
}

export interface PcsPowerState {
  phaseA: PcsPowerPhase;
  phaseB: PcsPowerPhase;
  phaseC: PcsPowerPhase;
  totalActiveKw: number;
  totalApparentKva: number;
  totalReactiveKvar: number;
}

export interface PcsStatusState {
  gridFreqHz: number;
  gridPhaseSeq: "positive" | "negative";
  deratCoeff: number;
  workingCondition: string;
  sourceMode: string;
  gridMode: string;
}

export interface PcsTemperatureState {
  igbtTempC: number;
  ambientTempC: number;
  inductanceTempC: number;
}

export interface PcsAlgoParams {
  ccCvRechgV: number;
  batCvChgV: number;
  cvChgMaxTimeMin: number;
  cvToFloatI: number;
  floatChgV: number;
  floatToCcI: number;
}

export interface PcsUnit {
  id: string;
  deviceId: string;
  name: string;
  linkedBessUnitId: string;
  linkedBessUnitName: string;
  status: PcsUnitStatus;
  ac: PcsAcMeasurement;
  dc: PcsDcMeasurement;
  power: PcsPowerState;
  pcsStatus: PcsStatusState;
  temperatures: PcsTemperatureState;
  algoParams: PcsAlgoParams;
}

// PCS Fault types
export type PcsFaultCategory =
  | "hardware"
  | "grid"
  | "bus_battery"
  | "ac_protection"
  | "system"
  | "relay"
  | "inverter"
  | "timeout";

export interface PcsFaultFlag {
  key: string;
  label: string;
  category: PcsFaultCategory;
  active: boolean;
}
