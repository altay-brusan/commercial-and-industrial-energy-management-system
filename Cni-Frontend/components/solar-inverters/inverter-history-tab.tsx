"use client";

import { useState, useMemo } from "react";
import {
  ChevronLeft,
  ChevronRight,
  CalendarIcon,
  Search,
  X,
} from "lucide-react";
import { format, addDays, subDays } from "date-fns";
import {
  Line,
  LineChart,
  CartesianGrid,
  XAxis,
  YAxis,
  ResponsiveContainer,
  Legend,
} from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  generateHistoryData,
  historySignalConfigs,
} from "@/lib/solar-inverters/mock-data";
import type { HistorySignalPoint } from "@/lib/solar-inverters/types";

interface InverterHistoryTabProps {
  inverterId: string;
}

const CHART_COLORS = [
  "var(--color-energy-solar)",
  "var(--color-energy-consumption)",
  "var(--color-energy-battery)",
  "var(--color-energy-grid)",
  "var(--color-severity-minor)",
];

const MAX_SIGNALS = 5;

export function InverterHistoryTab({ inverterId }: InverterHistoryTabProps) {
  const [selectedDate, setSelectedDate] = useState<Date>(new Date());
  const [selectedSignals, setSelectedSignals] = useState<HistorySignalPoint[]>([
    "activePower",
    "dcPower",
  ]);
  const [queriedSignals, setQueriedSignals] = useState<HistorySignalPoint[]>([
    "activePower",
    "dcPower",
  ]);
  const [calendarOpen, setCalendarOpen] = useState(false);
  const [signalOpen, setSignalOpen] = useState(false);

  const data = useMemo(
    () => generateHistoryData(inverterId),
    [inverterId]
  );

  const handleSearch = () => {
    setQueriedSignals([...selectedSignals]);
  };

  const toggleSignal = (signal: HistorySignalPoint) => {
    setSelectedSignals((prev) => {
      if (prev.includes(signal)) {
        return prev.filter((s) => s !== signal);
      }
      if (prev.length >= MAX_SIGNALS) return prev;
      return [...prev, signal];
    });
  };

  const removeSignal = (signal: HistorySignalPoint) => {
    setSelectedSignals((prev) => prev.filter((s) => s !== signal));
  };

  // Group signals by group
  const groupedSignals = useMemo(() => {
    const groups: Record<string, typeof historySignalConfigs> = {};
    for (const config of historySignalConfigs) {
      if (!groups[config.group]) groups[config.group] = [];
      groups[config.group].push(config);
    }
    return groups;
  }, []);

  // Build chart config from queried signals
  const chartConfig = useMemo(() => {
    const config: ChartConfig = {};
    queriedSignals.forEach((key, i) => {
      const signalConfig = historySignalConfigs.find((s) => s.key === key);
      if (signalConfig) {
        config[key] = {
          label: `${signalConfig.label} (${signalConfig.unit})`,
          color: CHART_COLORS[i % CHART_COLORS.length],
        };
      }
    });
    return config;
  }, [queriedSignals]);

  // Determine if we need dual Y-axis (different units)
  const units = useMemo(() => {
    const unitSet = new Set<string>();
    queriedSignals.forEach((key) => {
      const config = historySignalConfigs.find((s) => s.key === key);
      if (config?.unit) unitSet.add(config.unit);
    });
    return Array.from(unitSet);
  }, [queriedSignals]);

  const needsDualAxis = units.length === 2;
  const leftUnit = units[0] ?? "";
  const rightUnit = units[1] ?? "";

  const leftSignals = useMemo(
    () =>
      queriedSignals.filter((key) => {
        const config = historySignalConfigs.find((s) => s.key === key);
        return config?.unit === leftUnit;
      }),
    [queriedSignals, leftUnit]
  );

  const rightSignals = useMemo(
    () =>
      queriedSignals.filter((key) => {
        const config = historySignalConfigs.find((s) => s.key === key);
        return config?.unit === rightUnit;
      }),
    [queriedSignals, rightUnit]
  );

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4 space-y-4">
      {/* Filter bar */}
      <div className="flex items-center gap-3 flex-wrap">
        {/* Date Picker */}
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => setSelectedDate((d) => subDays(d, 1))}
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className="h-8 gap-2 text-sm font-normal"
              >
                {format(selectedDate, "dd/MMM/yyyy")}
                <CalendarIcon className="h-3.5 w-3.5" />
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0" align="start">
              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={(date) => {
                  if (date) {
                    setSelectedDate(date);
                    setCalendarOpen(false);
                  }
                }}
                initialFocus
              />
            </PopoverContent>
          </Popover>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            onClick={() => setSelectedDate((d) => addDays(d, 1))}
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>

        {/* Signal Selector */}
        <Popover open={signalOpen} onOpenChange={setSignalOpen}>
          <PopoverTrigger asChild>
            <Button
              variant="outline"
              className="h-8 text-sm font-normal min-w-[200px] justify-start"
            >
              {selectedSignals.length === 0
                ? "Select signals..."
                : `${selectedSignals.length} signal${selectedSignals.length > 1 ? "s" : ""} selected`}
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-[320px] p-0" align="start">
            <Command>
              <CommandInput placeholder="Search signals..." />
              <CommandList>
                <CommandEmpty>No signals found.</CommandEmpty>
                {Object.entries(groupedSignals).map(([group, signals]) => (
                  <CommandGroup key={group} heading={group}>
                    {signals.map((signal) => {
                      const isSelected = selectedSignals.includes(signal.key);
                      const isDisabled =
                        !isSelected && selectedSignals.length >= MAX_SIGNALS;
                      return (
                        <CommandItem
                          key={signal.key}
                          value={signal.label}
                          onSelect={() => {
                            if (!isDisabled) toggleSignal(signal.key);
                          }}
                          disabled={isDisabled}
                          className="gap-2"
                        >
                          <div
                            className={`h-4 w-4 rounded border flex items-center justify-center ${
                              isSelected
                                ? "bg-primary border-primary"
                                : "border-input"
                            }`}
                          >
                            {isSelected && (
                              <svg
                                className="h-3 w-3 text-primary-foreground"
                                viewBox="0 0 24 24"
                                fill="none"
                                stroke="currentColor"
                                strokeWidth="3"
                              >
                                <path d="M5 13l4 4L19 7" />
                              </svg>
                            )}
                          </div>
                          <span>
                            {signal.label}{" "}
                            {signal.unit && (
                              <span className="text-muted-foreground">
                                ({signal.unit})
                              </span>
                            )}
                          </span>
                        </CommandItem>
                      );
                    })}
                  </CommandGroup>
                ))}
              </CommandList>
            </Command>
          </PopoverContent>
        </Popover>

        {/* Search button */}
        <Button
          size="sm"
          className="h-8 gap-1.5"
          onClick={handleSearch}
          disabled={selectedSignals.length === 0}
        >
          <Search className="h-3.5 w-3.5" />
          Search
        </Button>
      </div>

      {/* Selected signals chips */}
      {selectedSignals.length > 0 && (
        <div className="flex items-center gap-1.5 flex-wrap">
          {selectedSignals.map((key, i) => {
            const config = historySignalConfigs.find((s) => s.key === key);
            return (
              <Badge
                key={key}
                variant="outline"
                className="gap-1 pr-1 text-xs"
              >
                <div
                  className="w-2 h-2 rounded-full"
                  style={{
                    backgroundColor: CHART_COLORS[i % CHART_COLORS.length],
                  }}
                />
                {config?.label}
                <button
                  onClick={() => removeSignal(key)}
                  className="ml-0.5 hover:bg-muted rounded-sm p-0.5"
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            );
          })}
        </div>
      )}

      {/* Chart */}
      {queriedSignals.length > 0 ? (
        <ChartContainer config={chartConfig} className="h-[400px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart
              data={data}
              margin={{ top: 10, right: needsDualAxis ? 10 : 10, left: -15, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
              <XAxis
                dataKey="time"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
                interval={Math.max(1, Math.floor(data.length / 12))}
              />
              <YAxis
                yAxisId="left"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
                label={
                  leftUnit
                    ? {
                        value: leftUnit,
                        angle: -90,
                        position: "insideLeft",
                        offset: 20,
                        className: "text-xs fill-muted-foreground",
                      }
                    : undefined
                }
              />
              {needsDualAxis && (
                <YAxis
                  yAxisId="right"
                  orientation="right"
                  tickLine={false}
                  axisLine={false}
                  tickMargin={8}
                  className="text-xs"
                  label={
                    rightUnit
                      ? {
                          value: rightUnit,
                          angle: 90,
                          position: "insideRight",
                          offset: 20,
                          className: "text-xs fill-muted-foreground",
                        }
                      : undefined
                  }
                />
              )}
              <ChartTooltip
                content={<ChartTooltipContent indicator="line" />}
              />
              <Legend
                verticalAlign="top"
                height={36}
                iconType="circle"
                iconSize={8}
                wrapperStyle={{ fontSize: "12px" }}
              />
              {queriedSignals.map((key, i) => {
                const yAxisId =
                  needsDualAxis && rightSignals.includes(key)
                    ? "right"
                    : "left";
                return (
                  <Line
                    key={key}
                    yAxisId={yAxisId}
                    type="monotone"
                    dataKey={key}
                    stroke={CHART_COLORS[i % CHART_COLORS.length]}
                    strokeWidth={1.5}
                    dot={false}
                    name={
                      historySignalConfigs.find((s) => s.key === key)?.label ??
                      key
                    }
                  />
                );
              })}
            </LineChart>
          </ResponsiveContainer>
        </ChartContainer>
      ) : (
        <div className="flex items-center justify-center text-muted-foreground py-20">
          Select signal points and click Search to view history data.
        </div>
      )}
    </div>
  );
}
