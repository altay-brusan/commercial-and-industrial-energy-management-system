export interface FacilityBriefDto {
  id: string;
  name: string;
  location: string | null;
  country: string | null;
  type: string | null;
  capacity: number | null;
}

export interface PaginatedFacilitiesDto {
  items: FacilityBriefDto[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
