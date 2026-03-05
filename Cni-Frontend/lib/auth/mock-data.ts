import type { Feature, Permission, PermissionGroup, TenantRole } from "./types";
import { users } from "@/lib/maintenance/mock-data";

// ── Features (16 features in 4 categories) ──

export const features: Feature[] = [
  // Core
  { id: "feat-1", key: "Dashboard", name: "Dashboard", description: "Main dashboard and overview panels", category: "Core", isActive: true },
  { id: "feat-2", key: "Alarms", name: "Alarms", description: "Alarm monitoring and management", category: "Core", isActive: true },
  { id: "feat-3", key: "Notifications", name: "Notifications", description: "System notifications and alerts", category: "Core", isActive: true },
  { id: "feat-4", key: "Monitoring", name: "Monitoring", description: "Real-time system monitoring", category: "Core", isActive: true },

  // Energy
  { id: "feat-5", key: "BESS", name: "Battery Storage", description: "Battery energy storage system management", category: "Energy", isActive: true },
  { id: "feat-6", key: "SolarInverters", name: "Solar Inverters", description: "Solar inverter monitoring and control", category: "Energy", isActive: true },
  { id: "feat-7", key: "EnergyTrading", name: "Energy Trading", description: "Energy market trading operations", category: "Energy", isActive: true },
  { id: "feat-8", key: "DemandResponse", name: "Demand Response", description: "Demand response program management", category: "Energy", isActive: true },

  // Intelligence
  { id: "feat-9", key: "Forecasting", name: "Forecasting", description: "Energy generation and demand forecasting", category: "Intelligence", isActive: true },
  { id: "feat-10", key: "Analytics", name: "Analytics", description: "Advanced analytics and reporting", category: "Intelligence", isActive: true },
  { id: "feat-11", key: "Optimization", name: "Optimization", description: "Dispatch optimization algorithms", category: "Intelligence", isActive: true },
  { id: "feat-12", key: "MachineLearning", name: "Machine Learning", description: "ML model management and predictions", category: "Intelligence", isActive: false },

  // Management
  { id: "feat-13", key: "Maintenance", name: "Maintenance", description: "Maintenance task management", category: "Management", isActive: true },
  { id: "feat-14", key: "Configuration", name: "Configuration", description: "System configuration and settings", category: "Management", isActive: true },
  { id: "feat-15", key: "UserManagement", name: "User Management", description: "User accounts and profile management", category: "Management", isActive: true },
  { id: "feat-16", key: "RoleManagement", name: "Role Management", description: "Role and permission management", category: "Management", isActive: true },
];

// ── Permissions (34 permissions with Feature.Action keys) ──

