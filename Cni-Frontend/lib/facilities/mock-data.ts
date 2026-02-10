import type { Facility } from "./types";

export const facilities: Facility[] = [
  {
    id: "fac-001",
    name: "İzmir Çeşme Güneş Santrali",
    type: "solar",
    location: "İzmir",
    country: "Turkey",
    capacity: 45.5,
  },
  {
    id: "fac-002",
    name: "Çanakkale Rüzgar Enerji Santrali",
    type: "wind",
    location: "Çanakkale",
    country: "Turkey",
    capacity: 64.0,
  },
  {
    id: "fac-003",
    name: "Ankara Enerji Depolama Merkezi",
    type: "bess",
    location: "Ankara",
    country: "Turkey",
    capacity: 30.0,
  },
  {
    id: "fac-004",
    name: "Gaziantep Hibrit Enerji Kompleksi",
    type: "hybrid",
    location: "Gaziantep",
    country: "Turkey",
    capacity: 85.0,
  },
  {
    id: "fac-005",
    name: "Muğla Yatağan Güneş Parkı",
    type: "solar",
    location: "Muğla",
    country: "Turkey",
    capacity: 37.8,
  },
  {
    id: "fac-006",
    name: "Kayseri Hibrit Merkezi",
    type: "hybrid",
    location: "Kayseri",
    country: "Turkey",
    capacity: 12.5,
  },
];
