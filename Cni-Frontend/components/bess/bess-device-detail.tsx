"use client";

import { BessUnitDetail } from "./bess-unit-detail";
import { BessRackDetail } from "./bess-rack-detail";
import { BessEscDetail } from "./bess-esc-detail";
import { BessHvacDetail } from "./bess-hvac-detail";
import {
  getBessUnitById,
  getRackById,
  getEscById,
  getHvacById,
} from "@/lib/bess/mock-data";
import type { BessSelectedDevice } from "@/lib/bess/types";

interface BessDeviceDetailProps {
  selectedDevice: BessSelectedDevice;
  onBack: () => void;
  onAlarmOpen: (alarmId: string) => void;
}

export function BessDeviceDetail({
  selectedDevice,
  onBack,
  onAlarmOpen,
}: BessDeviceDetailProps) {
  switch (selectedDevice.type) {
    case "unit": {
      const unit = getBessUnitById(selectedDevice.id);
      if (!unit) return <EmptyState />;
      return (
        <BessUnitDetail unit={unit} onBack={onBack} onAlarmOpen={onAlarmOpen} />
      );
    }
    case "rack": {
      const result = getRackById(selectedDevice.id);
      if (!result) return <EmptyState />;
      return (
        <BessRackDetail
          rack={result.rack}
          unit={result.unit}
          onBack={onBack}
          onAlarmOpen={onAlarmOpen}
        />
      );
    }
    case "esc": {
      const esc = getEscById(selectedDevice.id);
      if (!esc) return <EmptyState />;
      return (
        <BessEscDetail esc={esc} onBack={onBack} onAlarmOpen={onAlarmOpen} />
      );
    }
    case "hvac": {
      const hvac = getHvacById(selectedDevice.id);
      if (!hvac) return <EmptyState />;
      return (
        <BessHvacDetail hvac={hvac} onBack={onBack} onAlarmOpen={onAlarmOpen} />
      );
    }
    default:
      return <EmptyState />;
  }
}

function EmptyState() {
  return (
    <div className="flex-1 flex items-center justify-center text-muted-foreground">
      Device not found.
    </div>
  );
}
