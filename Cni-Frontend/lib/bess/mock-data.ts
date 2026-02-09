import type {
  BessUnit,
  BessRack,
  BessPack,
  BessModule,
  BessCell,
  BessFault,
  BessCurveData,
  BessComponentStatus,
} from "./types";

// --- Seeded random for deterministic data ---
function seededRandom(seed: number) {
  let s = seed;
  return () => {
    s = (s * 16807 + 0) % 2147483647;
    return (s - 1) / 2147483646;
  };
}

// Gaussian from uniform (Box-Muller)
function gaussian(rand: () => number, mean: number, stddev: number): number {
  const u1 = rand();
  const u2 = rand();
  const z = Math.sqrt(-2 * Math.log(u1)) * Math.cos(2 * Math.PI * u2);
  return mean + z * stddev;
}

function clamp(v: number, min: number, max: number) {
  return Math.max(min, Math.min(max, v));
}

function round(v: number, d = 2) {
  const f = 10 ** d;
  return Math.round(v * f) / f;
}

// --- Hierarchy generator ---
interface UnitSpec {
  id: string;
  name: string;
  status: BessUnit["status"];
  mode: BessUnit["operatingMode"];
  chemistry: "LFP" | "NMC";
  capacityKWh: number;
  maxPowerKW: number;
  rackCount: number;
  packsPerRack: number;
  modulesPerPack: number;
  cellsPerModule: number;
  seed: number;
  // Fault injection
  faultRackIndex?: number;
  warningPackIndices?: [number, number][];
}

const unitSpecs: UnitSpec[] = [
  {
    id: "bess-01",
    name: "Megapack Alpha",
    status: "online",
    mode: "discharging",
    chemistry: "LFP",
    capacityKWh: 500,
    maxPowerKW: 250,
    rackCount: 4,
    packsPerRack: 4,
    modulesPerPack: 8,
    cellsPerModule: 14,
    seed: 42,
    warningPackIndices: [[1, 2]],
  },
  {
    id: "bess-02",
    name: "Megapack Beta",
    status: "online",
    mode: "charging",
    chemistry: "LFP",
    capacityKWh: 500,
    maxPowerKW: 250,
    rackCount: 4,
    packsPerRack: 4,
    modulesPerPack: 8,
    cellsPerModule: 14,
    seed: 137,
    warningPackIndices: [[2, 1]],
  },
  {
    id: "bess-03",
    name: "PowerStore Gamma",
    status: "standby",
    mode: "standby",
    chemistry: "NMC",
    capacityKWh: 250,
    maxPowerKW: 125,
    rackCount: 2,
    packsPerRack: 4,
    modulesPerPack: 8,
    cellsPerModule: 14,
    seed: 256,
    faultRackIndex: 1,
  },
];

function generateCells(
  spec: UnitSpec,
  rackIdx: number,
  packIdx: number,
  moduleIdx: number,
  rand: () => number,
  isFaultModule: boolean,
  isWarningModule: boolean
): BessCell[] {
  const isLFP = spec.chemistry === "LFP";
  const nominalV = isLFP ? 3.3 : 3.7;
  const vMin = isLFP ? 3.2 : 3.5;
  const vMax = isLFP ? 3.65 : 4.2;
  const stddev = 0.02;

  const cells: BessCell[] = [];
  for (let c = 0; c < spec.cellsPerModule; c++) {
    const cellId = `${spec.id}-R${rackIdx + 1}-P${packIdx + 1}-M${moduleIdx + 1}-C${c + 1}`;
    let voltage = round(clamp(gaussian(rand, nominalV, stddev), vMin, vMax), 3);
    let temperature = round(clamp(gaussian(rand, 28, 3), 18, 45), 1);
    let soh = round(clamp(gaussian(rand, 96, 2), 80, 100), 1);
    let soc = round(clamp(gaussian(rand, 72, 8), 10, 100), 1);
    let status: BessComponentStatus = "normal";
    let internalResistance = round(clamp(gaussian(rand, 1.2, 0.15), 0.8, 3.0), 2);
    const balancing = rand() < 0.05;

    // Weak cell injection (~1 in 50)
    if (rand() < 0.02) {
      voltage = round(clamp(voltage - 0.08, vMin, vMax), 3);
      soh = round(clamp(soh - 8, 75, 100), 1);
      internalResistance = round(internalResistance + 0.5, 2);
      status = "warning";
    }

    // Fault module → one cell in fault
    if (isFaultModule && c === 7) {
      voltage = round(vMin - 0.05, 3);
      temperature = round(clamp(temperature + 12, 18, 55), 1);
      soh = 72;
      status = "fault";
      internalResistance = round(internalResistance + 1.5, 2);
    }

    // Warning module → slightly elevated temps
    if (isWarningModule && c >= 10) {
      temperature = round(clamp(temperature + 5, 18, 42), 1);
      if (status === "normal") status = "warning";
    }

    cells.push({
      id: cellId,
      cellNumber: c + 1,
      status,
      voltage,
      current: round(clamp(gaussian(rand, spec.mode === "charging" ? -15 : 12, 2), -25, 25), 1),
      temperature,
      soc,
      soh,
      internalResistance,
      balancing,
      cycleCount: Math.floor(clamp(gaussian(rand, 1200, 150), 800, 2000)),
    });
  }
  return cells;
}

