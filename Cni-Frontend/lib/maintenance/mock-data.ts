import {
  User,
  Asset,
  MaintenanceTask,
  StatusChange,
  TaskNote,
  ScheduledMaintenance,
  ChecklistItem,
  WorkSession,
} from "./types";

// Mock Users
export const users: User[] = [
  {
    id: "user-1",
    name: "John Doe",
    email: "john@energy.com",
    initials: "JD",
    role: "Operations Manager",
    department: "Operations",
    phone: "+1 (555) 123-4567",
  },
  {
    id: "user-2",
    name: "Jane Smith",
    email: "jane@energy.com",
    initials: "JS",
    role: "Senior Technician",
    department: "Field Services",
    phone: "+1 (555) 234-5678",
  },
  {
    id: "user-3",
    name: "Mike Johnson",
    email: "mike@energy.com",
    initials: "MJ",
    role: "BESS Specialist",
    department: "Battery Systems",
    phone: "+1 (555) 345-6789",
  },
  {
    id: "user-4",
    name: "Sarah Wilson",
    email: "sarah@energy.com",
    initials: "SW",
    role: "Electrical Engineer",
    department: "Engineering",
    phone: "+1 (555) 456-7890",
  },
  {
    id: "user-5",
    name: "Tom Brown",
    email: "tom@energy.com",
    initials: "TB",
    role: "Grid Technician",
    department: "Grid Operations",
    phone: "+1 (555) 567-8901",
  },
];

// Mock Assets
export const assets: Asset[] = [
  {
    id: "asset-1",
    name: "Solar Array A",
    type: "solar",
    location: "Building A - Rooftop",
    capacity: "250 kW",
    status: "operational",
    lastMaintenance: new Date("2025-11-15"),
    serialNumber: "SA-2023-001",
  },
  {
    id: "asset-2",
    name: "Solar Array B",
    type: "solar",
    location: "Building B - Rooftop",
    capacity: "180 kW",
    status: "operational",
    lastMaintenance: new Date("2025-12-01"),
    serialNumber: "SA-2023-002",
  },
  {
    id: "asset-3",
    name: "BESS Unit #1",
    type: "bess",
    location: "Ground Floor",
    capacity: "500 kWh",
    status: "warning",
    lastMaintenance: new Date("2025-10-20"),
    serialNumber: "BESS-2022-001",
  },
  {
    id: "asset-4",
    name: "BESS Unit #3",
    type: "bess",
    location: "Storage Facility",
    capacity: "750 kWh",
    status: "maintenance",
    lastMaintenance: new Date("2025-09-10"),
    serialNumber: "BESS-2023-003",
  },
  {
    id: "asset-5",
    name: "Inverter #2",
    type: "inverter",
    location: "Solar Array B",
    capacity: "100 kW",
    status: "operational",
    lastMaintenance: new Date("2026-02-05"),
    serialNumber: "INV-2022-002",
  },
  {
    id: "asset-6",
    name: "Grid Connection Point",
    type: "grid",
    location: "Main Building",
    capacity: "1 MW",
    status: "warning",
    lastMaintenance: new Date("2025-08-15"),
    serialNumber: "GCP-2021-001",
  },
];

