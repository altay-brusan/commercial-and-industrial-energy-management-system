"use client";

import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
import { DynamicSections } from "@/components/bess/dynamic-sections";
import * as pcsService from "@/lib/pcs/pcs-service";
import type { DynamicSectionDto } from "@/lib/pcs/api-types";

interface PcsGeneralTabProps {
  unitId: string;
}

export function PcsGeneralTab({ unitId }: PcsGeneralTabProps) {
  const [sections, setSections] = useState<DynamicSectionDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    pcsService.getUnitGeneral(unitId).then((data) => {
      setSections(data.sections);
    }).catch(() => {
      // handled by apiClient
    }).finally(() => setLoading(false));
  }, [unitId]);

  if (loading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="flex-1 overflow-y-auto px-6 py-4">
      <DynamicSections sections={sections} />
      <div className="h-6" />
    </div>
  );
}
