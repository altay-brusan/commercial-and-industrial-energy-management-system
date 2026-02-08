"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

interface TodayFinancials {
  cost: number;
  savings: number;
  revenue: number;
  netBenefit: number;
}

interface MonthFinancials {
  totalCost: number;
  totalSavings: number;
  totalRevenue: number;
  targetSavings: number;
  roiProgress: number;
}

const financialData = {
  today: {
    cost: 42.5,
    savings: 78.3,
    revenue: 23.4,
    netBenefit: 59.2,
  } as TodayFinancials,
  month: {
    totalCost: 856.2,
    totalSavings: 1542.8,
    totalRevenue: 487.3,
    targetSavings: 1800,
    roiProgress: 68,
  } as MonthFinancials,
};

export function FinancialSummary() {
  const savingsProgress =
    (financialData.month.totalSavings / financialData.month.targetSavings) * 100;

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base">Financial Summary</CardTitle>
      </CardHeader>
      <CardContent>
        <Tabs defaultValue="today">
          <TabsList className="mb-4 h-8">
            <TabsTrigger value="today" className="text-xs h-7">
              Today
            </TabsTrigger>
            <TabsTrigger value="month" className="text-xs h-7">
              This Month
            </TabsTrigger>
          </TabsList>

          <TabsContent value="today" className="space-y-3 mt-0">
            <div className="grid grid-cols-2 gap-3">
              <div className="p-3 rounded-lg bg-muted/50">
                <p className="text-xs text-muted-foreground">Grid Cost</p>
                <p className="text-lg font-semibold text-energy-grid">
                  ${financialData.today.cost.toFixed(2)}
                </p>
              </div>
              <div className="p-3 rounded-lg bg-muted/50">
                <p className="text-xs text-muted-foreground">Savings</p>
                <p className="text-lg font-semibold text-energy-solar">
                  ${financialData.today.savings.toFixed(2)}
                </p>
              </div>
              <div className="p-3 rounded-lg bg-muted/50">
                <p className="text-xs text-muted-foreground">Export Revenue</p>
                <p className="text-lg font-semibold text-energy-battery">
                  ${financialData.today.revenue.toFixed(2)}
                </p>
              </div>
              <div className="p-3 rounded-lg bg-energy-solar/10">
                <p className="text-xs text-muted-foreground">Net Benefit</p>
                <p className="text-lg font-semibold text-energy-solar">
                  +${financialData.today.netBenefit.toFixed(2)}
                </p>
              </div>
            </div>
          </TabsContent>

          <TabsContent value="month" className="space-y-4 mt-0">
            <div className="grid grid-cols-3 gap-2 text-center">
              <div>
                <p className="text-xs text-muted-foreground">Cost</p>
                <p className="font-semibold text-energy-grid">
                  ${financialData.month.totalCost.toFixed(0)}
                </p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Savings</p>
                <p className="font-semibold text-energy-solar">
                  ${financialData.month.totalSavings.toFixed(0)}
                </p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Revenue</p>
                <p className="font-semibold text-energy-battery">
                  ${financialData.month.totalRevenue.toFixed(0)}
                </p>
              </div>
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">Savings vs Target</span>
                <span className="font-medium">
                  ${financialData.month.totalSavings.toFixed(0)} / $
                  {financialData.month.targetSavings}
                </span>
              </div>
              <Progress
                value={savingsProgress}
                className="h-2 [&>[data-slot=progress-indicator]]:bg-energy-solar"
              />
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground">
                  {savingsProgress.toFixed(0)}% of target
                </span>
                <Badge
                  variant="secondary"
                  className={
                    savingsProgress >= 80
                      ? "text-energy-solar"
                      : savingsProgress >= 60
                      ? "text-energy-consumption"
                      : "text-energy-grid"
                  }
                >
                  {savingsProgress >= 80
                    ? "On Track"
                    : savingsProgress >= 60
                    ? "Needs Attention"
                    : "Behind"}
                </Badge>
              </div>
            </div>

            <div className="pt-2 border-t">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">ROI Progress</span>
                <span className="font-semibold">
                  {financialData.month.roiProgress}%
                </span>
              </div>
            </div>
          </TabsContent>
        </Tabs>
      </CardContent>
    </Card>
  );
}