// Mock Maintenance Tasks
export const maintenanceTasks: MaintenanceTask[] = [
  {
    id: "task-1",
    title: "Solar Panel Cleaning",
    description:
      "Quarterly cleaning of solar panels on Building A rooftop. Check for debris and dust accumulation.",
    status: "pending",
    priority: "medium",
    assignedTo: users[1],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-10"),
    dueDate: new Date("2026-02-15"),
    relatedAsset: assets[0],
    checklist: [
      { id: "cl-a1", label: "Check weather conditions (no rain expected)", completed: false },
      { id: "cl-a2", label: "Prepare cleaning equipment and safety gear", completed: false },
      { id: "cl-a3", label: "Secure roof access and set up safety lines", completed: false },
      { id: "cl-a4", label: "Inspect panels for visible damage before cleaning", completed: false },
      { id: "cl-a5", label: "Clean panels using approved methods", completed: false },
      { id: "cl-a6", label: "Document any damage or issues found", completed: false },
      { id: "cl-a7", label: "Take before/after photos", completed: false },
      { id: "cl-a8", label: "Verify power output improvement", completed: false },
    ],
  },
  {
    id: "task-2",
    title: "BESS Temperature Sensor Calibration",
    description:
      "Calibrate temperature sensors on BESS Unit #1. Recent readings show potential drift.",
    status: "in_progress",
    priority: "high",
    assignedTo: users[2],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-08"),
    dueDate: new Date("2026-02-12"),
    relatedAsset: assets[2],
    relatedAlarm: {
      id: "ALM-2024-0847",
      code: "BESS-TEMP-DRIFT",
      message: "Temperature sensor T-03 showing inconsistent readings",
      severity: "high",
      source: "BESS Unit #1",
      triggeredAt: new Date("2026-02-08T07:23:00"),
      value: "2.5°C deviation",
      threshold: "±1.0°C",
      acknowledgedAt: new Date("2026-02-08T07:45:00"),
      acknowledgedBy: users[0],
    },
    checklist: [
      { id: "cl-1", label: "Review sensor specifications and calibration requirements", completed: true, completedBy: users[2], completedAt: new Date("2026-02-10T09:30:00") },
      { id: "cl-2", label: "Gather calibration equipment and reference sensors", completed: true, completedBy: users[2], completedAt: new Date("2026-02-10T10:00:00") },
      { id: "cl-3", label: "Isolate BESS unit for safe access", completed: true, completedBy: users[2], completedAt: new Date("2026-02-10T10:30:00") },
      { id: "cl-4", label: "Record current sensor readings", completed: false },
      { id: "cl-5", label: "Perform calibration procedure on each sensor", completed: false },
      { id: "cl-6", label: "Verify readings against reference values", completed: false },
      { id: "cl-7", label: "Document calibration results", completed: false },
      { id: "cl-8", label: "Restore BESS unit to normal operation", completed: false },
    ],
    workSessions: [
      { id: "ws-1", taskId: "task-2", startedAt: new Date("2026-02-10T09:00:00"), endedAt: new Date("2026-02-10T11:30:00"), user: users[2] },
      { id: "ws-2", taskId: "task-2", startedAt: new Date("2026-02-10T13:00:00"), endedAt: new Date("2026-02-10T15:00:00"), user: users[2] },
    ],
  },
  {
    id: "task-3",
    title: "Inverter Firmware Update",
    description:
      "Update firmware on Inverter #2 to version 3.2.1. Backup current configuration before update.",
    status: "completed",
    priority: "low",
    assignedTo: users[3],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-01"),
    dueDate: new Date("2026-02-05"),
    relatedAsset: assets[4],
  },
  {
    id: "task-4",
    title: "Grid Connection Inspection",
    description:
      "Annual inspection of grid connection point. Check all connections and safety equipment.",
    status: "overdue",
    priority: "critical",
    assignedTo: users[4],
    assignedBy: users[0],
    assignedDate: new Date("2026-01-25"),
    dueDate: new Date("2026-02-01"),
    relatedAsset: assets[5],
    relatedAlarm: {
      id: "ALM-2024-0812",
      code: "GRID-VOLT-FLUCT",
      message: "Grid voltage fluctuation detected - exceeded safe operating range",
      severity: "critical",
      source: "Grid Connection Point",
      triggeredAt: new Date("2026-01-25T14:12:00"),
      value: "398V (fluctuating)",
      threshold: "400V ±5%",
      acknowledgedAt: new Date("2026-01-25T14:18:00"),
      acknowledgedBy: users[0],
    },
  },
  {
    id: "task-5",
    title: "BESS Ventilation System Check",
    description:
      "Inspect and clean ventilation system for BESS Unit #3. Replace filters if necessary.",
    status: "pending",
    priority: "high",
    assignedTo: users[1],
    assignedBy: users[2],
    assignedDate: new Date("2026-02-09"),
    dueDate: new Date("2026-02-14"),
    relatedAsset: assets[3],
  },
  {
    id: "task-6",
    title: "Solar Array B Performance Audit",
    description:
      "Conduct performance audit on Solar Array B. Compare actual vs expected output.",
    status: "pending",
    priority: "medium",
    assignedTo: users[3],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-11"),
    dueDate: new Date("2026-02-20"),
    relatedAsset: assets[1],
  },
  {
    id: "task-7",
    title: "Cable Insulation Testing",
    description:
      "Test cable insulation resistance for all main power cables. Document any degradation.",
    status: "in_progress",
    priority: "medium",
    assignedTo: users[4],
    assignedBy: users[1],
    assignedDate: new Date("2026-02-07"),
    dueDate: new Date("2026-02-13"),
  },
  {
    id: "task-8",
    title: "Emergency Generator Test",
    description:
      "Monthly test run of emergency backup generator. Check fuel levels and start sequence.",
    status: "pending",
    priority: "high",
    assignedTo: users[2],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-12"),
    dueDate: new Date("2026-02-18"),
  },
  {
    id: "task-9",
    title: "Transformer Oil Analysis",
    description:
      "Collect oil samples from main transformer for laboratory analysis. Check for contamination.",
    status: "completed",
    priority: "medium",
    assignedTo: users[1],
    assignedBy: users[0],
    assignedDate: new Date("2026-01-28"),
    dueDate: new Date("2026-02-03"),
  },
  {
    id: "task-10",
    title: "Lightning Protection Inspection",
    description:
      "Annual inspection of lightning protection system. Verify all connections and grounding.",
    status: "pending",
    priority: "low",
    assignedTo: users[4],
    assignedBy: users[2],
    assignedDate: new Date("2026-02-13"),
    dueDate: new Date("2026-02-25"),
    relatedAsset: assets[0],
  },
  {
    id: "task-11",
    title: "SCADA System Backup",
    description:
      "Weekly backup of SCADA system configuration and historical data to secure storage.",
    status: "completed",
    priority: "medium",
    assignedTo: users[3],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-05"),
    dueDate: new Date("2026-02-07"),
  },
  {
    id: "task-12",
    title: "Power Meter Calibration",
    description:
      "Calibrate all revenue-grade power meters. Document readings before and after calibration.",
    status: "in_progress",
    priority: "high",
    assignedTo: users[1],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-10"),
    dueDate: new Date("2026-02-16"),
    relatedAsset: assets[5],
  },
  {
    id: "task-13",
    title: "Cooling System Maintenance",
    description:
      "Service HVAC cooling system for battery storage room. Replace filters and check refrigerant.",
    status: "overdue",
    priority: "critical",
    assignedTo: users[2],
    assignedBy: users[1],
    assignedDate: new Date("2026-01-20"),
    dueDate: new Date("2026-01-30"),
    relatedAsset: assets[3],
  },
  {
    id: "task-14",
    title: "Safety Sign Replacement",
    description:
      "Replace faded and damaged safety signs throughout the facility. Update to current standards.",
    status: "pending",
    priority: "low",
    assignedTo: users[4],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-02-28"),
  },
  {
    id: "task-15",
    title: "String Combiner Box Inspection",
    description:
      "Inspect all string combiner boxes on Solar Array A. Check fuses and connections.",
    status: "pending",
    priority: "medium",
    assignedTo: users[3],
    assignedBy: users[2],
    assignedDate: new Date("2026-02-11"),
    dueDate: new Date("2026-02-19"),
    relatedAsset: assets[0],
  },
  {
    id: "task-16",
    title: "Annual Thermal Imaging Survey",
    description:
      "Scheduled thermal imaging inspection of all electrical connections and components.",
    status: "scheduled",
    priority: "medium",
    assignedTo: users[1],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-03-01"),
    relatedAsset: assets[0],
  },
  {
    id: "task-17",
    title: "Battery Capacity Test",
    description:
      "Full discharge/charge cycle test to verify BESS capacity and health metrics.",
    status: "scheduled",
    priority: "high",
    assignedTo: users[2],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-03-05"),
    relatedAsset: assets[2],
  },
  {
    id: "task-18",
    title: "Quarterly Safety Audit",
    description:
      "Comprehensive safety audit including PPE inspection, emergency procedures review.",
    status: "scheduled",
    priority: "high",
    assignedTo: users[4],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-03-10"),
  },
  {
    id: "task-19",
    title: "Inverter Efficiency Analysis",
    description:
      "Detailed efficiency analysis of all inverters. Compare performance curves with specifications.",
    status: "scheduled",
    priority: "low",
    assignedTo: users[3],
    assignedBy: users[1],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-03-15"),
    relatedAsset: assets[4],
  },
  {
    id: "task-20",
    title: "Ground Fault Detection System Test",
    description:
      "Test ground fault detection and isolation systems across all arrays.",
    status: "scheduled",
    priority: "critical",
    assignedTo: users[2],
    assignedBy: users[0],
    assignedDate: new Date("2026-02-14"),
    dueDate: new Date("2026-02-28"),
    relatedAsset: assets[5],
  },
];

