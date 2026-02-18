"use client";

import { useState, useMemo, useCallback } from "react";
import { ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { BessHierarchyTable } from "./bess-hierarchy-table";
import { BessHierarchyDetail } from "./bess-hierarchy-detail";
import { BessCellDetail } from "./bess-cell-detail";
import type {
  BessUnit,
  BessRack,
  BessPack,
  BessModule,
  BessCell,
  HierarchyLevel,
} from "@/lib/bess/types";

interface BessHierarchyTabProps {
  unit: BessUnit;
}

interface HierarchyState {
  level: HierarchyLevel;
  rackId?: string;
  packId?: string;
  moduleId?: string;
  cellId?: string;
}

interface BreadcrumbSegment {
  label: string;
  level: HierarchyLevel;
  state: HierarchyState;
}

export function BessHierarchyTab({ unit }: BessHierarchyTabProps) {
  const [state, setState] = useState<HierarchyState>({ level: "unit" });

  // Resolve current items from hierarchy
  const currentRack = useMemo(
    () => state.rackId ? unit.racks.find((r) => r.id === state.rackId) : undefined,
    [unit.racks, state.rackId]
  );
  const currentPack = useMemo(
    () => state.packId && currentRack ? currentRack.packs.find((p) => p.id === state.packId) : undefined,
    [currentRack, state.packId]
  );
  const currentModule = useMemo(
    () => state.moduleId && currentPack ? currentPack.modules.find((m) => m.id === state.moduleId) : undefined,
    [currentPack, state.moduleId]
  );
  const currentCell = useMemo(
    () => state.cellId && currentModule ? currentModule.cells.find((c) => c.id === state.cellId) : undefined,
    [currentModule, state.cellId]
  );

  // Build breadcrumb
  const breadcrumbs: BreadcrumbSegment[] = useMemo(() => {
    const segments: BreadcrumbSegment[] = [
      { label: unit.name, level: "unit", state: { level: "unit" } },
    ];
    if (currentRack) {
      segments.push({
        label: `Rack ${currentRack.rackNumber}`,
        level: "rack",
        state: { level: "rack", rackId: currentRack.id },
      });
    }
    if (currentPack && currentRack) {
      segments.push({
        label: `Pack ${currentPack.packNumber}`,
        level: "pack",
        state: { level: "pack", rackId: currentRack.id, packId: currentPack.id },
      });
    }
    if (currentModule && currentRack && currentPack) {
      segments.push({
        label: `Module ${currentModule.moduleNumber}`,
        level: "module",
        state: { level: "module", rackId: currentRack.id, packId: currentPack.id, moduleId: currentModule.id },
      });
    }
    if (currentCell && currentRack && currentPack && currentModule) {
      segments.push({
        label: `Cell ${currentCell.cellNumber}`,
        level: "cell",
        state: { level: "cell", rackId: currentRack.id, packId: currentPack.id, moduleId: currentModule.id, cellId: currentCell.id },
      });
    }
    return segments;
  }, [unit, currentRack, currentPack, currentModule, currentCell]);

  const handleRackSelect = useCallback((item: unknown) => {
    const rack = item as BessRack;
    setState({ level: "rack", rackId: rack.id });
  }, []);

  const handlePackSelect = useCallback((item: unknown) => {
    const pack = item as BessPack;
    setState((prev) => ({ ...prev, level: "pack", packId: pack.id }));
  }, []);

  const handleModuleSelect = useCallback((item: unknown) => {
    const mod = item as BessModule;
    setState((prev) => ({ ...prev, level: "module", moduleId: mod.id }));
  }, []);

  const handleCellSelect = useCallback((item: unknown) => {
    const cell = item as BessCell;
    setState((prev) => ({ ...prev, level: "cell", cellId: cell.id }));
  }, []);

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-1 text-sm mb-4 flex-wrap">
        {breadcrumbs.map((seg, i) => {
          const isLast = i === breadcrumbs.length - 1;
          return (
            <span key={seg.level} className="flex items-center gap-1">
              {i > 0 && <ChevronRight className="h-3.5 w-3.5 text-muted-foreground/50" />}
              <button
                onClick={() => setState(seg.state)}
                className={cn(
                  "px-1.5 py-0.5 rounded transition-colors",
                  isLast
                    ? "text-foreground font-medium cursor-default"
                    : "text-muted-foreground hover:text-foreground hover:bg-muted"
                )}
                disabled={isLast}
              >
                {seg.label}
              </button>
            </span>
          );
        })}
      </nav>

      {/* Content based on level */}
      {state.level === "unit" && (
        <BessHierarchyTable
          level="rack"
          items={unit.racks}
          onItemSelect={handleRackSelect}
        />
      )}

      {state.level === "rack" && currentRack && (
        <>
          <BessHierarchyDetail item={currentRack} label={`Rack ${currentRack.rackNumber}`} />
          <BessHierarchyTable
            level="pack"
            items={currentRack.packs}
            onItemSelect={handlePackSelect}
          />
        </>
      )}

      {state.level === "pack" && currentPack && (
        <>
          <BessHierarchyDetail item={currentPack} label={`Pack ${currentPack.packNumber}`} />
          <BessHierarchyTable
            level="module"
            items={currentPack.modules}
            onItemSelect={handleModuleSelect}
          />
        </>
      )}

      {state.level === "module" && currentModule && (
        <>
          <BessHierarchyDetail item={currentModule} label={`Module ${currentModule.moduleNumber}`} />
          <BessHierarchyTable
            level="module"
            items={currentModule.cells}
            onItemSelect={handleCellSelect}
          />
        </>
      )}

      {state.level === "cell" && currentCell && currentModule && (
        <BessCellDetail cell={currentCell} module={currentModule} />
      )}
    </div>
  );
}
