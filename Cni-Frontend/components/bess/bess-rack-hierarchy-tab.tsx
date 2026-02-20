"use client";

import { useState, useMemo, useCallback } from "react";
import { ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { BessHierarchyTable } from "./bess-hierarchy-table";
import { BessHierarchyDetail } from "./bess-hierarchy-detail";
import { BessCellDetail } from "./bess-cell-detail";
import type {
  BessRack,
  BessPack,
  BessModule,
  BessCell,
} from "@/lib/bess/types";

interface BessRackHierarchyTabProps {
  rack: BessRack;
}

type RackHierarchyLevel = "pack" | "module" | "cell";

interface HierarchyState {
  level: RackHierarchyLevel;
  packId?: string;
  moduleId?: string;
  cellId?: string;
}

interface BreadcrumbSegment {
  label: string;
  level: "rack" | RackHierarchyLevel;
  state: HierarchyState;
}

export function BessRackHierarchyTab({ rack }: BessRackHierarchyTabProps) {
  const [state, setState] = useState<HierarchyState>({ level: "pack" });

  const currentPack = useMemo(
    () => (state.packId ? rack.packs.find((p) => p.id === state.packId) : undefined),
    [rack.packs, state.packId]
  );
  const currentModule = useMemo(
    () =>
      state.moduleId && currentPack
        ? currentPack.modules.find((m) => m.id === state.moduleId)
        : undefined,
    [currentPack, state.moduleId]
  );
  const currentCell = useMemo(
    () =>
      state.cellId && currentModule
        ? currentModule.cells.find((c) => c.id === state.cellId)
        : undefined,
    [currentModule, state.cellId]
  );

  const breadcrumbs: BreadcrumbSegment[] = useMemo(() => {
    const segs: BreadcrumbSegment[] = [
      { label: `Rack ${rack.rackNumber}`, level: "rack", state: { level: "pack" } },
    ];
    if (currentPack) {
      segs.push({
        label: `Pack ${currentPack.packNumber}`,
        level: "pack",
        state: { level: "pack", packId: currentPack.id },
      });
    }
    if (currentModule && currentPack) {
      segs.push({
        label: `Module ${currentModule.moduleNumber}`,
        level: "module",
        state: { level: "module", packId: currentPack.id, moduleId: currentModule.id },
      });
    }
    if (currentCell && currentPack && currentModule) {
      segs.push({
        label: `Cell ${currentCell.cellNumber}`,
        level: "cell",
        state: {
          level: "cell",
          packId: currentPack.id,
          moduleId: currentModule.id,
          cellId: currentCell.id,
        },
      });
    }
    return segs;
  }, [rack, currentPack, currentModule, currentCell]);

  const handlePackSelect = useCallback((item: unknown) => {
    const pack = item as BessPack;
    setState({ level: "pack", packId: pack.id });
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
            <span key={seg.level + (seg.state.packId ?? "")} className="flex items-center gap-1">
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

      {/* Content */}
      {state.level === "pack" && !state.packId && (
        <BessHierarchyTable
          level="pack"
          items={rack.packs}
          onItemSelect={handlePackSelect}
        />
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