function rollupStatus(statuses: BessComponentStatus[]): BessComponentStatus {
  if (statuses.includes("fault")) return "fault";
  if (statuses.includes("warning")) return "warning";
  if (statuses.includes("offline")) return "offline";
  return "normal";
}

function generateUnit(spec: UnitSpec): BessUnit {
  const rand = seededRandom(spec.seed);

  const racks: BessRack[] = [];
  for (let r = 0; r < spec.rackCount; r++) {
    const isFaultRack = spec.faultRackIndex === r;
    const packs: BessPack[] = [];

    for (let p = 0; p < spec.packsPerRack; p++) {
      const isWarningPack = spec.warningPackIndices?.some(([ri, pi]) => ri === r && pi === p) ?? false;
      const modules: BessModule[] = [];

      for (let m = 0; m < spec.modulesPerPack; m++) {
        const isFaultModule = isFaultRack && p === 2 && m === 3;
        const isWarningModule = isWarningPack && m === 5;
        const cells = generateCells(spec, r, p, m, rand, isFaultModule, isWarningModule);

        const modVoltage = round(cells.reduce((s, c) => s + c.voltage, 0), 2);
        const modCurrent = round(cells.reduce((s, c) => s + c.current, 0) / cells.length, 1);
        const modSoc = round(cells.reduce((s, c) => s + c.soc, 0) / cells.length, 1);
        const modSoh = round(cells.reduce((s, c) => s + c.soh, 0) / cells.length, 1);
        const temps = cells.map((c) => c.temperature);

        modules.push({
          id: `${spec.id}-R${r + 1}-P${p + 1}-M${m + 1}`,
          moduleNumber: m + 1,
          status: rollupStatus(cells.map((c) => c.status)),
          voltage: modVoltage,
          current: modCurrent,
          soc: modSoc,
          soh: modSoh,
          tempMin: round(Math.min(...temps), 1),
          tempMax: round(Math.max(...temps), 1),
          tempAvg: round(temps.reduce((s, t) => s + t, 0) / temps.length, 1),
          cells,
        });
      }

      const packVoltage = round(modules.reduce((s, m) => s + m.voltage, 0), 2);
      const packCurrent = round(modules.reduce((s, m) => s + m.current, 0) / modules.length, 1);
      const packSoc = round(modules.reduce((s, m) => s + m.soc, 0) / modules.length, 1);
      const packSoh = round(modules.reduce((s, m) => s + m.soh, 0) / modules.length, 1);
      const packTemps = modules.flatMap((m) => m.cells.map((c) => c.temperature));

      packs.push({
        id: `${spec.id}-R${r + 1}-P${p + 1}`,
        packNumber: p + 1,
        status: rollupStatus(modules.map((m) => m.status)),
        voltage: packVoltage,
        current: packCurrent,
        soc: packSoc,
        soh: packSoh,
        tempMin: round(Math.min(...packTemps), 1),
        tempMax: round(Math.max(...packTemps), 1),
        tempAvg: round(packTemps.reduce((s, t) => s + t, 0) / packTemps.length, 1),
        modules,
      });
    }

    const rackVoltage = round(packs.reduce((s, p) => s + p.voltage, 0), 2);
    const rackCurrent = round(packs.reduce((s, p) => s + p.current, 0) / packs.length, 1);
    const rackSoc = round(packs.reduce((s, p) => s + p.soc, 0) / packs.length, 1);
    const rackSoh = round(packs.reduce((s, p) => s + p.soh, 0) / packs.length, 1);
    const rackTemps = packs.flatMap((p) => p.modules.flatMap((m) => m.cells.map((c) => c.temperature)));
    const rackPower = round(rackVoltage * (rackCurrent / 1000), 1);

    racks.push({
      id: `${spec.id}-R${r + 1}`,
      rackNumber: r + 1,
      status: rollupStatus(packs.map((p) => p.status)),
      voltage: rackVoltage,
      current: rackCurrent,
      soc: rackSoc,
      soh: rackSoh,
      power: rackPower,
      tempMin: round(Math.min(...rackTemps), 1),
      tempMax: round(Math.max(...rackTemps), 1),
      tempAvg: round(rackTemps.reduce((s, t) => s + t, 0) / rackTemps.length, 1),
      packs,
    });
  }

  const allCells = racks.flatMap((r) => r.packs.flatMap((p) => p.modules.flatMap((m) => m.cells)));
  const avgSoc = round(allCells.reduce((s, c) => s + c.soc, 0) / allCells.length, 1);
  const avgSoh = round(allCells.reduce((s, c) => s + c.soh, 0) / allCells.length, 1);
  const totalVoltage = round(racks.reduce((s, r) => s + r.voltage, 0) / racks.length, 1);
  const avgCurrent = round(racks.reduce((s, r) => s + r.current, 0) / racks.length, 1);
  const allTemps = allCells.map((c) => c.temperature);
  const tempAvg = round(allTemps.reduce((s, t) => s + t, 0) / allTemps.length, 1);
  const totalPower = round(racks.reduce((s, r) => s + r.power, 0), 1);
  const alarmCount = allCells.filter((c) => c.status === "fault" || c.status === "warning").length;

  return {
    id: spec.id,
    name: spec.name,
    status: spec.status,
    operatingMode: spec.mode,
    nameplate: {
      brand: spec.id === "bess-03" ? "Fluence" : "Tesla",
      model: spec.id === "bess-03" ? "Gridstack Pro" : "Megapack 2XL",
      serialNumber: `SN-${spec.id.toUpperCase()}-${spec.seed}`,
      chemistry: spec.chemistry,
      commissioningDate: spec.id === "bess-03" ? "15/Mar/2024" : "20/Jan/2024",
      capacityKWh: spec.capacityKWh,
      maxPowerKW: spec.maxPowerKW,
      nominalVoltage: spec.chemistry === "LFP" ? 768 : 880,
      voltageRange: spec.chemistry === "LFP" ? [614, 833] : [700, 1000],
      rackCount: spec.rackCount,
      packsPerRack: spec.packsPerRack,
      modulesPerPack: spec.modulesPerPack,
      cellsPerModule: spec.cellsPerModule,
    },
    live: {
      soc: avgSoc,
      soh: avgSoh,
      power: spec.mode === "charging" ? -Math.abs(totalPower) : spec.mode === "standby" ? 0 : Math.abs(totalPower),
      voltage: totalVoltage,
      current: avgCurrent,
      frequency: 50,
      tempAvg,
      tempMin: round(Math.min(...allTemps), 1),
      tempMax: round(Math.max(...allTemps), 1),
    },
    lifecycle: {
      cycleCount: Math.floor(1000 + spec.seed * 2.5),
      lifetimeThroughputMWh: round(spec.capacityKWh * (1000 + spec.seed * 2.5) * 0.0008, 1),
      dailyChargedKWh: round(38 + rand() * 20, 1),
      dailyDischargedKWh: round(42 + rand() * 25, 1),
    },
    connectedDevices: {
      inverter: `PCS-${spec.id.slice(-2).toUpperCase()}-01`,
      meter: `MTR-${spec.id.slice(-2).toUpperCase()}-01`,
    },
    activeAlarmCount: alarmCount,
    racks,
  };
}

