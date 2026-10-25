import type {
  WindTurbine,
  TurbineFault,
  TurbineCurveData,
} from "./types";

export const windTurbines: WindTurbine[] = [
  {
    id: "wt-01",
    deviceName: "WTG-01",
    deviceModel: "V150-4.2 MW",
    serialNumber: "VES-2024-00147",
    status: "normal",
    yieldToday: 42.8,
    activePower: 3650,
    ratedPower: 4200,
    windFarm: "NORTH WIND FARM",
    windSpeed: 11.2,
    windDirection: 225,
    ambientTemp: 8.3,
    rotorSpeed: 10.8,
    generatorSpeed: 1180,
    generatorPower: 3680,
    reactivePower: 120,
    powerFactor: 0.99,
    gridFrequency: 50.01,
    gridVoltage: 690,
    pitchAngles: [2.1, 2.3, 2.0],
    nacellePosition: 223,
    yawError: 1.8,
    generatorTemp: 82,
    gearboxOilTemp: 58,
    mainBearingTemp: 45,
    controlCabinetTemp: 32,
    totalYield: 18450,
    totalOperatingHours: 12840,
    dailyOperatingTime: 18.5,
    hubHeight: 105,
    rotorDiameter: 150,
    commissioningDate: "15/Mar/2024",
    manufacturer: "Vestas",
  },
  {
    id: "wt-02",
    deviceName: "WTG-02",
    deviceModel: "V150-4.2 MW",
    serialNumber: "VES-2024-00148",
    status: "normal",
    yieldToday: 44.1,
    activePower: 3820,
    ratedPower: 4200,
    windFarm: "NORTH WIND FARM",
    windSpeed: 11.8,
    windDirection: 228,
    ambientTemp: 8.1,
    rotorSpeed: 11.1,
    generatorSpeed: 1195,
    generatorPower: 3850,
    reactivePower: 135,
    powerFactor: 0.99,
    gridFrequency: 50.0,
    gridVoltage: 691,
    pitchAngles: [1.8, 1.9, 1.7],
    nacellePosition: 226,
    yawError: 1.2,
    generatorTemp: 84,
    gearboxOilTemp: 60,
    mainBearingTemp: 46,
    controlCabinetTemp: 33,
    totalYield: 19200,
    totalOperatingHours: 13100,
    dailyOperatingTime: 19.2,
    hubHeight: 105,
    rotorDiameter: 150,
    commissioningDate: "15/Mar/2024",
    manufacturer: "Vestas",
  },
  {
    id: "wt-03",
    deviceName: "WTG-03",
    deviceModel: "V150-4.2 MW",
    serialNumber: "VES-2024-00149",
    status: "normal",
    yieldToday: 40.5,
    activePower: 3480,
    ratedPower: 4200,
    windFarm: "NORTH WIND FARM",
    windSpeed: 10.5,
    windDirection: 220,
    ambientTemp: 8.5,
    rotorSpeed: 10.4,
    generatorSpeed: 1150,
    generatorPower: 3510,
    reactivePower: 110,
    powerFactor: 0.99,
    gridFrequency: 50.02,
    gridVoltage: 689,
    pitchAngles: [2.5, 2.6, 2.4],
    nacellePosition: 218,
    yawError: 2.1,
    generatorTemp: 80,
    gearboxOilTemp: 56,
    mainBearingTemp: 44,
    controlCabinetTemp: 31,
    totalYield: 17800,
    totalOperatingHours: 12600,
    dailyOperatingTime: 17.8,
    hubHeight: 105,
    rotorDiameter: 150,
    commissioningDate: "15/Mar/2024",
    manufacturer: "Vestas",
  },
  {
    id: "wt-04",
    deviceName: "WTG-04",
    deviceModel: "V150-4.2 MW",
    serialNumber: "VES-2024-00150",
    status: "warning",
    yieldToday: 28.3,
    activePower: 2100,
    ratedPower: 4200,
    windFarm: "NORTH WIND FARM",
    windSpeed: 10.1,
    windDirection: 222,
    ambientTemp: 8.4,
    rotorSpeed: 9.2,
    generatorSpeed: 1020,
    generatorPower: 2130,
    reactivePower: 85,
    powerFactor: 0.98,
    gridFrequency: 50.01,
    gridVoltage: 688,
    pitchAngles: [5.2, 5.4, 5.1],
    nacellePosition: 220,
    yawError: 4.5,
    generatorTemp: 92,
    gearboxOilTemp: 72,
    mainBearingTemp: 55,
    controlCabinetTemp: 38,
    totalYield: 16500,
    totalOperatingHours: 12200,
    dailyOperatingTime: 14.2,
    hubHeight: 105,
    rotorDiameter: 150,
    commissioningDate: "15/Mar/2024",
    manufacturer: "Vestas",
  },
  {
    id: "wt-05",
    deviceName: "WTG-05",
    deviceModel: "V150-4.2 MW",
    serialNumber: "VES-2024-00151",
    status: "normal",
    yieldToday: 43.2,
    activePower: 3720,
    ratedPower: 4200,
    windFarm: "NORTH WIND FARM",
    windSpeed: 11.5,
    windDirection: 226,
    ambientTemp: 8.2,
    rotorSpeed: 10.9,
    generatorSpeed: 1185,
    generatorPower: 3750,
    reactivePower: 125,
    powerFactor: 0.99,
    gridFrequency: 50.0,
    gridVoltage: 690,
    pitchAngles: [2.0, 2.1, 1.9],
    nacellePosition: 224,
    yawError: 1.5,
    generatorTemp: 83,
    gearboxOilTemp: 59,
    mainBearingTemp: 45,
    controlCabinetTemp: 32,
    totalYield: 18900,
    totalOperatingHours: 12950,
    dailyOperatingTime: 18.9,
    hubHeight: 105,
    rotorDiameter: 150,
    commissioningDate: "15/Mar/2024",
    manufacturer: "Vestas",
  },
  {
    id: "wt-06",
    deviceName: "WTG-06",
    deviceModel: "SG 5.0-145",
    serialNumber: "SGE-2024-03201",
    status: "normal",
    yieldToday: 52.6,
    activePower: 4380,
    ratedPower: 5000,
    windFarm: "NORTH WIND FARM",
    windSpeed: 12.1,
    windDirection: 230,
    ambientTemp: 7.9,
    rotorSpeed: 11.5,
    generatorSpeed: 1210,
    generatorPower: 4410,
    reactivePower: 160,
    powerFactor: 0.99,
    gridFrequency: 50.01,
    gridVoltage: 692,
    pitchAngles: [1.5, 1.6, 1.4],
    nacellePosition: 228,
    yawError: 1.0,
    generatorTemp: 86,
    gearboxOilTemp: 62,
    mainBearingTemp: 48,
    controlCabinetTemp: 34,
    totalYield: 22100,
    totalOperatingHours: 13400,
    dailyOperatingTime: 20.1,
    hubHeight: 110,
    rotorDiameter: 145,
    commissioningDate: "01/Apr/2024",
    manufacturer: "Siemens Gamesa",
  },
  {
    id: "wt-07",
    deviceName: "WTG-07",
    deviceModel: "SG 5.0-145",
    serialNumber: "SGE-2024-03202",
    status: "offline",
    yieldToday: null,
    activePower: null,
    ratedPower: 5000,
    windFarm: "NORTH WIND FARM",
    windSpeed: null,
    windDirection: null,
    ambientTemp: null,
    rotorSpeed: null,
    generatorSpeed: null,
    generatorPower: null,
    reactivePower: null,
    powerFactor: null,
    gridFrequency: null,
    gridVoltage: null,
    pitchAngles: [null, null, null],
    nacellePosition: null,
    yawError: null,
    generatorTemp: null,
    gearboxOilTemp: null,
    mainBearingTemp: null,
    controlCabinetTemp: null,
    totalYield: 20800,
    totalOperatingHours: 13000,
    dailyOperatingTime: null,
    hubHeight: 110,
    rotorDiameter: 145,
    commissioningDate: "01/Apr/2024",
    manufacturer: "Siemens Gamesa",
  },
  {
    id: "wt-08",
    deviceName: "WTG-08",
    deviceModel: "SG 5.0-145",
    serialNumber: "SGE-2024-03203",
    status: "normal",
    yieldToday: 50.8,
    activePower: 4250,
    ratedPower: 5000,
    windFarm: "NORTH WIND FARM",
    windSpeed: 11.8,
    windDirection: 232,
    ambientTemp: 7.8,
    rotorSpeed: 11.3,
    generatorSpeed: 1200,
    generatorPower: 4280,
    reactivePower: 150,
    powerFactor: 0.99,
    gridFrequency: 50.0,
    gridVoltage: 691,
    pitchAngles: [1.7, 1.8, 1.6],
    nacellePosition: 230,
    yawError: 1.3,
    generatorTemp: 85,
    gearboxOilTemp: 61,
    mainBearingTemp: 47,
    controlCabinetTemp: 33,
    totalYield: 21500,
    totalOperatingHours: 13250,
    dailyOperatingTime: 19.5,
    hubHeight: 110,
    rotorDiameter: 145,
    commissioningDate: "01/Apr/2024",
    manufacturer: "Siemens Gamesa",
  },
];