// Mock Status Changes for task-2
export const statusChanges: StatusChange[] = [
  {
    id: "change-1",
    taskId: "task-2",
    fromStatus: null,
    toStatus: "pending",
    changedBy: users[0],
    changedAt: new Date("2026-02-08T09:00:00"),
  },
  {
    id: "change-2",
    taskId: "task-2",
    fromStatus: "pending",
    toStatus: "in_progress",
    changedBy: users[2],
    changedAt: new Date("2026-02-10T14:30:00"),
  },
];

// Mock Notes for task-2
export const taskNotes: TaskNote[] = [
  {
    id: "note-1",
    taskId: "task-2",
    content:
      "Initial assessment completed. Sensor T-03 shows 2.5 degree deviation from reference.",
    author: users[2],
    createdAt: new Date("2026-02-10T15:00:00"),
  },
  {
    id: "note-2",
    taskId: "task-2",
    content: "Ordered replacement sensor from supplier. Expected delivery: Feb 11.",
    author: users[2],
    createdAt: new Date("2026-02-10T16:30:00"),
  },
  {
    id: "note-3",
    taskId: "task-2",
    content: "Priority marked as high due to potential safety concerns.",
    author: users[0],
    createdAt: new Date("2026-02-08T09:15:00"),
  },
];

// Mock Scheduled Maintenances
export const scheduledMaintenances: ScheduledMaintenance[] = [
  {
    id: "sched-0",
    title: "Morning System Check",
    scheduledDate: new Date("2026-02-04"),
    assetName: "All Systems",
    recurrence: "daily",
  },
  {
    id: "sched-0b",
    title: "BESS Voltage Monitoring",
    scheduledDate: new Date("2026-02-04"),
    assetName: "BESS Unit #1",
  },
  {
    id: "sched-1",
    title: "Thermal Camera Inspection",
    scheduledDate: new Date("2026-02-05"),
    assetName: "Solar Array A",
  },
  {
    id: "sched-2",
    title: "Battery Cell Balancing",
    scheduledDate: new Date("2026-02-05"),
    assetName: "BESS Unit #1",
    recurrence: "monthly",
  },
  {
    id: "sched-3",
    title: "Inverter Firmware Check",
    scheduledDate: new Date("2026-02-06"),
    assetName: "Inverter #2",
  },
  {
    id: "sched-4",
    title: "Ground Fault Test",
    scheduledDate: new Date("2026-02-07"),
    assetName: "Grid Connection",
    recurrence: "weekly",
  },
  {
    id: "sched-5",
    title: "Panel Voltage Reading",
    scheduledDate: new Date("2026-02-10"),
    assetName: "Solar Array B",
  },
  {
    id: "sched-6",
    title: "Cooling System Check",
    scheduledDate: new Date("2026-02-10"),
    assetName: "BESS Unit #3",
    recurrence: "monthly",
  },
  {
    id: "sched-7",
    title: "Solar Panel Cleaning",
    scheduledDate: new Date("2026-02-12"),
    assetName: "Solar Array A",
    recurrence: "quarterly",
  },
  {
    id: "sched-8",
    title: "Inverter Inspection",
    scheduledDate: new Date("2026-02-15"),
    assetName: "All Inverters",
    recurrence: "monthly",
  },
  {
    id: "sched-9",
    title: "BESS Health Check",
    scheduledDate: new Date("2026-02-18"),
    assetName: "BESS Unit #1",
    recurrence: "weekly",
  },
  {
    id: "sched-10",
    title: "Grid Sync Test",
    scheduledDate: new Date("2026-02-20"),
    assetName: "Grid Connection",
    recurrence: "monthly",
  },
  {
    id: "sched-11",
    title: "Safety Equipment Check",
    scheduledDate: new Date("2026-02-22"),
    assetName: "All Sites",
    recurrence: "monthly",
  },
  {
    id: "sched-12",
    title: "Annual Transformer Inspection",
    scheduledDate: new Date("2026-02-25"),
    assetName: "Main Transformer",
    recurrence: "yearly",
  },
  {
    id: "sched-13",
    title: "Cable Insulation Test",
    scheduledDate: new Date("2026-02-28"),
    assetName: "All Sites",
  },
];

