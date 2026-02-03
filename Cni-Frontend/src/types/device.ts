export type DeviceType =
    | 'BESS'
    | 'BMU'
    | 'Inverter'
    | 'Solar'
    | 'PCS'
    | 'Generator'
    | 'Grid'
    | 'EVCharger'
    | 'HeatPump'
    | 'Transformer'
    | 'SmartMeter';

export type DeviceStatus = 'active' | 'inactive' | 'fault' | 'maintenance' | 'offline';

export interface Device {
    id: string;
    name: string;
    type: DeviceType;
    status: DeviceStatus;
    facilityId: number;
    facilityName?: string;
    serialNumber?: string;
    manufacturer?: string;
    model?: string;
    capacity?: number;
    latitude?: number;
    longitude?: number;
    description?: string;
    lastUpdate: string;
    createdAt: string;
}

export interface DeviceFilter {
    type?: DeviceType;
    status?: DeviceStatus;
    search?: string;
}

export interface DeviceSummary {
    total: number;
    active: number;
    fault: number;
    maintenance: number;
    offline: number;
}

export interface DeviceListQuery {
    pageNumber: number;
    pageSize: number;
    filter?: DeviceFilter;
}