export const permissions: Permission[] = [
  // Dashboard
  { id: "perm-1", key: "Dashboard.View", name: "View Dashboard", description: "Access main dashboard", featureId: "feat-1" },
  { id: "perm-2", key: "Dashboard.Configure", name: "Configure Dashboard", description: "Customize dashboard layout and widgets", featureId: "feat-1" },

  // Alarms
  { id: "perm-3", key: "Alarms.View", name: "View Alarms", description: "View alarm list and details", featureId: "feat-2" },
  { id: "perm-4", key: "Alarms.Acknowledge", name: "Acknowledge Alarms", description: "Acknowledge active alarms", featureId: "feat-2" },
  { id: "perm-5", key: "Alarms.Manage", name: "Manage Alarms", description: "Create, edit, and resolve alarms", featureId: "feat-2" },

  // Notifications
  { id: "perm-6", key: "Notifications.View", name: "View Notifications", description: "View system notifications", featureId: "feat-3" },
  { id: "perm-7", key: "Notifications.Manage", name: "Manage Notifications", description: "Configure notification preferences", featureId: "feat-3" },

  // Monitoring
  { id: "perm-8", key: "Monitoring.View", name: "View Monitoring", description: "Access monitoring dashboards", featureId: "feat-4" },
  { id: "perm-9", key: "Monitoring.Control", name: "Control Devices", description: "Send commands to monitored devices", featureId: "feat-4" },

  // BESS
  { id: "perm-10", key: "BESS.View", name: "View BESS", description: "View battery storage data", featureId: "feat-5" },
  { id: "perm-11", key: "BESS.Control", name: "Control BESS", description: "Control battery charge/discharge", featureId: "feat-5" },
  { id: "perm-12", key: "BESS.Configure", name: "Configure BESS", description: "Configure battery parameters", featureId: "feat-5" },

  // Solar Inverters
  { id: "perm-13", key: "SolarInverters.View", name: "View Inverters", description: "View solar inverter data", featureId: "feat-6" },
  { id: "perm-14", key: "SolarInverters.Control", name: "Control Inverters", description: "Control inverter operations", featureId: "feat-6" },

  // Energy Trading
  { id: "perm-15", key: "EnergyTrading.View", name: "View Trading", description: "View energy trading data", featureId: "feat-7" },
  { id: "perm-16", key: "EnergyTrading.Execute", name: "Execute Trades", description: "Execute energy trades", featureId: "feat-7" },

  // Demand Response
  { id: "perm-17", key: "DemandResponse.View", name: "View DR Programs", description: "View demand response programs", featureId: "feat-8" },
  { id: "perm-18", key: "DemandResponse.Manage", name: "Manage DR Programs", description: "Create and manage DR events", featureId: "feat-8" },

  // Forecasting
  { id: "perm-19", key: "Forecasting.View", name: "View Forecasts", description: "View energy forecasts", featureId: "feat-9" },
  { id: "perm-20", key: "Forecasting.Configure", name: "Configure Forecasting", description: "Configure forecast models", featureId: "feat-9" },

  // Analytics
  { id: "perm-21", key: "Analytics.View", name: "View Analytics", description: "Access analytics dashboards", featureId: "feat-10" },
  { id: "perm-22", key: "Analytics.Export", name: "Export Reports", description: "Export analytics reports", featureId: "feat-10" },

  // Optimization
  { id: "perm-23", key: "Optimization.View", name: "View Optimization", description: "View optimization results", featureId: "feat-11" },
  { id: "perm-24", key: "Optimization.Execute", name: "Run Optimization", description: "Execute optimization algorithms", featureId: "feat-11" },

  // Machine Learning
  { id: "perm-25", key: "MachineLearning.View", name: "View ML Models", description: "View ML model status and results", featureId: "feat-12" },
  { id: "perm-26", key: "MachineLearning.Manage", name: "Manage ML Models", description: "Train and deploy ML models", featureId: "feat-12" },

  // Maintenance
  { id: "perm-27", key: "Maintenance.View", name: "View Maintenance", description: "View maintenance tasks", featureId: "feat-13" },
  { id: "perm-28", key: "Maintenance.Manage", name: "Manage Maintenance", description: "Create and assign maintenance tasks", featureId: "feat-13" },

  // Configuration
  { id: "perm-29", key: "Configuration.View", name: "View Configuration", description: "View system configuration", featureId: "feat-14" },
  { id: "perm-30", key: "Configuration.Manage", name: "Manage Configuration", description: "Modify system settings", featureId: "feat-14" },

  // User Management
  { id: "perm-31", key: "UserManagement.View", name: "View Users", description: "View user accounts", featureId: "feat-15" },
  { id: "perm-32", key: "UserManagement.Manage", name: "Manage Users", description: "Create, edit, and deactivate users", featureId: "feat-15" },

  // Role Management
  { id: "perm-33", key: "RoleManagement.View", name: "View Roles", description: "View roles and permissions", featureId: "feat-16" },
  { id: "perm-34", key: "RoleManagement.Manage", name: "Manage Roles", description: "Create, edit, and delete roles", featureId: "feat-16" },
];

// All permission IDs for convenience
const allPermissionIds = permissions.map((p) => p.id);

// ── Roles (5 system + 2 custom) ──

