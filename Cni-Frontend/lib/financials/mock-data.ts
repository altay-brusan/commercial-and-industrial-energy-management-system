import type {
  FinancialKPI,
  RevenueStream,
  CostItem,
  TariffPeriod,
  RealTimePricing,
  PeakDemandPoint,
  AssetFinancial,
  Transaction,
  MonthlyPnL,
  MonthlyRevenueTrend,
  MonthlyCostTrend,
  BudgetVsActual,
  ProjectionScenario,
  HourlyPriceProfile,
  Currency,
} from "./types";

// --- Constants ---

export const USD_TO_TRY = 36.5;

export function formatCurrency(amount: number, currency: Currency): string {
  if (currency === "TRY") {
    const tryAmount = amount * USD_TO_TRY;
    return `${tryAmount >= 0 ? "" : "-"}₺${Math.abs(tryAmount).toLocaleString("en-US", { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
  }
  return `${amount >= 0 ? "" : "-"}$${Math.abs(amount).toLocaleString("en-US", { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
}

export function formatCurrencyCompact(amount: number, currency: Currency): string {
  const val = currency === "TRY" ? amount * USD_TO_TRY : amount;
  const abs = Math.abs(val);
  const sign = val < 0 ? "-" : "";
  const symbol = currency === "TRY" ? "₺" : "$";
  if (abs >= 1_000_000) return `${sign}${symbol}${(abs / 1_000_000).toFixed(1)}M`;
  if (abs >= 1_000) return `${sign}${symbol}${(abs / 1_000).toFixed(1)}K`;
  return `${sign}${symbol}${abs.toFixed(0)}`;
}

// --- KPI ---

export const financialKPI: FinancialKPI = {
  totalRevenue: 127450,
  totalCost: 48230,
  netIncome: 79220,
  grossMargin: 62.2,
  portfolioLCOE: 0.038,
  roi: 14.8,
};

// --- Revenue Streams ---

export const revenueStreams: RevenueStream[] = [
  {
    type: "energy_arbitrage",
    label: "Energy Arbitrage",
    monthlyAmount: 8500,
    previousMonth: 7800,
    ytdAmount: 16300,
    description: "BESS charge/discharge spread revenue from TOU price differentials",
  },
  {
    type: "demand_response",
    label: "Demand Response",
    monthlyAmount: 16000,
    previousMonth: 14500,
    ytdAmount: 30500,
    description: "Grid operator demand response program participation payments",
  },
  {
    type: "capacity_market",
    label: "Capacity Market",
    monthlyAmount: 16500,
    previousMonth: 16500,
    ytdAmount: 33000,
    description: "Capacity availability payments for grid reliability commitment",
  },
  {
    type: "ancillary_services",
    label: "Ancillary Services",
    monthlyAmount: 15000,
    previousMonth: 13200,
    ytdAmount: 28200,
    description: "Frequency regulation, spinning reserve, and voltage support services",
  },
  {
    type: "feed_in_tariff",
    label: "Feed-in Tariff",
    monthlyAmount: 55000,
    previousMonth: 52800,
    ytdAmount: 107800,
    description: "Guaranteed purchase price for solar and wind energy generation",
  },
  {
    type: "rec_carbon",
    label: "REC & Carbon Credits",
    monthlyAmount: 16450,
    previousMonth: 15800,
    ytdAmount: 32250,
    description: "Renewable energy certificates and voluntary carbon credit sales",
  },
];

// --- Cost Items ---

export const costItems: CostItem[] = [
  { category: "operations", label: "Operations", monthlyAmount: 12800, budgeted: 13000, variance: -1.5 },
  { category: "maintenance", label: "Maintenance", monthlyAmount: 8950, budgeted: 8500, variance: 5.3 },
  { category: "grid_charges", label: "Grid Charges", monthlyAmount: 9200, budgeted: 9000, variance: 2.2 },
  { category: "depreciation", label: "Depreciation", monthlyAmount: 11500, budgeted: 11500, variance: 0.0 },
  { category: "insurance", label: "Insurance", monthlyAmount: 3280, budgeted: 3280, variance: 0.0 },
  { category: "admin", label: "Administration", monthlyAmount: 2500, budgeted: 2800, variance: -10.7 },
];

// --- Tariff Periods ---

export const tariffPeriods: TariffPeriod[] = [
  { name: "Off-Peak", hours: "22:00 - 06:00", rate: 0.045, rateTRY: 1.64 },
  { name: "Mid-Peak", hours: "06:00 - 11:00, 17:00 - 22:00", rate: 0.082, rateTRY: 2.99 },
  { name: "On-Peak", hours: "11:00 - 14:00, 15:00 - 17:00", rate: 0.145, rateTRY: 5.29 },
  { name: "Super-Peak", hours: "14:00 - 15:00", rate: 0.21, rateTRY: 7.67 },
];

// --- 24h Price Profile ---

export function getHourlyPriceProfile(): HourlyPriceProfile[] {
  const profile: HourlyPriceProfile[] = [];
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`;
    let rate: number;
    let period: string;
    if (h >= 22 || h < 6) {
      rate = 0.045;
      period = "Off-Peak";
    } else if ((h >= 6 && h < 11) || (h >= 17 && h < 22)) {
      rate = 0.082;
      period = "Mid-Peak";
    } else if (h === 14) {
      rate = 0.21;
      period = "Super-Peak";
    } else {
      rate = 0.145;
      period = "On-Peak";
    }
    profile.push({ hour, rate, period });
  }
  return profile;
}

// --- Real-Time Pricing (last 24h) ---

export function getRealTimePricing(): RealTimePricing[] {
  const data: RealTimePricing[] = [];
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`;
    let base: number;
    if (h >= 22 || h < 6) base = 32;
    else if ((h >= 6 && h < 11) || (h >= 17 && h < 22)) base = 58;
    else if (h === 14) base = 145;
    else base = 95;
    const noise = (Math.random() - 0.5) * 15;
    data.push({ hour, price: +(base + noise).toFixed(1) });
  }
  return data;
}

// --- Peak Demand ---

export function getPeakDemandData(): PeakDemandPoint[] {
  const data: PeakDemandPoint[] = [];
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`;
    let demand: number;
    if (h >= 0 && h < 6) demand = 2200 + Math.random() * 300;
    else if (h >= 6 && h < 9) demand = 3500 + Math.random() * 500;
    else if (h >= 9 && h < 12) demand = 5200 + Math.random() * 800;
    else if (h >= 12 && h < 15) demand = 6500 + Math.random() * 1000;
    else if (h >= 15 && h < 18) demand = 5800 + Math.random() * 700;
    else if (h >= 18 && h < 22) demand = 4500 + Math.random() * 600;
    else demand = 2800 + Math.random() * 400;

    const shaved = demand > 5000 ? demand - 5000 + Math.random() * 200 : 0;
    data.push({
      hour,
      demand: +demand.toFixed(0),
      shaved: +shaved.toFixed(0),
    });
  }
  return data;
}

// --- Asset Financial Performance ---

export const assetFinancials: AssetFinancial[] = [
  {
    type: "solar",
    name: "Solar Farm A",
    capacity: "5 MWp",
    capacityFactor: 22.5,
    monthlyProduction: 825,
    monthlyRevenue: 55000,
    monthlyCost: 14200,
    netIncome: 40800,
    lcoe: 0.032,
    roi: 18.5,
    investment: 4200000,
    npv: 2850000,
    irr: 16.2,
    paybackYears: 5.4,
  },
  {
    type: "bess",
    name: "BESS Unit 1",
    capacity: "10 MW / 40 MWh",
    capacityFactor: 45.0,
    monthlyProduction: 432,
    monthlyRevenue: 40000,
    monthlyCost: 18500,
    netIncome: 21500,
    lcoe: 0.045,
    roi: 12.2,
    investment: 6800000,
    npv: 1920000,
    irr: 11.8,
    paybackYears: 7.2,
  },
  {
    type: "wind",
    name: "Wind Park B",
    capacity: "8 MW",
    capacityFactor: 32.8,
    monthlyProduction: 1935,
    monthlyRevenue: 28450,
    monthlyCost: 12030,
    netIncome: 16420,
    lcoe: 0.035,
    roi: 13.6,
    investment: 9600000,
    npv: 3150000,
    irr: 13.1,
    paybackYears: 6.8,
  },
  {
    type: "grid",
    name: "Grid Interconnect",
    capacity: "15 MVA",
    capacityFactor: 68.2,
    monthlyProduction: 0,
    monthlyRevenue: 4000,
    monthlyCost: 3500,
    netIncome: 500,
    lcoe: 0,
    roi: 2.1,
    investment: 1200000,
    npv: 180000,
    irr: 3.5,
    paybackYears: 18.5,
  },
];

// --- Monthly P&L (last 12 months) ---

export const monthlyPnL: MonthlyPnL[] = [
  { month: "Mar", revenue: 118200, cost: 46500, netIncome: 71700, margin: 60.7 },
  { month: "Apr", revenue: 124500, cost: 47200, netIncome: 77300, margin: 62.1 },
  { month: "May", revenue: 132800, cost: 47800, netIncome: 85000, margin: 64.0 },
  { month: "Jun", revenue: 141200, cost: 48100, netIncome: 93100, margin: 65.9 },
  { month: "Jul", revenue: 145600, cost: 49200, netIncome: 96400, margin: 66.2 },
  { month: "Aug", revenue: 142300, cost: 49500, netIncome: 92800, margin: 65.2 },
  { month: "Sep", revenue: 135800, cost: 48800, netIncome: 87000, margin: 64.1 },
  { month: "Oct", revenue: 128400, cost: 48200, netIncome: 80200, margin: 62.5 },
  { month: "Nov", revenue: 121600, cost: 47600, netIncome: 74000, margin: 60.9 },
  { month: "Dec", revenue: 115800, cost: 47100, netIncome: 68700, margin: 59.3 },
  { month: "Jan", revenue: 119200, cost: 47800, netIncome: 71400, margin: 59.9 },
  { month: "Feb", revenue: 127450, cost: 48230, netIncome: 79220, margin: 62.2 },
];

// --- Revenue Trend (monthly per stream) ---

export const monthlyRevenueTrend: MonthlyRevenueTrend[] = [
  { month: "Mar", energy_arbitrage: 7200, demand_response: 13500, capacity_market: 16500, ancillary_services: 12800, feed_in_tariff: 52500, rec_carbon: 15700 },
  { month: "Apr", energy_arbitrage: 7500, demand_response: 14200, capacity_market: 16500, ancillary_services: 13200, feed_in_tariff: 56800, rec_carbon: 16300 },
  { month: "May", energy_arbitrage: 8200, demand_response: 15000, capacity_market: 16500, ancillary_services: 14100, feed_in_tariff: 62100, rec_carbon: 16900 },
  { month: "Jun", energy_arbitrage: 9100, demand_response: 16800, capacity_market: 16500, ancillary_services: 15200, feed_in_tariff: 66200, rec_carbon: 17400 },
  { month: "Jul", energy_arbitrage: 9800, demand_response: 17500, capacity_market: 16500, ancillary_services: 15800, feed_in_tariff: 68500, rec_carbon: 17500 },
  { month: "Aug", energy_arbitrage: 9500, demand_response: 17200, capacity_market: 16500, ancillary_services: 15500, feed_in_tariff: 66800, rec_carbon: 16800 },
  { month: "Sep", energy_arbitrage: 8800, demand_response: 16200, capacity_market: 16500, ancillary_services: 14800, feed_in_tariff: 62800, rec_carbon: 16700 },
  { month: "Oct", energy_arbitrage: 8200, demand_response: 15500, capacity_market: 16500, ancillary_services: 14200, feed_in_tariff: 57500, rec_carbon: 16500 },
  { month: "Nov", energy_arbitrage: 7500, demand_response: 14800, capacity_market: 16500, ancillary_services: 13500, feed_in_tariff: 53200, rec_carbon: 16100 },
  { month: "Dec", energy_arbitrage: 7000, demand_response: 14000, capacity_market: 16500, ancillary_services: 12800, feed_in_tariff: 50000, rec_carbon: 15500 },
  { month: "Jan", energy_arbitrage: 7800, demand_response: 14500, capacity_market: 16500, ancillary_services: 13200, feed_in_tariff: 51400, rec_carbon: 15800 },
  { month: "Feb", energy_arbitrage: 8500, demand_response: 16000, capacity_market: 16500, ancillary_services: 15000, feed_in_tariff: 55000, rec_carbon: 16450 },
];

// --- Cost Trend (monthly per category) ---

export const monthlyCostTrend: MonthlyCostTrend[] = [
  { month: "Mar", operations: 12200, maintenance: 8200, grid_charges: 8800, depreciation: 11500, insurance: 3280, admin: 2520 },
  { month: "Apr", operations: 12400, maintenance: 8400, grid_charges: 8900, depreciation: 11500, insurance: 3280, admin: 2620 },
  { month: "May", operations: 12500, maintenance: 8600, grid_charges: 9000, depreciation: 11500, insurance: 3280, admin: 2920 },
  { month: "Jun", operations: 12700, maintenance: 8800, grid_charges: 9100, depreciation: 11500, insurance: 3280, admin: 2720 },
  { month: "Jul", operations: 13000, maintenance: 9200, grid_charges: 9300, depreciation: 11500, insurance: 3280, admin: 2920 },
  { month: "Aug", operations: 13100, maintenance: 9400, grid_charges: 9400, depreciation: 11500, insurance: 3280, admin: 2820 },
  { month: "Sep", operations: 12900, maintenance: 9100, grid_charges: 9200, depreciation: 11500, insurance: 3280, admin: 2820 },
  { month: "Oct", operations: 12700, maintenance: 8800, grid_charges: 9100, depreciation: 11500, insurance: 3280, admin: 2820 },
  { month: "Nov", operations: 12500, maintenance: 8500, grid_charges: 9000, depreciation: 11500, insurance: 3280, admin: 2820 },
  { month: "Dec", operations: 12300, maintenance: 8300, grid_charges: 8900, depreciation: 11500, insurance: 3280, admin: 2820 },
  { month: "Jan", operations: 12500, maintenance: 8600, grid_charges: 9000, depreciation: 11500, insurance: 3280, admin: 2920 },
  { month: "Feb", operations: 12800, maintenance: 8950, grid_charges: 9200, depreciation: 11500, insurance: 3280, admin: 2500 },
];

// --- Budget vs Actual ---

export const budgetVsActual: BudgetVsActual[] = [
  { month: "Mar", budgeted: 47000, actual: 46500 },
  { month: "Apr", budgeted: 47000, actual: 47200 },
  { month: "May", budgeted: 47500, actual: 47800 },
  { month: "Jun", budgeted: 47500, actual: 48100 },
  { month: "Jul", budgeted: 48000, actual: 49200 },
  { month: "Aug", budgeted: 48000, actual: 49500 },
  { month: "Sep", budgeted: 48000, actual: 48800 },
  { month: "Oct", budgeted: 48000, actual: 48200 },
  { month: "Nov", budgeted: 48000, actual: 47600 },
  { month: "Dec", budgeted: 48000, actual: 47100 },
  { month: "Jan", budgeted: 48500, actual: 47800 },
  { month: "Feb", budgeted: 48500, actual: 48230 },
];

// --- Transactions (50 records) ---

const txTypes: { type: Transaction["type"]; assets: string[]; descs: string[] }[] = [
  { type: "energy_sale", assets: ["Solar Farm A", "Wind Park B"], descs: ["Energy export to grid", "Spot market sale", "PPA delivery"] },
  { type: "energy_purchase", assets: ["BESS Unit 1", "Grid Interconnect"], descs: ["Off-peak charging", "Grid import for storage"] },
  { type: "demand_response", assets: ["BESS Unit 1", "Solar Farm A"], descs: ["DR event participation", "Peak shaving service", "Emergency curtailment"] },
  { type: "capacity_payment", assets: ["BESS Unit 1", "Wind Park B"], descs: ["Monthly capacity payment", "Availability premium"] },
  { type: "ancillary_service", assets: ["BESS Unit 1"], descs: ["Frequency regulation", "Spinning reserve", "Voltage support"] },
  { type: "rec_sale", assets: ["Solar Farm A", "Wind Park B"], descs: ["REC batch sale Q1", "Green certificate transfer"] },
  { type: "carbon_credit", assets: ["Solar Farm A", "Wind Park B"], descs: ["VCU credit sale", "Carbon offset delivery"] },
  { type: "maintenance_cost", assets: ["Solar Farm A", "BESS Unit 1", "Wind Park B"], descs: ["Scheduled maintenance", "Inverter replacement", "Blade inspection"] },
  { type: "grid_charge", assets: ["Grid Interconnect"], descs: ["Transmission charge", "Connection fee", "Wheeling charge"] },
];

const txStatuses: Transaction["status"][] = ["completed", "completed", "completed", "pending", "invoiced", "overdue"];

function generateTransactions(): Transaction[] {
  const transactions: Transaction[] = [];
  const baseDate = new Date(2026, 1, 10);

  for (let i = 0; i < 50; i++) {
    const txGroup = txTypes[i % txTypes.length];
    const date = new Date(baseDate);
    date.setDate(date.getDate() - i);

    const isRevenue = !["energy_purchase", "maintenance_cost", "grid_charge"].includes(txGroup.type);
    const baseQty = isRevenue ? 50 + Math.random() * 200 : 20 + Math.random() * 100;
    const basePrice = isRevenue ? 40 + Math.random() * 80 : 30 + Math.random() * 50;
    const amount = +(baseQty * basePrice * (isRevenue ? 1 : -1)).toFixed(2);

    transactions.push({
      id: `TXN-${(1000 + i).toString()}`,
      date,
      type: txGroup.type,
      asset: txGroup.assets[i % txGroup.assets.length],
      description: txGroup.descs[i % txGroup.descs.length],
      quantity: +baseQty.toFixed(1),
      unitPrice: +basePrice.toFixed(2),
      amount,
      status: txStatuses[i % txStatuses.length],
      reference: `REF-${(2026000 + i).toString()}`,
      currency: "USD",
    });
  }

  return transactions;
}

export const transactions: Transaction[] = generateTransactions();

// --- Projection Scenarios ---

function generateProjection(
  baseName: string,
  revenueMultiplier: number,
  costMultiplier: number,
  npv: number,
  irr: number,
  payback: number,
): ProjectionScenario {
  const months: ProjectionScenario["months"] = [];
  const futureMonths = ["Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb"];
  let cumulative = 0;

  for (let i = 0; i < 12; i++) {
    const seasonFactor = 1 + 0.15 * Math.sin(((i + 2) / 12) * Math.PI * 2);
    const revenue = +(127450 * revenueMultiplier * seasonFactor).toFixed(0);
    const cost = +(48230 * costMultiplier).toFixed(0);
    const netIncome = revenue - cost;
    cumulative += netIncome;

    months.push({
      month: futureMonths[i],
      revenue,
      cost,
      netIncome,
      cumulative: +cumulative.toFixed(0),
    });
  }

  return { name: baseName, months, npv, irr, paybackYears: payback };
}

export const projectionScenarios: ProjectionScenario[] = [
  generateProjection("Base", 1.0, 1.0, 4250000, 14.8, 6.2),
  generateProjection("Optimistic", 1.15, 0.95, 5800000, 18.5, 5.1),
  generateProjection("Pessimistic", 0.85, 1.08, 2400000, 9.2, 8.4),
];

// --- Revenue breakdown for pie chart ---

export function getRevenueBreakdown(): { name: string; value: number; fill: string }[] {
  const colors = [
    "var(--color-energy-battery)",
    "var(--color-energy-consumption)",
    "var(--color-energy-grid)",
    "var(--color-primary)",
    "var(--color-energy-solar)",
    "var(--color-chart-5)",
  ];
  return revenueStreams.map((s, i) => ({
    name: s.label,
    value: s.monthlyAmount,
    fill: colors[i % colors.length],
  }));
}

// --- Arbitrage profile (BESS charge/discharge with price overlay) ---

export function getArbitrageProfile(): { hour: string; chargeDischarge: number; price: number }[] {
  const data: { hour: string; chargeDischarge: number; price: number }[] = [];
  for (let h = 0; h < 24; h++) {
    const hour = `${h.toString().padStart(2, "0")}:00`;
    let chargeDischarge: number;
    let price: number;

    if (h >= 0 && h < 6) {
      chargeDischarge = -(2 + Math.random() * 3); // charging (negative)
      price = 32 + Math.random() * 8;
    } else if (h >= 6 && h < 11) {
      chargeDischarge = Math.random() * 1 - 0.5;
      price = 55 + Math.random() * 15;
    } else if (h >= 11 && h < 15) {
      chargeDischarge = 3 + Math.random() * 5; // discharging (positive)
      price = 95 + Math.random() * 50;
    } else if (h >= 15 && h < 18) {
      chargeDischarge = 2 + Math.random() * 4;
      price = 80 + Math.random() * 30;
    } else if (h >= 18 && h < 22) {
      chargeDischarge = 1 + Math.random() * 3;
      price = 60 + Math.random() * 20;
    } else {
      chargeDischarge = -(1 + Math.random() * 2);
      price = 35 + Math.random() * 10;
    }

    data.push({
      hour,
      chargeDischarge: +chargeDischarge.toFixed(1),
      price: +price.toFixed(1),
    });
  }
  return data;
}

// --- Transaction helpers ---

export function getTransactionSummary() {
  const totalRevenue = transactions.filter((t) => t.amount > 0).reduce((s, t) => s + t.amount, 0);
  const totalExpense = transactions.filter((t) => t.amount < 0).reduce((s, t) => s + t.amount, 0);
  return {
    count: transactions.length,
    totalRevenue: +totalRevenue.toFixed(2),
    totalExpense: +totalExpense.toFixed(2),
    net: +(totalRevenue + totalExpense).toFixed(2),
  };
}
