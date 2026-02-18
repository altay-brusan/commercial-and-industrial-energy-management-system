export type FacilityType = "solar" | "wind" | "bess" | "hybrid";

export interface Facility {
  id: string;
  name: string;
  type: FacilityType;
  location: string;
  country: string;
  capacity: number; // MW
}
