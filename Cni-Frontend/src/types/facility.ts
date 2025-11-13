export type Facility = {
  id: number;
  tenantId: number;
  firmId: number;
  name: string;
  address: string;
  capacity: number;
  longitude: number;
  latitude: number;
  grpcStatus: boolean;
  grpcChannel: string;
  status: 'active' | 'inactive' | 'maintenance';
  efficiency: number;
};