// --- Generate units ---
export const bessUnits: BessUnit[] = unitSpecs.map(generateUnit);

// --- Accessor functions ---
export function getBessUnitById(id: string): BessUnit | undefined {
  return bessUnits.find((u) => u.id === id);
}

export function getRacksForUnit(unitId: string): BessRack[] {
  return getBessUnitById(unitId)?.racks ?? [];
}

export function getPacksForRack(unitId: string, rackId: string): BessPack[] {
  return getRacksForUnit(unitId).find((r) => r.id === rackId)?.packs ?? [];
}

export function getModulesForPack(unitId: string, rackId: string, packId: string): BessModule[] {
  return getPacksForRack(unitId, rackId).find((p) => p.id === packId)?.modules ?? [];
}

export function getCellsForModule(unitId: string, rackId: string, packId: string, moduleId: string): BessCell[] {
  return getModulesForPack(unitId, rackId, packId).find((m) => m.id === moduleId)?.cells ?? [];
}

// --- Aggregates ---
export function getTotalCapacity(): number {
  return bessUnits.reduce((s, u) => s + u.nameplate.capacityKWh, 0);
}

export function getAverageSoc(): number {
  if (bessUnits.length === 0) return 0;
  return round(bessUnits.reduce((s, u) => s + u.live.soc, 0) / bessUnits.length, 1);
}

