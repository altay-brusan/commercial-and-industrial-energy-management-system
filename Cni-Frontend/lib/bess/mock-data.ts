import type {
  BessUnit,
  BessRack,
  BessPack,
  BessModule,
  BessCell,
  BessFault,
  BessCurveData,
  BessComponentStatus,
  BatteryFaultFlag,
  BatteryFaultCategory,
  ChillerAlarmFlag,
  ChillerFaultCategory,
  BessChillerState,
  BessHvcuState,
  BessEsc,
  BessHvac,
  BessDeviceTreeNode,
  BessUnitSettings,
  BessRackSettings,
  BessEscSettings,
  BessHvacSettings,
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
    live: (() => {
      const allVoltages = allCells.map((c) => c.voltage * 1000); // mV
      const maxVIdx = allVoltages.indexOf(Math.max(...allVoltages));
      const minVIdx = allVoltages.indexOf(Math.min(...allVoltages));
      const maxTIdx = allTemps.indexOf(Math.max(...allTemps));
      const minTIdx = allTemps.indexOf(Math.min(...allTemps));
      const cellLocFromIdx = (idx: number) => ({
        bmuLoc: Math.floor(idx / spec.cellsPerModule) + 1,
        cellLoc: (idx % spec.cellsPerModule) + 1,
      });
      return {
        soc: avgSoc,
        soh: avgSoh,
        power: spec.mode === "charging" ? -Math.abs(totalPower) : spec.mode === "standby" ? 0 : Math.abs(totalPower),
        voltage: totalVoltage,
        current: avgCurrent,
        frequency: 50,
        tempAvg,
        tempMin: round(Math.min(...allTemps), 1),
        tempMax: round(Math.max(...allTemps), 1),
        remainingEnergyKwh: round(spec.capacityKWh * (avgSoc / 100), 1),
        availableEnergyKwh: round(spec.capacityKWh * (avgSoc / 100) * 0.95, 1),
        dmccA: round(spec.maxPowerKW * 1000 / totalVoltage * 0.9, 1),
        dmdcA: round(spec.maxPowerKW * 1000 / totalVoltage * 0.85, 1),
        contactorPositive: spec.status !== "offline",
        contactorNegative: spec.status !== "offline",
        isolationStatus: round(500 + rand() * 300, 0),
        cellVoltageMaxMv: round(Math.max(...allVoltages), 0),
        cellVoltageMinMv: round(Math.min(...allVoltages), 0),
        avgCellVoltageMv: round(allVoltages.reduce((s, v) => s + v, 0) / allVoltages.length, 0),
        cellVoltageMaxLoc: cellLocFromIdx(maxVIdx),
        cellVoltageMinLoc: cellLocFromIdx(minVIdx),
        cellTempMaxC: round(Math.max(...allTemps), 1),
        cellTempMinC: round(Math.min(...allTemps), 1),
        cellTempMaxLoc: cellLocFromIdx(maxTIdx),
        cellTempMinLoc: cellLocFromIdx(minTIdx),
        outVoltPosV: round(totalVoltage / 2 + rand() * 2, 1),
        outVoltNegV: round(-(totalVoltage / 2 + rand() * 2), 1),
        stateCode: spec.status === "online" ? 3 : spec.status === "standby" ? 1 : spec.status === "fault" ? 7 : 0,
      };
    })(),
    lifecycle: (() => {
      const cycleCount = Math.floor(1000 + spec.seed * 2.5);
      const designedCycleLife = spec.chemistry === "LFP" ? 6000 : 4000;
      const equivalentFullCycles = Math.floor(cycleCount * (0.7 + rand() * 0.2));
      const averageDoD = round(55 + rand() * 25, 1);
      const initialRTE = spec.chemistry === "LFP" ? 95.5 : 93.0;
      const rte = round(initialRTE - (cycleCount / designedCycleLife) * 5 - rand() * 1.5, 1);
      const baseSoh = avgSoh;
      const sohTrend: number[] = [];
      for (let i = 11; i >= 0; i--) {
        sohTrend.push(round(baseSoh + i * 0.15 + (rand() - 0.5) * 0.3, 1));
      }
      const lifetimeCharged = round(spec.capacityKWh * equivalentFullCycles * 0.001, 1);
      const lifetimeDischarged = round(lifetimeCharged * (rte / 100), 1);
      const commYear = spec.id === "bess-03" ? 2024 : 2024;
      const commMonth = spec.id === "bess-03" ? 3 : 1;

      return {
        cycleCount,
        equivalentFullCycles,
        designedCycleLife,
        designedCalendarLifeYears: spec.chemistry === "LFP" ? 15 : 12,
        averageDoD,
        sohTrendMonthly: sohTrend,
        lifetimeChargedMWh: lifetimeCharged,
        lifetimeDischargedMWh: lifetimeDischarged,
        dailyChargedKWh: round(38 + rand() * 20, 1),
        dailyDischargedKWh: round(42 + rand() * 25, 1),
        roundTripEfficiency: rte,
        initialRoundTripEfficiency: initialRTE,
        totalOperatingHours: Math.floor(8000 + spec.seed * 10 + rand() * 2000),
        totalStandbyHours: Math.floor(2000 + rand() * 1500),
        lastMaintenanceDate: "15/Dec/2025",
        nextMaintenanceDue: commMonth <= 3 ? "15/Mar/2026" : "15/Jun/2026",
        warrantyExpiryDate: `${10 + commMonth}/Jan/${commYear + (spec.chemistry === "LFP" ? 10 : 8)}`,
        firmwareVersion: spec.id === "bess-03" ? "v3.2.1" : "v4.1.0",
        maxRecordedTemp: round(42 + rand() * 8, 1),
        thermalExcursionCount: Math.floor(rand() * 12),
        avgOperatingTemp: round(26 + rand() * 6, 1),
      };
    })(),
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

// --- Battery Fault Flags ---
function generateBatteryFaultFlags(seed: number): BatteryFaultFlag[] {
  const rand = seededRandom(seed + 5000);
  const defs: { key: string; label: string; category: BatteryFaultCategory; severity: "danger" | "warning" }[] = [
    { key: "faultPackWarning", label: "Pack Warning", category: "pack", severity: "warning" },
    { key: "faultPackDanger", label: "Pack Danger", category: "pack", severity: "danger" },
    { key: "faultPackOverVoltage", label: "Pack Over Voltage", category: "pack", severity: "danger" },
    { key: "faultPackUnderVoltage", label: "Pack Under Voltage", category: "pack", severity: "danger" },
    { key: "faultPackOverCurrent", label: "Pack Over Current", category: "pack", severity: "danger" },
    { key: "faultPackOverTemp", label: "Pack Over Temperature", category: "pack", severity: "danger" },
    { key: "faultPackUnderTemp", label: "Pack Under Temperature", category: "pack", severity: "warning" },
    { key: "faultPackSocHigh", label: "Pack SoC High", category: "pack", severity: "warning" },
    { key: "faultPackSocLow", label: "Pack SoC Low", category: "pack", severity: "warning" },
    { key: "faultPackInsulationLow", label: "Pack Insulation Low", category: "pack", severity: "danger" },
    { key: "faultPackVoltageDiff", label: "Pack Voltage Difference", category: "pack", severity: "warning" },
    { key: "faultPackTempDiff", label: "Pack Temperature Difference", category: "pack", severity: "warning" },
    { key: "faultCellOverVoltage", label: "Cell Over Voltage", category: "cell", severity: "danger" },
    { key: "faultCellUnderVoltage", label: "Cell Under Voltage", category: "cell", severity: "danger" },
    { key: "faultCellOverTemp", label: "Cell Over Temperature", category: "cell", severity: "danger" },
    { key: "faultCellUnderTemp", label: "Cell Under Temperature", category: "cell", severity: "warning" },
    { key: "faultCellVoltageDiffHigh", label: "Cell Voltage Diff High", category: "cell", severity: "warning" },
    { key: "faultCellTempDiffHigh", label: "Cell Temp Diff High", category: "cell", severity: "warning" },
    { key: "faultCellBalanceFail", label: "Cell Balance Failure", category: "cell", severity: "warning" },
    { key: "faultCellSohLow", label: "Cell SoH Low", category: "cell", severity: "warning" },
    { key: "faultModuleCommLoss", label: "Module Communication Loss", category: "module", severity: "danger" },
    { key: "faultModuleOverVoltage", label: "Module Over Voltage", category: "module", severity: "danger" },
    { key: "faultModuleUnderVoltage", label: "Module Under Voltage", category: "module", severity: "danger" },
    { key: "faultModuleOverTemp", label: "Module Over Temperature", category: "module", severity: "danger" },
    { key: "faultModuleSensorFail", label: "Module Sensor Failure", category: "module", severity: "warning" },
    { key: "faultModuleVoltageDiff", label: "Module Voltage Difference", category: "module", severity: "warning" },
    { key: "faultCanCommWarning", label: "CAN Communication Warning", category: "communication", severity: "warning" },
    { key: "faultCanCommFault", label: "CAN Communication Fault", category: "communication", severity: "danger" },
    { key: "faultBmuCommTimeout", label: "BMU Communication Timeout", category: "communication", severity: "danger" },
    { key: "faultBmsCommLoss", label: "BMS Communication Loss", category: "communication", severity: "danger" },
    { key: "faultModbusCommFail", label: "Modbus Communication Fail", category: "communication", severity: "warning" },
    { key: "faultEmsCommLoss", label: "EMS Communication Loss", category: "communication", severity: "warning" },
    { key: "faultPcsCommLoss", label: "PCS Communication Loss", category: "communication", severity: "warning" },
    { key: "faultBoardOverTemp", label: "Board Over Temperature", category: "board", severity: "danger" },
    { key: "faultBoardPowerFail", label: "Board Power Supply Failure", category: "board", severity: "danger" },
    { key: "faultBoardWatchdog", label: "Board Watchdog Fault", category: "board", severity: "danger" },
    { key: "faultBoardEepromFail", label: "Board EEPROM Failure", category: "board", severity: "warning" },
    { key: "faultBoardFlashFail", label: "Board Flash Memory Failure", category: "board", severity: "warning" },
    { key: "faultBoardAdcFail", label: "Board ADC Failure", category: "board", severity: "warning" },
    { key: "faultBoardRtcFail", label: "Board RTC Failure", category: "board", severity: "warning" },
    { key: "faultPackContactorFail", label: "Pack Contactor Failure", category: "pack", severity: "danger" },
    { key: "faultPackFuseBlown", label: "Pack Fuse Blown", category: "pack", severity: "danger" },
    { key: "faultPackPreChargeFail", label: "Pack Pre-charge Failure", category: "pack", severity: "danger" },
    { key: "faultCellInternalShort", label: "Cell Internal Short Circuit", category: "cell", severity: "danger" },
    { key: "faultCellOpenCircuit", label: "Cell Open Circuit", category: "cell", severity: "danger" },
    { key: "faultModuleCurrentSensorFail", label: "Module Current Sensor Fail", category: "module", severity: "warning" },
    { key: "faultBoardCanBusOff", label: "Board CAN Bus Off", category: "board", severity: "danger" },
    { key: "faultPackEmergencyStop", label: "Pack Emergency Stop", category: "pack", severity: "danger" },
    { key: "faultCellReversePolarity", label: "Cell Reverse Polarity", category: "cell", severity: "danger" },
    { key: "faultModuleTempSensorOpen", label: "Module Temp Sensor Open", category: "module", severity: "warning" },
  ];

  return defs.map((d) => ({
    ...d,
    active: rand() < 0.06, // ~3 active on average
  }));
}

// --- Chiller Alarm Flags ---
function generateChillerAlarmFlags(seed: number): ChillerAlarmFlag[] {
  const rand = seededRandom(seed + 6000);
  const defs: { key: string; label: string; category: ChillerFaultCategory }[] = [
    { key: "chillerOutletTempSensorFault", label: "Outlet Temp Sensor Fault", category: "sensor" },
    { key: "chillerInletTempSensorFault", label: "Inlet Temp Sensor Fault", category: "sensor" },
    { key: "chillerAmbientTempSensorFault", label: "Ambient Temp Sensor Fault", category: "sensor" },
    { key: "chillerCoolantFlowSensorFault", label: "Coolant Flow Sensor Fault", category: "sensor" },
    { key: "chillerPressureSensorFault", label: "Pressure Sensor Fault", category: "sensor" },
    { key: "chillerPowerSupplyFault", label: "Power Supply Fault", category: "power" },
    { key: "chillerPhaseSequenceFault", label: "Phase Sequence Fault", category: "power" },
    { key: "chillerOverVoltageFault", label: "Over Voltage Fault", category: "power" },
    { key: "chillerUnderVoltageFault", label: "Under Voltage Fault", category: "power" },
    { key: "chillerSystemFault", label: "System General Fault", category: "system" },
    { key: "chillerControllerFault", label: "Controller Fault", category: "system" },
    { key: "chillerEepromFault", label: "EEPROM Fault", category: "system" },
    { key: "chillerPumpOverloadFault", label: "Pump Overload Fault", category: "pump" },
    { key: "chillerPumpDryRunFault", label: "Pump Dry Run Fault", category: "pump" },
    { key: "chillerPumpBlockedFault", label: "Pump Blocked Fault", category: "pump" },
    { key: "chillerPumpLeakFault", label: "Pump Leak Fault", category: "pump" },
    { key: "chillerHighPressureFault", label: "High Pressure Fault", category: "pressure" },
    { key: "chillerLowPressureFault", label: "Low Pressure Fault", category: "pressure" },
    { key: "chillerPressureDiffFault", label: "Pressure Difference Fault", category: "pressure" },
    { key: "chillerHighTempFault", label: "High Temperature Fault", category: "temperature" },
    { key: "chillerLowTempFault", label: "Low Temperature Fault", category: "temperature" },
    { key: "chillerTempDiffFault", label: "Temperature Difference Fault", category: "temperature" },
    { key: "chillerFreezeProtectionFault", label: "Freeze Protection Fault", category: "temperature" },
    { key: "chillerCommTimeoutFault", label: "Communication Timeout", category: "communication" },
    { key: "chillerModbusCommFault", label: "Modbus Communication Fault", category: "communication" },
    { key: "chillerCanCommFault", label: "CAN Communication Fault", category: "communication" },
    { key: "chillerCompVfdOverTempFault", label: "Compressor VFD Over Temp", category: "compressor_vfd" },
    { key: "chillerCompVfdOverCurrentFault", label: "Compressor VFD Over Current", category: "compressor_vfd" },
    { key: "chillerCompVfdCommFault", label: "Compressor VFD Comm Fault", category: "compressor_vfd" },
    { key: "chillerCompVfdPhaseFault", label: "Compressor VFD Phase Fault", category: "compressor_vfd" },
    { key: "chillerRemoteLockFault", label: "Remote Lock Fault", category: "lock" },
    { key: "chillerLocalLockFault", label: "Local Lock Fault", category: "lock" },
    { key: "chillerSafetyInterlock", label: "Safety Interlock", category: "lock" },
    { key: "chillerEmergencyStop", label: "Emergency Stop", category: "lock" },
  ];

  return defs.map((d) => ({
    ...d,
    active: rand() < 0.04, // ~1-2 active on average
  }));
}

// --- Chiller State ---
function generateChillerState(seed: number): BessChillerState {
  const rand = seededRandom(seed + 7000);
  const modes = ["Cooling", "Heating", "Standby", "Auto"];
  const modeIdx = Math.floor(rand() * modes.length);
  return {
    operatingMode: modeIdx,
    operatingModeStr: modes[modeIdx],
    outletCoolantTempC: round(18 + rand() * 8, 1),
    inletCoolantTempC: round(22 + rand() * 10, 1),
    ambientTempC: round(20 + rand() * 15, 1),
    inletCoolantPressure: round(1.5 + rand() * 1.5, 2),
    outletCoolantPressure: round(1.2 + rand() * 1.2, 2),
    compressorStatus: rand() > 0.3,
    pumpStatus: rand() > 0.2,
    fanRunStatus: rand() > 0.4,
    electricHeaterStatus: rand() < 0.15,
    compressorSpeed: Math.floor(rand() * 100),
    pumpSpeed: Math.floor(40 + rand() * 60),
  };
}

// --- HVCU State ---
function generateHvcuState(seed: number): BessHvcuState {
  const rand = seededRandom(seed + 8000);
  const stateStrs = ["Standby", "Pre-charge", "Active", "Shutdown", "Fault"];
  const bcuCount = 2 + Math.floor(rand() * 3);
  const bcuStates = Array.from({ length: bcuCount }, (_, i) => {
    const stateIdx = i === 0 ? 2 : Math.floor(rand() * stateStrs.length);
    return {
      id: i + 1,
      controlState: stateIdx,
      stateStr: stateStrs[stateIdx],
    };
  });
  return {
    bcuStates,
    isolationControl: rand() > 0.3,
  };
}

// --- Per-unit fault/chiller/hvcu maps ---
const batteryFaultFlagMap = new Map<string, BatteryFaultFlag[]>();
const chillerAlarmFlagMap = new Map<string, ChillerAlarmFlag[]>();
const chillerStateMap = new Map<string, BessChillerState>();
const hvcuStateMap = new Map<string, BessHvcuState>();

unitSpecs.forEach((spec) => {
  batteryFaultFlagMap.set(spec.id, generateBatteryFaultFlags(spec.seed));
  chillerAlarmFlagMap.set(spec.id, generateChillerAlarmFlags(spec.seed));
  chillerStateMap.set(spec.id, generateChillerState(spec.seed));
  hvcuStateMap.set(spec.id, generateHvcuState(spec.seed));
});

export function getBatteryFaultFlagsForUnit(unitId: string): BatteryFaultFlag[] {
  return batteryFaultFlagMap.get(unitId) ?? [];
}

export function getChillerAlarmFlagsForUnit(unitId: string): ChillerAlarmFlag[] {
  return chillerAlarmFlagMap.get(unitId) ?? [];
}

export function getChillerStateForUnit(unitId: string): BessChillerState | undefined {
  return chillerStateMap.get(unitId);
}

export function getHvcuStateForUnit(unitId: string): BessHvcuState | undefined {
  return hvcuStateMap.get(unitId);
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

// --- ESC (DC-DC Controller) generation ---

function generateEsc(unitId: string, seed: number): BessEsc {
  const rand = seededRandom(seed + 9000);
  const escId = `${unitId}-ESC-1`;
  const operatingStates = ["Running", "Standby", "Pre-charge", "Shutdown"];
  const stateIdx = Math.floor(rand() * operatingStates.length);
  const hasAlarm = rand() < 0.15;

  return {
    id: escId,
    unitId,
    deviceInfo: {
      model: "ESC-3000D",
      serialNumber: `SN-ESC-${unitId.toUpperCase()}-${seed}`,
      firmware: "v2.4.1",
      protocol: "Modbus TCP",
      modbusAddress: 10 + Math.floor(rand() * 5),
    },
    live: {
      dcVoltageIn: round(750 + rand() * 100, 1),
      dcVoltageOut: round(700 + rand() * 80, 1),
      dcCurrent: round(50 + rand() * 150, 1),
      power: round(40 + rand() * 60, 1),
      efficiency: round(95 + rand() * 4, 1),
      temperature: round(30 + rand() * 20, 1),
      isolationResistance: round(400 + rand() * 400, 0),
      operatingState: operatingStates[stateIdx],
    },
    status: hasAlarm ? "warning" : stateIdx === 3 ? "offline" : "normal",
    activeAlarmCount: hasAlarm ? 1 : 0,
  };
}

// --- HVAC (Thermal Management) generation ---

function generateHvacs(unitId: string, seed: number, count: number): BessHvac[] {
  const hvacs: BessHvac[] = [];
  for (let i = 0; i < count; i++) {
    const hvacSeed = seed + 10000 + i * 100;
    const rand = seededRandom(hvacSeed);
    const hvacId = `${unitId}-HVAC-${i + 1}`;
    const modes = ["Cooling", "Heating", "Standby", "Auto"];
    const modeIdx = Math.floor(rand() * modes.length);
    const hasAlarm = rand() < 0.1;

    hvacs.push({
      id: hvacId,
      unitId,
      deviceInfo: {
        model: i === 0 ? "TMS-5000A" : "TMS-5000B",
        serialNumber: `SN-HVAC-${unitId.toUpperCase()}-${hvacSeed}`,
        firmware: "v1.8.3",
        protocol: "Modbus RTU",
        modbusAddress: 20 + i,
      },
      chillerState: {
        operatingMode: modeIdx,
        operatingModeStr: modes[modeIdx],
        outletCoolantTempC: round(18 + rand() * 8, 1),
        inletCoolantTempC: round(22 + rand() * 10, 1),
        ambientTempC: round(20 + rand() * 15, 1),
        inletCoolantPressure: round(1.5 + rand() * 1.5, 2),
        outletCoolantPressure: round(1.2 + rand() * 1.2, 2),
        compressorStatus: rand() > 0.3,
        pumpStatus: rand() > 0.2,
        fanRunStatus: rand() > 0.4,
        electricHeaterStatus: rand() < 0.15,
        compressorSpeed: Math.floor(rand() * 100),
        pumpSpeed: Math.floor(40 + rand() * 60),
      },
      setpoints: {
        coolantTempSetpoint: 22,
        highAlarm: 35,
        lowAlarm: 5,
        operatingMode: modes[modeIdx],
      },
      status: hasAlarm ? "warning" : "normal",
      activeAlarmCount: hasAlarm ? 1 : 0,
    });
  }
  return hvacs;
}

// --- Per-unit ESC/HVAC maps ---

const escMap = new Map<string, BessEsc>();
const hvacMap = new Map<string, BessHvac[]>();

unitSpecs.forEach((spec) => {
  escMap.set(spec.id, generateEsc(spec.id, spec.seed));
  const hvacCount = spec.rackCount >= 4 ? 2 : 1;
  hvacMap.set(spec.id, generateHvacs(spec.id, spec.seed, hvacCount));
});

export function getEscForUnit(unitId: string): BessEsc | undefined {
  return escMap.get(unitId);
}

export function getHvacsForUnit(unitId: string): BessHvac[] {
  return hvacMap.get(unitId) ?? [];
}

export function getEscById(escId: string): BessEsc | undefined {
  for (const esc of escMap.values()) {
    if (esc.id === escId) return esc;
  }
  return undefined;
}

export function getHvacById(hvacId: string): BessHvac | undefined {
  for (const hvacs of hvacMap.values()) {
    const found = hvacs.find((h) => h.id === hvacId);
    if (found) return found;
  }
  return undefined;
}

// --- ESC/HVAC faults (appended to bessFaults) ---

const escHvacFaults: Omit<BessFault, "id">[] = [
  {
    unitId: "bess-01",
    code: "ESC-0101",
    name: "DC Bus Over Voltage",
    severity: "major",
    status: "recovered",
    source: "ESC-1",
    occurredAt: new Date("2025-02-07T10:22:00"),
    recoveredAt: new Date("2025-02-07T10:35:00"),
    relatedAlarmId: "alarm-013",
  },
  {
    unitId: "bess-02",
    code: "HVAC-0201",
    name: "Compressor Over Temperature",
    severity: "warning",
    status: "active",
    source: "HVAC-1",
    occurredAt: new Date("2025-02-09T07:45:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-014",
  },
  {
    unitId: "bess-01",
    code: "HVAC-0301",
    name: "Coolant Flow Low",
    severity: "minor",
    status: "recovered",
    source: "HVAC-2",
    occurredAt: new Date("2025-02-06T14:10:00"),
    recoveredAt: new Date("2025-02-06T14:25:00"),
    relatedAlarmId: "alarm-015",
  },
  {
    unitId: "bess-03",
    code: "ESC-0201",
    name: "Isolation Resistance Low",
    severity: "warning",
    status: "active",
    source: "ESC-1",
    occurredAt: new Date("2025-02-09T01:30:00"),
    recoveredAt: null,
    relatedAlarmId: "alarm-016",
  },
];

// Append ESC/HVAC faults to main fault list
escHvacFaults.forEach((f, i) => {
  bessFaults.push({
    ...f,
    id: `bess-fault-${String(baseFaults.length + i + 1).padStart(3, "0")}`,
  });
});

// --- Device Tree builder ---

export function buildDeviceTree(): BessDeviceTreeNode[] {
  return bessUnits.map((unit) => {
    const children: BessDeviceTreeNode[] = [];

    // Racks
    unit.racks.forEach((rack) => {
      const rackAlarms = rack.packs.reduce(
        (sum, p) =>
          sum +
          p.modules.reduce(
            (ms, m) => ms + m.cells.filter((c) => c.status === "fault" || c.status === "warning").length,
            0
          ),
        0
      );
      children.push({
        id: rack.id,
        name: `Rack ${rack.rackNumber}`,
        type: "rack",
        status: rack.status,
        parentUnitId: unit.id,
        alarmCount: rackAlarms,
      });
    });

    // ESC
    const esc = escMap.get(unit.id);
    if (esc) {
      children.push({
        id: esc.id,
        name: "ESC-1",
        type: "esc",
        status: esc.status,
        parentUnitId: unit.id,
        alarmCount: esc.activeAlarmCount,
      });
    }

    // HVACs
    const hvacs = hvacMap.get(unit.id) ?? [];
    hvacs.forEach((hvac, i) => {
      children.push({
        id: hvac.id,
        name: `HVAC-${i + 1}`,
        type: "hvac",
        status: hvac.status,
        parentUnitId: unit.id,
        alarmCount: hvac.activeAlarmCount,
      });
    });

    const unitStatus: BessComponentStatus =
      unit.status === "online" ? "normal" :
      unit.status === "fault" ? "fault" :
      unit.status === "standby" ? "normal" :
      "offline";

    return {
      id: unit.id,
      name: unit.name,
      type: "unit" as const,
      status: unitStatus,
      parentUnitId: unit.id,
      children,
      alarmCount: unit.activeAlarmCount,
    };
  });
}

// --- Settings defaults ---

export function getDefaultUnitSettings(unitId: string): BessUnitSettings {
  const unit = getBessUnitById(unitId);
  return {
    deviceName: unit?.name ?? "Unknown",
    protocol: "Modbus TCP",
    firmwareVersion: unit?.lifecycle.firmwareVersion ?? "v0.0.0",
    serialNumber: unit?.nameplate.serialNumber ?? "",
    modbusAddress: 1,
    maintenanceIntervalDays: 90,
    tempAlarmThreshold: 45,
    socLowAlarm: 10,
    socHighAlarm: 95,
  };
}

export function getDefaultRackSettings(): BessRackSettings {
  return {
    socCalibrationIntervalHours: 168,
    chargeStopSoc: 95,
    dischargeStopSoc: 10,
    operatingMode: "auto",
    insulationResistanceThreshold: 500,
    balancingEnabled: true,
    balancingThresholdMv: 30,
  };
}

export function getDefaultEscSettings(escId: string): BessEscSettings {
  const esc = getEscById(escId);
  return {
    deviceName: "ESC-1",
    model: esc?.deviceInfo.model ?? "ESC-3000D",
    serialNumber: esc?.deviceInfo.serialNumber ?? "",
    firmware: esc?.deviceInfo.firmware ?? "",
    protocol: esc?.deviceInfo.protocol ?? "Modbus TCP",
    modbusAddress: esc?.deviceInfo.modbusAddress ?? 10,
    isolationResistanceThreshold: 500,
    maxPowerLimit: 100,
    autoRestart: true,
  };
}

export function getDefaultHvacSettings(hvacId: string): BessHvacSettings {
  const hvac = getHvacById(hvacId);
  return {
    operatingMode: hvac?.setpoints.operatingMode ?? "Auto",
    coolantTempSetpoint: hvac?.setpoints.coolantTempSetpoint ?? 22,
    highTempAlarm: hvac?.setpoints.highAlarm ?? 35,
    lowTempAlarm: hvac?.setpoints.lowAlarm ?? 5,
    compressorMaxSpeed: 100,
    pumpMaxSpeed: 100,
    autoDefrost: true,
  };
}

// --- Helper: get rack by ID ---

export function getRackById(rackId: string): { rack: BessRack; unit: BessUnit } | undefined {
  for (const unit of bessUnits) {
    const rack = unit.racks.find((r) => r.id === rackId);
    if (rack) return { rack, unit };
  }
  return undefined;
}
