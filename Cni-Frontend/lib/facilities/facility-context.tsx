"use client";

import { createContext, useContext, useState, useEffect } from "react";
import type { FacilityBriefDto } from "./types";
import * as facilityService from "./facility-service";

interface FacilityContextValue {
  selectedFacilityId: string;
  setSelectedFacilityId: (id: string) => void;
  selectedFacility: FacilityBriefDto | null;
  facilities: FacilityBriefDto[];
  loading: boolean;
}

// Default facility ID — used as fallback when Facilities API is unavailable
const DEFAULT_FACILITY_ID = "83ffe365-c2ee-4c41-a3f4-fe3626598a3c";

const FacilityContext = createContext<FacilityContextValue | null>(null);

export function FacilityProvider({ children }: { children: React.ReactNode }) {
  const [facilities, setFacilities] = useState<FacilityBriefDto[]>([]);
  const [selectedFacilityId, setSelectedFacilityId] = useState(DEFAULT_FACILITY_ID);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    facilityService
      .getFacilities({ pageSize: 100 })
      .then((res) => {
        if (res.items.length > 0) {
          setFacilities(res.items);
          // Only change selection if current ID is still the default and API has data
          setSelectedFacilityId((prev) =>
            prev === DEFAULT_FACILITY_ID && res.items.every((f) => f.id !== prev)
              ? res.items[0].id
              : prev
          );
        }
      })
      .catch(() => {
        // Facilities API may not be available yet — use default facility
      })
      .finally(() => setLoading(false));
  }, []);

  const selectedFacility =
    facilities.find((f) => f.id === selectedFacilityId) ?? null;

  return (
    <FacilityContext.Provider
      value={{
        selectedFacilityId,
        setSelectedFacilityId,
        selectedFacility,
        facilities,
        loading,
      }}
    >
      {children}
    </FacilityContext.Provider>
  );
}

export function useFacility() {
  const context = useContext(FacilityContext);
  if (!context) {
    throw new Error("useFacility must be used within a FacilityProvider");
  }
  return context;
}
