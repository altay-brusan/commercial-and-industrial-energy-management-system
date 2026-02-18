"use client";

import { Activity } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { PcsUnit } from "@/lib/pcs/types";

interface PcsGeneralTabProps {
  unit: PcsUnit;
}

function MetricItem({ label, value, unit }: { label: string; value: number | string; unit?: string }) {
  const displayValue = typeof value === "number" ? `${value.toLocaleString("en-US")} ${unit ?? ""}` : value;
  return (
    <div className="flex items-center justify-between py-2">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium">{displayValue}</span>
        {typeof value === "number" && <Activity className="h-3.5 w-3.5 text-muted-foreground/50" />}
      </div>
    </div>
  );
}

function MetricGrid({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-8 gap-y-0">
      {children}
    </div>
  );
}

function SectionTitle({ title }: { title: string }) {
  return (
    <h3 className="text-sm font-semibold text-foreground mt-6 mb-2 first:mt-0">
      {title}
    </h3>
  );
}

export function PcsGeneralTab({ unit }: PcsGeneralTabProps) {
  const { ac, dc, power, pcsStatus, temperatures, algoParams } = unit;

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* PCS Status */}
      <SectionTitle title="PCS Status" />
      <MetricGrid>
        <MetricItem label="Grid Frequency" value={pcsStatus.gridFreqHz} unit="Hz" />
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Phase Sequence</span>
          <Badge variant="secondary" className="capitalize text-xs">{pcsStatus.gridPhaseSeq}</Badge>
        </div>
        <MetricItem label="Derating Coefficient" value={pcsStatus.deratCoeff} />
      </MetricGrid>
      <MetricGrid>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Working Condition</span>
          <Badge variant="secondary" className="text-xs">{pcsStatus.workingCondition}</Badge>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Source Mode</span>
          <Badge variant="secondary" className="text-xs">{pcsStatus.sourceMode}</Badge>
        </div>
        <div className="flex items-center justify-between py-2">
          <span className="text-sm text-muted-foreground">Grid Mode</span>
          <Badge variant="secondary" className="text-xs">{pcsStatus.gridMode}</Badge>
        </div>
      </MetricGrid>

      {/* AC Measurements */}
      <SectionTitle title="AC Measurements" />
      <h4 className="text-xs font-medium text-muted-foreground mt-3 mb-1">Line Voltages</h4>
      <MetricGrid>
        <MetricItem label="V(AB)" value={ac.lineVoltageAbV} unit="V" />
        <MetricItem label="V(BC)" value={ac.lineVoltageBcV} unit="V" />
        <MetricItem label="V(CA)" value={ac.lineVoltageCaV} unit="V" />
      </MetricGrid>
      <h4 className="text-xs font-medium text-muted-foreground mt-3 mb-1">Phase Voltages</h4>
      <MetricGrid>
        <MetricItem label="V(A)" value={ac.phaseVoltageAV} unit="V" />
        <MetricItem label="V(B)" value={ac.phaseVoltageBV} unit="V" />
        <MetricItem label="V(C)" value={ac.phaseVoltageCV} unit="V" />
      </MetricGrid>
      <h4 className="text-xs font-medium text-muted-foreground mt-3 mb-1">Phase Currents</h4>
      <MetricGrid>
        <MetricItem label="I(A)" value={ac.phaseCurrentAA} unit="A" />
        <MetricItem label="I(B)" value={ac.phaseCurrentBA} unit="A" />
        <MetricItem label="I(C)" value={ac.phaseCurrentCA} unit="A" />
      </MetricGrid>
      <h4 className="text-xs font-medium text-muted-foreground mt-3 mb-1">Inductor Currents</h4>
      <MetricGrid>
        <MetricItem label="IL(A)" value={ac.inductorCurrentAA} unit="A" />
        <MetricItem label="IL(B)" value={ac.inductorCurrentBA} unit="A" />
        <MetricItem label="IL(C)" value={ac.inductorCurrentCA} unit="A" />
      </MetricGrid>

      {/* DC Measurements */}
      <SectionTitle title="DC Measurements" />
      <MetricGrid>
        <MetricItem label="Total DC Bus" value={dc.totalBusV} unit="V" />
        <MetricItem label="DC Bus (+)" value={dc.posBusV} unit="V" />
        <MetricItem label="DC Bus (−)" value={dc.negBusV} unit="V" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="Battery Voltage" value={dc.batV} unit="V" />
        <MetricItem label="Battery Current" value={dc.batI} unit="A" />
        <MetricItem label="DC Power" value={dc.dcPwrKw} unit="kW" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="AC Power Factor" value={dc.acPwrFactor} />
        <MetricItem label="Total DC Current" value={dc.totalDcI} unit="A" />
      </MetricGrid>

      {/* Power Table */}
      <SectionTitle title="Power Distribution" />
      <div className="border rounded-md overflow-hidden bg-muted/30">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="text-left px-4 py-2 text-xs font-medium text-muted-foreground">Phase</th>
              <th className="text-right px-4 py-2 text-xs font-medium text-muted-foreground">Active (kW)</th>
              <th className="text-right px-4 py-2 text-xs font-medium text-muted-foreground">Apparent (kVA)</th>
              <th className="text-right px-4 py-2 text-xs font-medium text-muted-foreground">Reactive (kVar)</th>
            </tr>
          </thead>
          <tbody>
            {(["phaseA", "phaseB", "phaseC"] as const).map((phase, i) => (
              <tr key={phase} className="border-b">
                <td className="px-4 py-2 font-medium">Phase {String.fromCharCode(65 + i)}</td>
                <td className="px-4 py-2 text-right font-mono">{power[phase].activeKw}</td>
                <td className="px-4 py-2 text-right font-mono">{power[phase].apparentKva}</td>
                <td className="px-4 py-2 text-right font-mono">{power[phase].reactiveKvar}</td>
              </tr>
            ))}
            <tr className="bg-muted/30 font-semibold">
              <td className="px-4 py-2">Total</td>
              <td className="px-4 py-2 text-right font-mono">{power.totalActiveKw}</td>
              <td className="px-4 py-2 text-right font-mono">{power.totalApparentKva}</td>
              <td className="px-4 py-2 text-right font-mono">{power.totalReactiveKvar}</td>
            </tr>
          </tbody>
        </table>
      </div>

      {/* Temperatures */}
      <SectionTitle title="Temperatures" />
      <MetricGrid>
        <MetricItem label="IGBT" value={temperatures.igbtTempC} unit="°C" />
        <MetricItem label="Ambient" value={temperatures.ambientTempC} unit="°C" />
        <MetricItem label="Inductance" value={temperatures.inductanceTempC} unit="°C" />
      </MetricGrid>

      {/* Charge Algorithm */}
      <SectionTitle title="Charge Algorithm Parameters" />
      <MetricGrid>
        <MetricItem label="CC-CV Recharge V" value={algoParams.ccCvRechgV} unit="V" />
        <MetricItem label="CV Charge V" value={algoParams.batCvChgV} unit="V" />
        <MetricItem label="CV Max Time" value={algoParams.cvChgMaxTimeMin} unit="min" />
      </MetricGrid>
      <MetricGrid>
        <MetricItem label="CV → Float I" value={algoParams.cvToFloatI} unit="A" />
        <MetricItem label="Float Charge V" value={algoParams.floatChgV} unit="V" />
        <MetricItem label="Float → CC I" value={algoParams.floatToCcI} unit="A" />
      </MetricGrid>

      <div className="h-6" />
    </div>
  );
}
