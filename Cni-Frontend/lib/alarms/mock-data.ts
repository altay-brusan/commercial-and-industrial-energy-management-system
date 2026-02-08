import type { Alarm, SystemNotification } from "./types";
import { users, assets, maintenanceTasks } from "@/lib/maintenance/mock-data";

// Mock Alarms (~15 alarms)
export const alarms: Alarm[] = [
  // Critical alarms (3)
  {
    id: "ALM-2024-0812",
    code: "GRID-VOLT-FLUCT",
    message: "Grid voltage fluctuation detected - exceeded safe operating range",
    severity: "critical",
    status: "acknowledged",
    source: "Grid Connection Point",
    sourceType: "grid",
    triggeredAt: new Date("2026-01-25T14:12:00"),
    value: "398V (fluctuating)",
    threshold: "400V ±5%",
    acknowledgedAt: new Date("2026-01-25T14:18:00"),
    acknowledgedBy: users[0],
    relatedAsset: assets[5],
    relatedMaintenanceTaskId: "task-4",
    location: "Main Building",
  },
  {
    id: "ALM-2024-0901",
    code: "BESS-SOC-CRIT",
    message: "Battery State of Charge critically low - immediate attention required",
    severity: "critical",
    status: "active",
    source: "BESS Unit #3",
    sourceType: "bess",
    triggeredAt: new Date("2026-02-05T08:15:00"),
    value: "5%",
    threshold: ">10%",
    relatedAsset: assets[3],
    location: "Storage Facility",
  },
  {
    id: "ALM-2024-0905",
    code: "GRID-FREQ-DEV",
    message: "Grid frequency deviation beyond acceptable limits",
    severity: "critical",
    status: "active",
    source: "Grid Connection Point",
    sourceType: "grid",
    triggeredAt: new Date("2026-02-05T10:30:00"),
    value: "49.2 Hz",
    threshold: "50 Hz ±0.5 Hz",
    relatedAsset: assets[5],
    location: "Main Building",
  },
  // High alarms (4)
  {
    id: "ALM-2024-0847",
    code: "BESS-TEMP-DRIFT",
    message: "Temperature sensor T-03 showing inconsistent readings",
    severity: "high",
    status: "acknowledged",
    source: "BESS Unit #1",
    sourceType: "bess",
    triggeredAt: new Date("2026-02-08T07:23:00"),
    value: "2.5°C deviation",
    threshold: "±1.0°C",
    acknowledgedAt: new Date("2026-02-08T07:45:00"),
    acknowledgedBy: users[0],
    relatedAsset: assets[2],
    relatedMaintenanceTaskId: "task-2",
    location: "Ground Floor",
  },
  {
    id: "ALM-2024-0856",
    code: "BESS-OVERHEAT",
    message: "Battery overheating - temperature exceeding safe limits",
    severity: "high",
    status: "active",
    source: "BESS Unit #3",
    sourceType: "bess",
    triggeredAt: new Date("2026-02-05T06:42:00"),
    value: "48°C",
    threshold: "<45°C",
    relatedAsset: assets[3],
    location: "Storage Facility",
  },
  {
    id: "ALM-2024-0870",
    code: "INV-OVERCURR",
    message: "Inverter overcurrent detected - output exceeding rated capacity",
    severity: "high",
    status: "active",
    source: "Inverter #2",
    sourceType: "inverter",
    triggeredAt: new Date("2026-02-04T16:20:00"),
    value: "112A",
    threshold: "<100A",
    relatedAsset: assets[4],
    location: "Solar Array B",
  },
  {
    id: "ALM-2024-0882",
    code: "SOL-HOTSPOT",
    message: "Solar panel hotspot detected - potential cell degradation",
    severity: "high",
    status: "active",
    source: "Solar Array A",
    sourceType: "solar",
    triggeredAt: new Date("2026-02-05T11:15:00"),
    value: "85°C (cell)",
    threshold: "<70°C",
    relatedAsset: assets[0],
    location: "Building A - Rooftop",
  },
  // Medium alarms (5)
  {
    id: "ALM-2024-0830",
    code: "SOL-OUTPUT-LOW",
    message: "Solar output below expected performance ratio",
    severity: "medium",
    status: "active",
    source: "Solar Array B",
    sourceType: "solar",
    triggeredAt: new Date("2026-02-03T09:45:00"),
    value: "62% PR",
    threshold: ">75% PR",
    relatedAsset: assets[1],
    relatedMaintenanceTaskId: "task-6",
    location: "Building B - Rooftop",
  },
  {
    id: "ALM-2024-0840",
    code: "INV-EFF-DROP",
    message: "Inverter efficiency dropped below optimal range",
    severity: "medium",
    status: "acknowledged",
    source: "Inverter #2",
    sourceType: "inverter",
    triggeredAt: new Date("2026-02-02T14:30:00"),
    value: "91.2%",
    threshold: ">95%",
    acknowledgedAt: new Date("2026-02-02T15:10:00"),
    acknowledgedBy: users[3],
    relatedAsset: assets[4],
    location: "Solar Array B",
  },
  {
    id: "ALM-2024-0860",
    code: "SYS-COMM-LAT",
    message: "Communication latency detected on SCADA link",
    severity: "medium",
    status: "active",
    source: "SCADA System",
    sourceType: "system",
    triggeredAt: new Date("2026-02-04T11:00:00"),
    value: "850ms",
    threshold: "<500ms",
    location: "Main Building",
  },
  {
    id: "ALM-2024-0875",
    code: "BESS-CELL-IMB",
    message: "Battery cell voltage imbalance detected",
    severity: "medium",
    status: "active",
    source: "BESS Unit #1",
    sourceType: "bess",
    triggeredAt: new Date("2026-02-05T09:30:00"),
    value: "0.15V delta",
    threshold: "<0.1V",
    relatedAsset: assets[2],
    location: "Ground Floor",
  },
  {
    id: "ALM-2024-0890",
    code: "SOL-SHADE-DET",
    message: "Partial shading detected on solar string",
    severity: "medium",
    status: "resolved",
    source: "Solar Array A",
    sourceType: "solar",
    triggeredAt: new Date("2026-02-01T10:00:00"),
    value: "String 3 - 30% loss",
    threshold: "<10% loss",
    acknowledgedAt: new Date("2026-02-01T10:15:00"),
    acknowledgedBy: users[1],
    resolvedAt: new Date("2026-02-01T14:00:00"),
    resolvedBy: users[1],
    relatedAsset: assets[0],
    location: "Building A - Rooftop",
  },
  // Low alarms (3)
  {
    id: "ALM-2024-0810",
    code: "SYS-LOG-FULL",
    message: "System log storage reaching capacity threshold",
    severity: "low",
    status: "resolved",
    source: "SCADA System",
    sourceType: "system",
    triggeredAt: new Date("2026-01-28T16:00:00"),
    value: "87%",
    threshold: "<85%",
    acknowledgedAt: new Date("2026-01-28T16:30:00"),
    acknowledgedBy: users[3],
    resolvedAt: new Date("2026-01-29T09:00:00"),
    resolvedBy: users[3],
    location: "Main Building",
  },
  {
    id: "ALM-2024-0895",
    code: "SOL-DIRT-DET",
    message: "Soiling index above threshold - cleaning recommended",
    severity: "low",
    status: "active",
    source: "Solar Array A",
    sourceType: "solar",
    triggeredAt: new Date("2026-02-04T08:00:00"),
    value: "Soiling: 12%",
    threshold: "<8%",
    relatedAsset: assets[0],
    relatedMaintenanceTaskId: "task-1",
    location: "Building A - Rooftop",
  },
  {
    id: "ALM-2024-0898",
    code: "SYS-NTP-SYNC",
    message: "NTP time synchronization offset detected",
    severity: "low",
    status: "acknowledged",
    source: "SCADA System",
    sourceType: "system",
    triggeredAt: new Date("2026-02-05T07:00:00"),
    value: "2.3s offset",
    threshold: "<1s",
    acknowledgedAt: new Date("2026-02-05T07:30:00"),
    acknowledgedBy: users[3],
    location: "Main Building",
  },
];

