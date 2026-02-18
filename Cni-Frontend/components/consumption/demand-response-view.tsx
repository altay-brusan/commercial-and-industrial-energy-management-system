"use client";

import { useMemo } from "react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  XAxis,
  YAxis,
} from "recharts";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { cn } from "@/lib/utils";
import {
  getEnrolledCount,
  getAvailableFlexibility,
  getCurrentCurtailment,
  getDREventsYTD,
  getDREnrollmentData,
  getFlexibilityByType,
  getDREventHistory,
  getConsumerLoadHeatmap,
} from "@/lib/consumption/analytics-mock-data";
import { consumers } from "@/lib/consumption/mock-data";
import type { DRStatus } from "@/lib/consumption/types";

const DONUT_COLORS = [
  "var(--color-energy-consumption)",
  "var(--color-energy-solar)",
  "var(--color-energy-battery)",
  "var(--color-energy-grid)",
  "hsl(var(--chart-5))",
  "hsl(var(--chart-1))",
];

const drEventConfig = {
  events: {
    label: "DR Events",
    color: "var(--color-energy-battery)",
  },
} satisfies ChartConfig;

const flexibilityConfig = {
  flexibility: {
    label: "Flexibility (kW)",
    color: "var(--color-energy-consumption)",
  },
} satisfies ChartConfig;

function DRStatusBadge({ status }: { status: DRStatus }) {
  switch (status) {
    case "enrolled":
      return (
        <Badge className="bg-energy-solar/15 text-energy-solar border-energy-solar/30 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-solar" />
          Enrolled
        </Badge>
      );
    case "active":
      return (
        <Badge className="bg-energy-battery/15 text-energy-battery border-energy-battery/30 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-battery animate-pulse" />
          Active
        </Badge>
      );
    case "standby":
      return (
        <Badge className="bg-energy-consumption/15 text-energy-consumption border-energy-consumption/30 gap-1">
          <span className="h-2 w-2 rounded-full bg-energy-consumption" />
          Standby
        </Badge>
      );
    case "not-enrolled":
      return (
        <Badge variant="secondary" className="gap-1">
          <span className="h-2 w-2 rounded-full bg-muted-foreground" />
          Not Enrolled
        </Badge>
      );
  }
}