// --- Fault mock data ---

export const turbineFaults: TurbineFault[] = [
  {
    id: "wf-001",
    turbineId: "wt-07",
    code: "F-1101",
    name: "Generator Overtemperature",
    severity: "critical",
    status: "active",
    occurredAt: new Date(2026, 1, 9, 8, 45),
    recoveredAt: null,
    relatedAlarmId: "ALM-WT-001",
  },
  {
    id: "wf-002",
    turbineId: "wt-07",
    code: "F-1202",
    name: "Communication Loss",
    severity: "major",
    status: "active",
    occurredAt: new Date(2026, 1, 9, 8, 42),
    recoveredAt: null,
    relatedAlarmId: "ALM-WT-002",
  },
  {
    id: "wf-003",
    turbineId: "wt-04",
    code: "W-0801",
    name: "Gearbox Oil Temperature High",
    severity: "warning",
    status: "active",
    occurredAt: new Date(2026, 1, 9, 10, 15),
    recoveredAt: null,
    relatedAlarmId: "ALM-WT-003",
  },
  {
    id: "wf-004",
    turbineId: "wt-04",
    code: "W-0605",
    name: "Yaw Error Exceeded",
    severity: "warning",
    status: "active",
    occurredAt: new Date(2026, 1, 9, 9, 30),
    recoveredAt: null,
    relatedAlarmId: "ALM-WT-004",
  },
  {
    id: "wf-005",
    turbineId: "wt-01",
    code: "W-0312",
    name: "Pitch System Warning",
    severity: "warning",
    status: "recovered",
    occurredAt: new Date(2026, 1, 8, 14, 20),
    recoveredAt: new Date(2026, 1, 8, 14, 35),
    relatedAlarmId: "ALM-WT-005",
  },
  {
    id: "wf-006",
    turbineId: "wt-02",
    code: "F-0905",
    name: "Grid Voltage Deviation",
    severity: "major",
    status: "recovered",
    occurredAt: new Date(2026, 1, 8, 11, 10),
    recoveredAt: new Date(2026, 1, 8, 11, 15),
    relatedAlarmId: "ALM-WT-006",
  },
  {
    id: "wf-007",
    turbineId: "wt-06",
    code: "W-0410",
    name: "Vibration Level High",
    severity: "minor",
    status: "recovered",
    occurredAt: new Date(2026, 1, 7, 16, 45),
    recoveredAt: new Date(2026, 1, 7, 17, 0),
    relatedAlarmId: "ALM-WT-007",
  },
  {
    id: "wf-008",
    turbineId: "wt-03",
    code: "F-1005",
    name: "Main Bearing Temperature High",
    severity: "major",
    status: "recovered",
    occurredAt: new Date(2026, 1, 7, 9, 20),
    recoveredAt: new Date(2026, 1, 7, 9, 50),
    relatedAlarmId: "ALM-WT-008",
  },
  {
    id: "wf-009",
    turbineId: "wt-05",
    code: "W-0215",
    name: "Rotor Speed Imbalance",
    severity: "minor",
    status: "recovered",
    occurredAt: new Date(2026, 1, 6, 13, 30),
    recoveredAt: new Date(2026, 1, 6, 13, 42),
    relatedAlarmId: "ALM-WT-009",
  },
  {
    id: "wf-010",
    turbineId: "wt-08",
    code: "F-0701",
    name: "Converter Fault",
    severity: "critical",
    status: "recovered",
    occurredAt: new Date(2026, 1, 5, 7, 15),
    recoveredAt: new Date(2026, 1, 5, 7, 45),
    relatedAlarmId: "ALM-WT-010",
  },
  {
    id: "wf-011",
    turbineId: "wt-01",
    code: "W-0118",
    name: "Blade Ice Detection",
    severity: "minor",
    status: "recovered",
    occurredAt: new Date(2026, 1, 4, 5, 30),
    recoveredAt: new Date(2026, 1, 4, 8, 0),
    relatedAlarmId: "ALM-WT-011",
  },
  {
    id: "wf-012",
    turbineId: "wt-06",
    code: "F-0503",
    name: "Hydraulic System Pressure Low",
    severity: "major",
    status: "recovered",
    occurredAt: new Date(2026, 1, 3, 14, 10),
    recoveredAt: new Date(2026, 1, 3, 14, 30),
    relatedAlarmId: "ALM-WT-012",
  },
];