export const roles: TenantRole[] = [
  {
    id: "role-1",
    tenantId: "tenant-1",
    name: "Administrator",
    description: "Full system access with all permissions. Cannot be modified or deleted.",
    isActive: true,
    isSystem: true,
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-01-01"),
    permissionIds: [...allPermissionIds],
    userIds: ["user-1"],
  },
  {
    id: "role-2",
    tenantId: "tenant-1",
    name: "Tenant Admin",
    description: "Tenant-level administration with user and role management capabilities.",
    isActive: true,
    isSystem: true,
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-06-15"),
    permissionIds: [
      "perm-1", "perm-2", "perm-3", "perm-4", "perm-5", "perm-6", "perm-7",
      "perm-8", "perm-10", "perm-13", "perm-15", "perm-17", "perm-19", "perm-21",
      "perm-22", "perm-27", "perm-28", "perm-29", "perm-30", "perm-31", "perm-32",
      "perm-33", "perm-34",
    ],
    userIds: ["user-1", "user-4"],
  },
  {
    id: "role-3",
    tenantId: "tenant-1",
    name: "Operator",
    description: "Operational access to monitoring, alarms, and device control.",
    isActive: true,
    isSystem: true,
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-03-20"),
    permissionIds: [
      "perm-1", "perm-3", "perm-4", "perm-5", "perm-6", "perm-8", "perm-9",
      "perm-10", "perm-11", "perm-13", "perm-14", "perm-17", "perm-19", "perm-23",
      "perm-27",
    ],
    userIds: ["user-2", "user-3"],
  },
  {
    id: "role-4",
    tenantId: "tenant-1",
    name: "User",
    description: "Standard user access with view permissions and basic interactions.",
    isActive: true,
    isSystem: true,
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-01-01"),
    permissionIds: [
      "perm-1", "perm-3", "perm-6", "perm-8", "perm-10", "perm-13", "perm-15",
      "perm-17", "perm-19", "perm-21", "perm-23", "perm-25", "perm-27", "perm-29",
    ],
    userIds: ["user-5", "user-6"],
  },
  {
    id: "role-5",
    tenantId: "tenant-1",
    name: "Viewer",
    description: "Read-only access to all viewable system data.",
    isActive: true,
    isSystem: true,
    createdAt: new Date("2024-01-01"),
    updatedAt: new Date("2024-01-01"),
    permissionIds: [
      "perm-1", "perm-3", "perm-6", "perm-8", "perm-10", "perm-13", "perm-15",
      "perm-17", "perm-19", "perm-21", "perm-23", "perm-25", "perm-27", "perm-29",
      "perm-31", "perm-33",
    ],
    userIds: ["user-7"],
  },
  {
    id: "role-6",
    tenantId: "tenant-1",
    name: "Field Technician",
    description: "Access to maintenance, BESS, and solar inverter systems for field operations.",
    isActive: true,
    isSystem: false,
    createdAt: new Date("2024-08-10"),
    updatedAt: new Date("2025-01-05"),
    permissionIds: [
      "perm-1", "perm-3", "perm-4", "perm-6", "perm-8", "perm-9", "perm-10",
      "perm-11", "perm-12", "perm-13", "perm-14", "perm-27", "perm-28",
    ],
    userIds: ["user-2", "user-3", "user-5"],
  },
  {
    id: "role-7",
    tenantId: "tenant-1",
    name: "Financial Analyst",
    description: "Access to energy trading, analytics, and reporting for financial analysis.",
    isActive: false,
    isSystem: false,
    createdAt: new Date("2024-11-20"),
    updatedAt: new Date("2025-02-14"),
    permissionIds: [
      "perm-1", "perm-6", "perm-15", "perm-16", "perm-19", "perm-21", "perm-22",
      "perm-23",
    ],
    userIds: ["user-4"],
  },
];

// ── Helper Functions ──

export function getAllFeatures(): Feature[] {
  return features;
}

export function getPermissionsByFeature(): PermissionGroup[] {
  return features.map((feature) => ({
    feature,
    permissions: permissions.filter((p) => p.featureId === feature.id),
  }));
}

export function getAllRoles(): TenantRole[] {
  return roles;
}

export function getRoleById(id: string): TenantRole | undefined {
  return roles.find((r) => r.id === id);
}

export function getUsersForRole(role: TenantRole) {
  return users.filter((u) => role.userIds.includes(u.id));
}

export function getPermissionsForRole(role: TenantRole): Permission[] {
  return permissions.filter((p) => role.permissionIds.includes(p.id));
}

export function getRoleCount() {
  return {
    total: roles.length,
    active: roles.filter((r) => r.isActive).length,
    system: roles.filter((r) => r.isSystem).length,
    custom: roles.filter((r) => !r.isSystem).length,
  };
}