export function DemandResponseView() {
  const enrolled = useMemo(() => getEnrolledCount(), []);
  const flexibility = useMemo(() => getAvailableFlexibility(), []);
  const curtailment = useMemo(() => getCurrentCurtailment(), []);
  const eventsYTD = useMemo(() => getDREventsYTD(), []);
  const enrollmentData = useMemo(() => getDREnrollmentData(), []);
  const flexByType = useMemo(() => getFlexibilityByType(), []);
  const eventHistory = useMemo(() => getDREventHistory(), []);
  const heatmapData = useMemo(() => getConsumerLoadHeatmap(), []);

  const totalConsumers = consumers.length;

  const stats = [
    { label: "Enrolled Consumers", value: `${enrolled}/${totalConsumers}`, unit: "" },
    { label: "Available Flexibility", value: flexibility.toLocaleString("en-US"), unit: "kW" },
    { label: "Currently Curtailed", value: curtailment.toLocaleString("en-US"), unit: "kW" },
    { label: "DR Events YTD", value: String(eventsYTD), unit: "" },
  ];

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Stats Bar */}
      <div className="shrink-0 grid grid-cols-4 border-b border-border">
        {stats.map((stat, i) => (
          <div
            key={stat.label}
            className={`px-6 py-5 ${i < stats.length - 1 ? "border-r border-border" : ""}`}
          >
            <p className="text-sm text-muted-foreground mb-1">{stat.label}</p>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-semibold tracking-tight">{stat.value}</span>
              <span className="text-sm text-muted-foreground">{stat.unit}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Content */}
      <main className="flex-1 overflow-y-auto">
        {/* DR Enrollment Table */}
        <div className="px-6 py-6 border-b border-border">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">DR Enrollment</h2>
            <p className="text-sm text-muted-foreground">Consumer demand response program participation</p>
          </div>
          <div className="overflow-x-auto border rounded-md">
            <Table>
              <TableHeader>
                <TableRow className="hover:bg-transparent">
                  <TableHead className="pl-4">Consumer</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>DR Status</TableHead>
                  <TableHead className="text-right">Contracted (kW)</TableHead>
                  <TableHead className="text-right">Curtailable (kW)</TableHead>
                  <TableHead className="text-right">Curtailed (kW)</TableHead>
                  <TableHead className="text-right pr-4">Events YTD</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {enrollmentData.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="pl-4 font-medium">{item.name}</TableCell>
                    <TableCell className="text-muted-foreground">{item.type}</TableCell>
                    <TableCell>
                      <DRStatusBadge status={item.drStatus} />
                    </TableCell>
                    <TableCell className="text-right text-muted-foreground">{item.contractedCapacity.toLocaleString("en-US")}</TableCell>
                    <TableCell className="text-right text-muted-foreground">{item.curtailableCapacity !== null ? item.curtailableCapacity.toLocaleString("en-US") : "--"}</TableCell>
                    <TableCell className="text-right text-muted-foreground">{item.currentCurtailment !== null ? item.currentCurtailment.toLocaleString("en-US") : "--"}</TableCell>
                    <TableCell className="text-right pr-4 text-muted-foreground">{item.drEventsYTD}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </div>

        {/* Flexibility by Type (donut) + DR Event History (bar) */}
        <div className="px-6 py-6 border-b border-border">
          <div className="grid gap-8 lg:grid-cols-2">
            {/* Flexibility by Type */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">Flexibility by Type</h2>
                <p className="text-sm text-muted-foreground">Curtailable capacity distribution (kW)</p>
              </div>
              <ChartContainer config={flexibilityConfig} className="h-[280px] w-full">
                <PieChart>
                  <Pie
                    data={flexByType}
                    dataKey="flexibility"
                    nameKey="type"
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={2}
                  >
                    {flexByType.map((_, index) => (
                      <Cell key={index} fill={DONUT_COLORS[index % DONUT_COLORS.length]} />
                    ))}
                  </Pie>
                  <ChartTooltip content={<ChartTooltipContent />} />
                </PieChart>
              </ChartContainer>
              <div className="flex flex-wrap items-center justify-center gap-4 mt-2">
                {flexByType.map((item, i) => (
                  <div key={item.type} className="flex items-center gap-1.5 text-xs">
                    <div className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: DONUT_COLORS[i % DONUT_COLORS.length] }} />
                    <span className="text-muted-foreground">{item.type} ({item.flexibility} kW)</span>
                  </div>
                ))}
              </div>
            </div>

            {/* DR Event History */}
            <div>
              <div className="mb-4">
                <h2 className="text-lg font-semibold">DR Event History</h2>
                <p className="text-sm text-muted-foreground">Monthly DR events (last 12 months)</p>
              </div>
              <ChartContainer config={drEventConfig} className="h-[280px] w-full">
                <BarChart data={eventHistory} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                  <XAxis dataKey="month" tickLine={false} axisLine={false} tickMargin={8} className="text-xs" />
                  <YAxis tickLine={false} axisLine={false} tickMargin={8} className="text-xs" allowDecimals={false} />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey="events" fill="var(--color-energy-battery)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ChartContainer>
            </div>
          </div>
        </div>

        {/* Consumer Load Heatmap */}
        <div className="px-6 py-6">
          <div className="mb-4">
            <h2 className="text-lg font-semibold">Consumer Load Heatmap</h2>
            <p className="text-sm text-muted-foreground">Hourly load as % of contracted capacity</p>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 px-2 font-medium text-muted-foreground sticky left-0 bg-background">Consumer</th>
                  {Array.from({ length: 24 }, (_, i) => (
                    <th key={i} className="text-center py-2 px-1 font-medium text-muted-foreground min-w-[36px]">
                      {String(i).padStart(2, "0")}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {heatmapData.map((row) => (
                  <tr key={row.consumer} className="border-b border-border/50">
                    <td className="py-2 px-2 font-medium sticky left-0 bg-background">{row.consumer}</td>
                    {row.hours.map((val, i) => {
                      const isHigh = val > 80;
                      const isMedium = val >= 50 && val <= 80;
                      return (
                        <td key={i} className="text-center py-1.5 px-0.5">
                          <div
                            className={cn(
                              "inline-flex items-center justify-center w-8 h-6 rounded text-[9px] font-mono",
                              isHigh && "bg-energy-grid/20 text-energy-grid",
                              isMedium && "bg-energy-consumption/20 text-energy-consumption",
                              !isHigh && !isMedium && "bg-energy-solar/20 text-energy-solar"
                            )}
                          >
                            {val}
                          </div>
                        </td>
                      );
                    })}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </div>
  );
}