export function getTurbineById(id: string): WindTurbine | undefined {
  return windTurbines.find((t) => t.id === id);
}

export function getFaultsForTurbine(turbineId: string): TurbineFault[] {
  return turbineFaults.filter((f) => f.turbineId === turbineId);
}

// --- Curve mock data (time-series for today, 5-min intervals) ---

function generateCurveData(basePower: number, baseWindSpeed: number): TurbineCurveData {
  const power: TurbineCurveData["power"] = [];
  const windSpeedData: TurbineCurveData["windSpeed"] = [];
  const pitch: TurbineCurveData["pitch"] = [];
  const temperature: TurbineCurveData["temperature"] = [];

  const now = new Date();
  const currentHour = now.getHours();
  const endMinute = currentHour <= 0 ? 0 : currentHour * 60 + now.getMinutes();

  let ws = baseWindSpeed;

  for (let m = 0; m <= Math.min(endMinute, 1440); m += 5) {
    const h = Math.floor(m / 60);
    const min = m % 60;
    const time = `${String(h).padStart(2, "0")}:${String(min).padStart(2, "0")}`;

    // Random walk wind speed
    ws += (Math.random() - 0.5) * 1.2;
    ws = Math.max(3, Math.min(25, ws));
    const currentWs = +ws.toFixed(1);

    // Power correlated to wind speed (cubic relation, capped at rated)
    const cubeRatio = Math.pow(currentWs / 12, 3);
    const rawPower = basePower * Math.min(1, cubeRatio);
    const activePow = +(rawPower * (1 + (Math.random() - 0.5) * 0.06)).toFixed(1);
    const reactivePow = +(20 + Math.random() * 30).toFixed(1);

    power.push({ time, activePower: Math.max(0, activePow), windSpeed: currentWs, reactivePower: reactivePow });

    // Rotor speed correlated to wind speed
    const rotorSpd = +(Math.min(12, currentWs * 0.95) + (Math.random() - 0.5) * 0.3).toFixed(1);
    windSpeedData.push({ time, windSpeed: currentWs, rotorSpeed: Math.max(0, rotorSpd) });

    // Pitch: increases when wind > rated to limit power
    const pitchAngle = currentWs > 12 ? +((currentWs - 12) * 3 + Math.random() * 1.5).toFixed(1) : +(1 + Math.random() * 2).toFixed(1);
    pitch.push({ time, pitchAngle, activePower: Math.max(0, activePow) });

    // Temperatures: slow-varying
    const hourFactor = Math.sin((m / 1440) * Math.PI);
    temperature.push({
      time,
      generator: +(75 + hourFactor * 15 + (Math.random() - 0.5) * 3).toFixed(1),
      gearboxOil: +(52 + hourFactor * 12 + (Math.random() - 0.5) * 2).toFixed(1),
      mainBearing: +(40 + hourFactor * 10 + (Math.random() - 0.5) * 2).toFixed(1),
    });
  }

  return { power, windSpeed: windSpeedData, pitch, temperature };
}

const curveCache = new Map<string, TurbineCurveData>();

export function getCurveDataForTurbine(turbineId: string): TurbineCurveData {
  if (curveCache.has(turbineId)) return curveCache.get(turbineId)!;

  const turbine = windTurbines.find((t) => t.id === turbineId);
  const basePower = turbine?.activePower ?? 0;
  const baseWind = turbine?.windSpeed ?? 10;

  const data = generateCurveData(basePower, baseWind);
  curveCache.set(turbineId, data);
  return data;
}