export function getTotalActiveAlarms(): number {
  return bessUnits.reduce((s, u) => s + u.activeAlarmCount, 0);
}

// --- Faults ---
const baseFaults: Omit<BessFault, "id">[] = [
  {
    unitId: "bess-01",
    code: "BMS-0101",
    name: "Cell Over Voltage",
    severity: "major",
    status: "recovered",
    source: "Rack 1 / Pack 2 / Module 3",
    occurredAt: new Date("2025-02-08T14:32:00"),
    recoveredAt: new Date("2025-02-08T14:45:00"),
    relatedAlarmId: "alarm-001",
  },
  {
    unitId: "bess-01",
    code: "BMS-0202",
    name: "Cell Under Voltage",
    severity: "minor",
    status: "recovered",
    source: "Rack 2 / Pack 1 / Module 6",
    occurredAt: new Date("2025-02-07T09:15:00"),
    recoveredAt: new Date("2025-02-07T09:30:00"),
    relatedAlarmId: "alarm-002",
  },
  {
    unitId: "bess-01",
    code: "BMS-0305",
    name: "High Temperature Warning",
    severity: "warning",
    status: "active",
    source: "Rack 1 / Pack 3 / Module 5",
    occurredAt: new Date("2025-02-09T08:10:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-003",
  },
  {
    unitId: "bess-02",
    code: "BMS-0101",
    name: "Cell Over Voltage",
    severity: "major",
    status: "recovered",
    source: "Rack 3 / Pack 2 / Module 1",
    occurredAt: new Date("2025-02-06T16:20:00"),
    recoveredAt: new Date("2025-02-06T16:35:00"),
    relatedAlarmId: "alarm-004",
  },
  {
    unitId: "bess-02",
    code: "BMS-0410",
    name: "Communication Loss",
    severity: "minor",
    status: "recovered",
    source: "Rack 2 / Pack 4",
    occurredAt: new Date("2025-02-05T11:45:00"),
    recoveredAt: new Date("2025-02-05T11:50:00"),
    relatedAlarmId: "alarm-005",
  },
  {
    unitId: "bess-02",
    code: "BMS-0503",
    name: "SoC Imbalance Detected",
    severity: "warning",
    status: "active",
    source: "Rack 2 / Pack 1 / Module 7",
    occurredAt: new Date("2025-02-09T06:30:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-006",
  },
  {
    unitId: "bess-03",
    code: "BMS-0301",
    name: "Over Temperature Alarm",
    severity: "critical",
    status: "active",
    source: "Rack 2 / Pack 3 / Module 4",
    occurredAt: new Date("2025-02-09T03:12:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-007",
  },
  {
    unitId: "bess-03",
    code: "BMS-0102",
    name: "Pack Voltage Deviation",
    severity: "major",
    status: "active",
    source: "Rack 1 / Pack 2",
    occurredAt: new Date("2025-02-09T04:55:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-008",
  },
  {
    unitId: "bess-03",
    code: "BMS-0601",
    name: "Insulation Resistance Low",
    severity: "minor",
    status: "recovered",
    source: "Rack 2",
    occurredAt: new Date("2025-02-08T22:10:00"),
    recoveredAt: new Date("2025-02-08T22:40:00"),
    relatedAlarmId: "alarm-009",
  },
  {
    unitId: "bess-01",
    code: "BMS-0701",
    name: "Balancing Timeout",
    severity: "warning",
    status: "recovered",
    source: "Rack 3 / Pack 1 / Module 2",
    occurredAt: new Date("2025-02-06T02:30:00"),
    recoveredAt: new Date("2025-02-06T03:15:00"),
    relatedAlarmId: "alarm-010",
  },
  {
    unitId: "bess-03",
    code: "BMS-0302",
    name: "Temperature Sensor Fault",
    severity: "major",
    status: "recovered",
    source: "Rack 1 / Pack 4 / Module 8",
    occurredAt: new Date("2025-02-04T18:45:00"),
    recoveredAt: new Date("2025-02-04T19:20:00"),
    relatedAlarmId: "alarm-011",
  },
  {
    unitId: "bess-02",
    code: "BMS-0801",
    name: "Contactor Failure",
    severity: "critical",
    status: "recovered",
    source: "Rack 1",
    occurredAt: new Date("2025-02-03T10:00:00"),
    recoveredAt: new Date("2025-02-03T10:30:00"),
    relatedAlarmId: "alarm-012",
  },
];

export const bessFaults: BessFault[] = baseFaults.map((f, i) => ({
  ...f,
  id: `bess-fault-${String(i + 1).padStart(3, "0")}`,
}));

export function getFaultsForUnit(unitId: string): BessFault[] {
  return bessFaults.filter((f) => f.unitId === unitId);
}

// --- Curve data generation ---
function generateCurveData(spec: UnitSpec): BessCurveData {
  const rand = seededRandom(spec.seed + 1000);
  const points = 288; // 24h * 12 (5-min intervals)
  const power: BessCurveData["power"] = [];
  const voltage: BessCurveData["voltage"] = [];
  const temperature: BessCurveData["temperature"] = [];

  let soc = spec.mode === "charging" ? 25 : 85;

  for (let i = 0; i < points; i++) {
    const hour = i / 12;
    const hh = String(Math.floor(hour)).padStart(2, "0");
    const mm = String(Math.floor((hour % 1) * 60)).padStart(2, "0");
    const time = `${hh}:${mm}`;

    // Power pattern: charge at night (0-6), idle (6-8), discharge day (8-18), charge evening (18-24)
    let p = 0;
    if (hour < 6) p = -(spec.maxPowerKW * 0.6 + rand() * 20);
    else if (hour < 8) p = rand() * 10 - 5;
    else if (hour < 18) p = spec.maxPowerKW * 0.7 + rand() * 30;
    else p = -(spec.maxPowerKW * 0.5 + rand() * 15);

    if (spec.mode === "standby") p = rand() * 5 - 2.5;

    soc = clamp(soc - (p * 5) / (60 * spec.capacityKWh) * 100, 5, 98);

    power.push({ time, power: round(p, 1), soc: round(soc, 1) });

    const baseVoltage = spec.chemistry === "LFP" ? 768 : 880;
    const v = baseVoltage + (soc - 50) * 0.8 + gaussian(rand, 0, 2);
    const c = p !== 0 ? (p * 1000) / v : gaussian(rand, 0, 2);
    voltage.push({ time, voltage: round(v, 1), current: round(c, 1) });

    const baseTemp = 25 + Math.abs(p) / spec.maxPowerKW * 8;
    const tAvg = round(baseTemp + gaussian(rand, 0, 1), 1);
    const tMin = round(tAvg - 2 - rand() * 2, 1);
    const tMax = round(tAvg + 2 + rand() * 2, 1);
    temperature.push({ time, tempAvg: tAvg, tempMin: tMin, tempMax: tMax });
  }

  return { power, voltage, temperature };
}

const curveDataMap = new Map<string, BessCurveData>();
unitSpecs.forEach((spec) => {
  curveDataMap.set(spec.id, generateCurveData(spec));
});

export function getCurveDataForUnit(unitId: string): BessCurveData {
  return curveDataMap.get(unitId) ?? { power: [], voltage: [], temperature: [] };
}
