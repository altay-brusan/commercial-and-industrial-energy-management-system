export const BESS_ENDPOINTS = {
  TREE: "/BatteryStorage/Tree",
  UNITS: "/BatteryStorage/Units",
  DEVICE_HEADER: "/BatteryStorage/Units/Header",
  DEVICE_SUMMARY: "/BatteryStorage/Units/{deviceId}/Summary",
  DEVICE_HEALTH: "/BatteryStorage/Units/{deviceId}/Health",
  DEVICE_CURVES: "/BatteryStorage/Units/{deviceId}/Curves",
  DEVICE_FAULT_FLAGS: "/BatteryStorage/Units/{deviceId}/FaultFlags",
  DEVICE_NOTIFICATIONS: "/BatteryStorage/Units/{deviceId}/Notifications",
  DEVICE_SETTINGS: "/BatteryStorage/Units/{deviceId}/Settings",
} as const;
