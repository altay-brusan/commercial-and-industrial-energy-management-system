export interface BessState {
    id: string;
    name: string;
    stateOfCharge: number;
    status: 'charging' | 'discharging' | 'standby';
    temperature: number;
}

export interface PcsState {
    status: string;
    temperature: number;
    load: number;
    efficiency: number;
}

export interface SolarState {
    capacity: number;
    maintenanceStatus: string;
    temperature: number;
}

export interface EnergyUsage {
    totalToday: number;
    activePower: number;
    reactivePower: number;
    sources: {
        grid: number;
        bess: number;
        generator: number;
    };
}

export interface EnergyCost {
    gridConsumption: number;
    savings: number;
}

export interface BessDevice {
    deviceId: number;
    nodeId: number;
    stateOfCharge: number;
    stateOfHealth: number;
    dcPower: DcPower;
    batteryTemperature: number;
    totalEnergyStored: number;
    coolingActive: boolean;
    capacity: number;
    chargeStatus: number;
    status: Status;
}

export interface DcPower {
    voltage: number;
    current: number;
    power: number;
    totalEnergyDaily: number;
    totalEnergyLifetime: number;
}

export interface Status {
    operatingMode: number;
    warningStatus: boolean;
    warningMessages: any[];
    faultStatus: boolean;
    faultMessages: any[];
}

export interface FacilityData {
    deviceId: number;
    nodeId: number;
    acPower: ACPower;
    status: Status;
}

export interface ACPower {
    totalActivePower: number;
    totalReactivePower: number;
    totalPowerFactor: number;
    totalEnergyDaily: number;
    currentRms: number;
    voltageRms: number;
}

export interface GeneratorDevice {
    deviceId: number;
    nodeId: number;
    totalPower: number;
    loadGeneratorContribution: number;
}

export interface PcsDevice {
    deviceId: number;
    nodeId: number;
    efficiency: number;
    temperaturePcs: number;
    acPower: ACPower;
    dcPower: DcPower;
    status: Status;
}

export interface SolarDevice {
    deviceId: number;
    nodeId: number;
    dcOutput: DcPower;
    panelPower: DcPower;
    performanceRatio: number;
    pvPanelTemperature: number;
    status: Status;
    irradiance: number;
    weather: number;
}

export interface WeatherData {
    temperature: number;
    irradiance: number;
}

export interface SystemData {
    generatorPower: number;
    gridPower: number;
    loadGeneratorContribution: number;
    loadGridContribution: number;
    loadPcsContribution: number;
    loadSolarContribution: number;
    pcsPower: number;
    solarPower: number;
    cost_Saving_TL_Daily: number;
    grid_Consumption_TL_Daily: number;
}

export interface DashboardRealTimeData {
    bessDevices: BessDevice[];
    solarDevices: SolarDevice[];
    pcsDevices: PcsDevice[];
    generatorDevices: GeneratorDevice[];
    gridDevices: FacilityData[];
    facilityData: FacilityData;
    systemData: SystemData;
    weatherData?: WeatherData;
}
