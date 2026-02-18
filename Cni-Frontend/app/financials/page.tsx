"use client";

import { useState } from "react";
import { TopBar } from "@/components/layout/top-bar";
import { FinancialsSidebar } from "@/components/financials/financials-sidebar";
import { FinancialKPIStrip } from "@/components/financials/financial-kpi-strip";
import { OverviewView } from "@/components/financials/overview-view";
import { RevenueView } from "@/components/financials/revenue-view";
import { CostView } from "@/components/financials/cost-view";
import { TariffView } from "@/components/financials/tariff-view";
import { AssetPerformanceView } from "@/components/financials/asset-performance-view";
import { TransactionsView } from "@/components/financials/transactions-view";
import { ProjectionsView } from "@/components/financials/projections-view";
import type { FinancialView } from "@/components/financials/financials-sidebar";
import type { Currency } from "@/lib/financials/types";

export default function FinancialsPage() {
  const [activeView, setActiveView] = useState<FinancialView>("overview");
  const [currency, setCurrency] = useState<Currency>("USD");

  const toggleCurrency = () => setCurrency((c) => (c === "USD" ? "TRY" : "USD"));

  const renderContent = () => {
    switch (activeView) {
      case "overview":
        return <OverviewView currency={currency} />;
      case "revenue":
        return <RevenueView currency={currency} />;
      case "cost":
        return <CostView currency={currency} />;
      case "tariff":
        return <TariffView currency={currency} />;
      case "asset-performance":
        return <AssetPerformanceView currency={currency} />;
      case "transactions":
        return <TransactionsView currency={currency} />;
      case "projections":
        return <ProjectionsView currency={currency} />;
    }
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="financials" />
      <div className="flex flex-1 overflow-hidden">
        <FinancialsSidebar activeItem={activeView} onItemChange={setActiveView} />
        <main className="flex-1 flex flex-col overflow-hidden">
          <FinancialKPIStrip currency={currency} onCurrencyToggle={toggleCurrency} />
          <div className="flex-1 overflow-y-auto">
            {renderContent()}
          </div>
        </main>
      </div>
    </div>
  );
}