// Helper functions
export function getTaskById(id: string): MaintenanceTask | undefined {
  return maintenanceTasks.find((task) => task.id === id);
}

export function getStatusChangesForTask(taskId: string): StatusChange[] {
  return statusChanges
    .filter((change) => change.taskId === taskId)
    .sort((a, b) => b.changedAt.getTime() - a.changedAt.getTime());
}

export function getNotesForTask(taskId: string): TaskNote[] {
  return taskNotes
    .filter((note) => note.taskId === taskId)
    .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
}

export function getScheduledMaintenancesForDate(date: Date): ScheduledMaintenance[] {
  return scheduledMaintenances.filter(
    (m) =>
      m.scheduledDate.getDate() === date.getDate() &&
      m.scheduledDate.getMonth() === date.getMonth() &&
      m.scheduledDate.getFullYear() === date.getFullYear()
  );
}

export function getDatesWithMaintenances(): Date[] {
  return [...new Set(scheduledMaintenances.map((m) => m.scheduledDate))];
}

export function getChecklistForTask(taskId: string): ChecklistItem[] {
  const task = maintenanceTasks.find((t) => t.id === taskId);
  return task?.checklist || [];
}

export function getWorkSessionsForTask(taskId: string): WorkSession[] {
  const task = maintenanceTasks.find((t) => t.id === taskId);
  return task?.workSessions || [];
}
