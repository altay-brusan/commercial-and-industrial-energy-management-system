import type { ScadaState } from "./types";

export const initialScadaState: ScadaState = {
  nodes: [
    {
      id: "load",
      name: "Load",
      realPower: 9.3,
      unit: "MW",
      secondaryLabel: "Energy Today",
      secondaryValue: "69.2 MWh",
      color: "energy-consumption",
    },
    {
      id: "battery",
      name: "Battery",
      realPower: 0.5,
      unit: "MW",
      secondaryLabel: "Stored",
      secondaryValue: "8.3 MWh",
      tertiaryLabel: "SoE",
      tertiaryValue: "76.6%",
      color: "energy-battery",
    },
    {
      id: "solar",
      name: "Solar",
      realPower: 8.4,
      unit: "MW",
      color: "energy-solar",
    },
    {
      id: "grid",
      name: "Grid",
      realPower: 0.4,
      unit: "MW",
      color: "energy-grid",
    },
  ],
  connections: [
    { from: "solar", to: "load", power: 7.5, direction: "forward" },
    { from: "solar", to: "battery", power: 0.5, direction: "forward" },
    { from: "grid", to: "load", power: 0.4, direction: "forward" },
    { from: "battery", to: "load", power: 0.9, direction: "forward" },
  ],
  metricsSections: [
    {
      id: "battery",
      title: "BATTERY",
      color: "energy-battery",
      metrics: [
        { label: "Real Power", value: "0.5", unit: "MW" },
        { label: "State of Energy", value: "76.6", unit: "%" },
        { label: "Stored Energy", value: "8.3", unit: "MWh" },
        { label: "Capacity", value: "10.8", unit: "MWh" },
        { label: "Charge Rate", value: "0.5", unit: "MW" },
        { label: "Discharge Rate", value: "0.0", unit: "MW" },
        { label: "Cycles Today", value: "3", unit: "" },
        { label: "Temperature", value: "32.4", unit: "°C" },
      ],
    },
    {
      id: "solar",
      title: "SOLAR",
      color: "energy-solar",
      metrics: [
        { label: "Real Power", value: "8.4", unit: "MW" },
        { label: "Irradiance", value: "842", unit: "W/m²" },
        { label: "Today's Production", value: "52.6", unit: "MWh" },
        { label: "Performance Ratio", value: "94.2", unit: "%" },
      ],
    },
    {
      id: "grid",
      title: "GRID",
      color: "energy-grid",
      metrics: [
        { label: "Import Power", value: "0.4", unit: "MW" },
        { label: "Export Power", value: "0.0", unit: "MW" },
        { label: "Frequency", value: "50.01", unit: "Hz" },
      ],
    },
    {
      id: "load",
      title: "LOAD",
      color: "energy-consumption",
      metrics: [
        { label: "Real Power", value: "9.3", unit: "MW" },
        { label: "Energy Today", value: "69.2", unit: "MWh" },
        { label: "Peak Demand", value: "11.2", unit: "MW" },
      ],
    },
  ],
  lastUpdated: null as unknown as Date,
};