// Mock System Notifications (~15 notifications)
export const notifications: SystemNotification[] = [
  // Today
  {
    id: "notif-1",
    type: "system_alert",
    title: "Critical Alarm: Battery SOC Critically Low",
    message: "BESS Unit #3 state of charge has dropped to 5%. Immediate attention required to prevent deep discharge damage.",
    timestamp: new Date("2026-02-05T08:15:00"),
    read: false,
    relatedAlarmId: "ALM-2024-0901",
  },
  {
    id: "notif-2",
    type: "system_alert",
    title: "Critical Alarm: Grid Frequency Deviation",
    message: "Grid frequency has deviated to 49.2 Hz. Anti-islanding protection may activate if deviation continues.",
    timestamp: new Date("2026-02-05T10:30:00"),
    read: false,
    relatedAlarmId: "ALM-2024-0905",
  },
  {
    id: "notif-3",
    type: "schedule_reminder",
    title: "Upcoming: Battery Capacity Test",
    message: "Battery capacity test for BESS Unit #1 is scheduled for March 5. Please ensure pre-test checklist is completed.",
    timestamp: new Date("2026-02-05T09:00:00"),
    read: false,
    relatedTaskId: "task-17",
  },
  {
    id: "notif-4",
    type: "alarm_resolved",
    title: "Alarm Resolved: Solar Panel Shading",
    message: "Partial shading alarm on Solar Array A (String 3) has been resolved. Output restored to normal levels.",
    timestamp: new Date("2026-02-05T07:00:00"),
    read: true,
    relatedAlarmId: "ALM-2024-0890",
  },
  // Yesterday
  {
    id: "notif-5",
    type: "maintenance_completed",
    title: "Maintenance Completed: Inverter Firmware Update",
    message: "Inverter #2 firmware has been successfully updated to version 3.2.1. All post-update checks passed.",
    timestamp: new Date("2026-02-04T16:30:00"),
    read: true,
    relatedTaskId: "task-3",
  },
  {
    id: "notif-6",
    type: "system_alert",
    title: "High Alarm: Inverter Overcurrent",
    message: "Inverter #2 output current exceeded rated capacity at 112A. Automatic derating applied.",
    timestamp: new Date("2026-02-04T16:20:00"),
    read: true,
    relatedAlarmId: "ALM-2024-0870",
  },
  {
    id: "notif-7",
    type: "system_update",
    title: "System Update: SCADA Dashboard v2.4",
    message: "SCADA monitoring dashboard has been updated to version 2.4. New features include enhanced alarm correlation and trend analysis.",
    timestamp: new Date("2026-02-04T10:00:00"),
    read: false,
  },
  // This Week
  {
    id: "notif-8",
    type: "schedule_reminder",
    title: "Overdue: Grid Connection Inspection",
    message: "Grid Connection Inspection (task-4) was due on Feb 1 and is now overdue. Please prioritize completion.",
    timestamp: new Date("2026-02-03T08:00:00"),
    read: true,
    relatedTaskId: "task-4",
  },
  {
    id: "notif-9",
    type: "system_alert",
    title: "Medium Alarm: Solar Output Below Expected",
    message: "Solar Array B performance ratio dropped to 62%. Inspection recommended to identify root cause.",
    timestamp: new Date("2026-02-03T09:45:00"),
    read: true,
    relatedAlarmId: "ALM-2024-0830",
  },
  {
    id: "notif-10",
    type: "maintenance_completed",
    title: "Maintenance Completed: SCADA System Backup",
    message: "Weekly SCADA system backup completed successfully. All configuration and historical data secured.",
    timestamp: new Date("2026-02-02T17:00:00"),
    read: true,
    relatedTaskId: "task-11",
  },
  {
    id: "notif-11",
    type: "alarm_resolved",
    title: "Alarm Resolved: System Log Storage",
    message: "System log storage alarm has been resolved. Log rotation configured and old logs archived.",
    timestamp: new Date("2026-02-02T09:00:00"),
    read: true,
    relatedAlarmId: "ALM-2024-0810",
  },
  // Older
  {
    id: "notif-12",
    type: "system_update",
    title: "System Update: Firmware Repository Updated",
    message: "New firmware versions available for inverters (v3.2.1) and BESS controllers (v5.0.3). Schedule updates as needed.",
    timestamp: new Date("2026-01-29T11:00:00"),
    read: true,
  },
  {
    id: "notif-13",
    type: "schedule_reminder",
    title: "Reminder: Cooling System Maintenance Overdue",
    message: "Cooling System Maintenance for BESS Unit #3 was due Jan 30. This is critical for battery health.",
    timestamp: new Date("2026-01-30T08:00:00"),
    read: true,
    relatedTaskId: "task-13",
  },
  {
    id: "notif-14",
    type: "maintenance_completed",
    title: "Maintenance Completed: Transformer Oil Analysis",
    message: "Oil analysis results received. All parameters within acceptable range. Next analysis scheduled in 6 months.",
    timestamp: new Date("2026-01-28T14:00:00"),
    read: true,
    relatedTaskId: "task-9",
  },
  {
    id: "notif-15",
    type: "system_alert",
    title: "Critical Alarm: Grid Voltage Fluctuation",
    message: "Grid voltage fluctuation detected at Grid Connection Point. Voltage measured at 398V with fluctuation pattern.",
    timestamp: new Date("2026-01-25T14:12:00"),
    read: true,
    relatedAlarmId: "ALM-2024-0812",
  },
];

// Helper functions
export function getAlarmById(id: string): Alarm | undefined {
  return alarms.find((alarm) => alarm.id === id);
}

export function getAlarmsByAsset(assetId: string): Alarm[] {
  return alarms.filter((alarm) => alarm.relatedAsset?.id === assetId);
}

export function getAlarmsBySource(source: string): Alarm[] {
  return alarms.filter((alarm) => alarm.source === source);
}

export function getActiveAlarmCount(): number {
  return alarms.filter((alarm) => alarm.status === "active").length;
}

export function getCriticalAlarmCount(): number {
  return alarms.filter((alarm) => alarm.severity === "critical" && alarm.status !== "resolved").length;
}

export function getUnreadNotificationCount(): number {
  return notifications.filter((n) => !n.read).length;
}

export function getRelatedMaintenanceTask(taskId: string) {
  return maintenanceTasks.find((t) => t.id === taskId);
}

export function getAlarmLocations(): string[] {
  const locations = new Set(alarms.map((a) => a.location).filter(Boolean) as string[]);
  return Array.from(locations).sort();
}
