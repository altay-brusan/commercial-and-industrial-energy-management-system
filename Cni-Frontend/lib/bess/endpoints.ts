export const BESS_ENDPOINTS = {
  TREE: "/BatteryStorage/Tree/{facilityId}",
  UNITS: "/BatteryStorage/Units/{facilityId}",
  DEVICE_HEADER: "/BatteryStorage/Units/{deviceId}/Header",
  DEVICE_SUMMARY: "/BatteryStorage/Units/{deviceId}/Summary",
  DEVICE_HEALTH: "/BatteryStorage/Units/{deviceId}/Health",
  DEVICE_CURVES: "/BatteryStorage/Units/{deviceId}/Curves",
  DEVICE_FAULT_FLAGS: "/BatteryStorage/Units/{deviceId}/faultflags",
  DEVICE_FAULTS: "/BatteryStorage/Units/{deviceId}/faults",
  DEVICE_SETTINGS: "/BatteryStorage/Units/{deviceId}/Settings",
} as const;
