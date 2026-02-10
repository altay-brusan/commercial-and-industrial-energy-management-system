"use client";

import { createContext, useContext, useState } from "react";
import type { Facility } from "./types";
import { facilities } from "./mock-data";

interface FacilityContextValue {
  selectedFacilityId: string;
  setSelectedFacilityId: (id: string) => void;
  selectedFacility: Facility;
  facilities: Facility[];
}

const FacilityContext = createContext<FacilityContextValue | null>(null);

export function FacilityProvider({ children }: { children: React.ReactNode }) {
  const [selectedFacilityId, setSelectedFacilityId] = useState(facilities[0].id);

  const selectedFacility = facilities.find((f) => f.id === selectedFacilityId) ?? facilities[0];

  return (
    <FacilityContext.Provider
      value={{
        selectedFacilityId,
        setSelectedFacilityId,
        selectedFacility,
        facilities,
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
