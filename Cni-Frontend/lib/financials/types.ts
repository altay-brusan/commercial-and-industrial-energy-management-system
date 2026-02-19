export type Currency = "USD" | "TRY";

// KPI
export interface FinancialKPI {
  totalRevenue: number;
  totalCost: number;
  netIncome: number;
  grossMargin: number;
  portfolioLCOE: number;
  roi: number;
}

// Revenue
export type RevenueStreamType =
  | "energy_arbitrage"
  | "demand_response"
  | "capacity_market"
  | "ancillary_services"
  | "feed_in_tariff"
  | "rec_carbon";

export interface RevenueStream {
  type: RevenueStreamType;
  label: string;
  monthlyAmount: number;
  previousMonth: number;
  ytdAmount: number;
  description: string;
}

// Cost
export type CostCategory =
  | "operations"
  | "maintenance"
  | "grid_charges"
  | "depreciation"
  | "insurance"
  | "admin";

export interface CostItem {
  category: CostCategory;
  label: string;
  monthlyAmount: number;
  budgeted: number;
  variance: number;
}

// Tariff
export interface TariffPeriod {
  name: string;
  hours: string;
  rate: number;
  rateTRY: number;
}

export interface RealTimePricing {
  hour: string;
  price: number;
}

export interface PeakDemandPoint {
  hour: string;
  demand: number;
  shaved: number;
}

// Asset Financial Performance
export type AssetType = "solar" | "bess" | "wind" | "grid";

export interface AssetFinancial {
  type: AssetType;
  name: string;
  capacity: string;
  capacityFactor: number;
  monthlyProduction: number;
  monthlyRevenue: number;
  monthlyCost: number;
  netIncome: number;
  lcoe: number;
  roi: number;
  investment: number;
  npv: number;
  irr: number;
  paybackYears: number;
}

// Transactions
export type TransactionType =
  | "energy_sale"
  | "energy_purchase"
  | "demand_response"
  | "capacity_payment"
  | "ancillary_service"
  | "rec_sale"
  | "carbon_credit"
  | "maintenance_cost"
  | "grid_charge";

export type TransactionStatus = "completed" | "pending" | "invoiced" | "overdue";

export interface Transaction {
  id: string;
  date: Date;
  type: TransactionType;
  asset: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  status: TransactionStatus;
  reference: string;
  currency: Currency;
}

export type TransactionSortField =
  | "date"
  | "type"
  | "asset"
  | "amount"
  | "status";

export type SortDirection = "asc" | "desc";

// Monthly P&L
export interface MonthlyPnL {
  month: string;
  revenue: number;
  cost: number;
  netIncome: number;
  margin: number;
}

// Revenue trend (monthly per stream)
export interface MonthlyRevenueTrend {
  month: string;
  energy_arbitrage: number;
  demand_response: number;
  capacity_market: number;
  ancillary_services: number;
  feed_in_tariff: number;
  rec_carbon: number;
}

// Cost trend (monthly per category)
export interface MonthlyCostTrend {
  month: string;
  operations: number;
  maintenance: number;
  grid_charges: number;
  depreciation: number;
  insurance: number;
  admin: number;
}

// Budget vs Actual
export interface BudgetVsActual {
  month: string;
  budgeted: number;
  actual: number;
}

// Projection
export interface ProjectionMonth {
  month: string;
  revenue: number;
  cost: number;
  netIncome: number;
  cumulative: number;
}

export interface ProjectionScenario {
  name: string;
  months: ProjectionMonth[];
  npv: number;
  irr: number;
  paybackYears: number;
}

// 24h price profile
export interface HourlyPriceProfile {
  hour: string;
  rate: number;
  period: string;
}
