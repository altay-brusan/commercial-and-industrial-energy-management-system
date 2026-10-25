import type { Feature, Permission, PermissionGroup, TenantRole } from "./types";
import { users } from "@/lib/maintenance/mock-data";

// ── Features (16 features in 4 categories) ──

export const features: Feature[] = [
  // Core
  { id: "feat-1", key: "Portfolio", name: "Portfolio", description: "Portfolio overview and management", category: "Core", isActive: true },
  { id: "feat-2", key: "Dashboard", name: "Dashboard", description: "Main dashboard and overview panels", category: "Core", isActive: true },
  { id: "feat-3", key: "Monitoring", name: "Monitoring", description: "Real-time system monitoring", category: "Core", isActive: true },
  { id: "feat-4", key: "FacilityMap", name: "Facility Map", description: "Interactive facility map view", category: "Core", isActive: true },

  // Energy
  { id: "feat-5", key: "Solar", name: "Solar", description: "Solar energy monitoring and management", category: "Energy", isActive: true },
  { id: "feat-6", key: "Bess", name: "BESS", description: "Battery energy storage system management", category: "Energy", isActive: true },
  { id: "feat-7", key: "Pcs", name: "PCS", description: "Power conversion system management", category: "Energy", isActive: true },
  { id: "feat-8", key: "GridAndLoads", name: "Grid & Loads", description: "Grid connection and load management", category: "Energy", isActive: true },
  { id: "feat-9", key: "Scada", name: "SCADA", description: "Supervisory control and data acquisition", category: "Energy", isActive: true },

  // Intelligence
  { id: "feat-10", key: "Analytics", name: "Analytics", description: "Advanced analytics and data insights", category: "Intelligence", isActive: true },
  { id: "feat-11", key: "Financials", name: "Financials", description: "Financial data and energy market prices", category: "Intelligence", isActive: true },
  { id: "feat-12", key: "Reporting", name: "Reporting", description: "Report generation and export", category: "Intelligence", isActive: true },

  // Management
  { id: "feat-13", key: "Operations", name: "Operations", description: "Operational management and maintenance", category: "Management", isActive: true },
  { id: "feat-14", key: "Settings", name: "Settings", description: "System configuration and entity management", category: "Management", isActive: true },
  { id: "feat-15", key: "UserManagement", name: "User Management", description: "User accounts and access management", category: "Management", isActive: true },
  { id: "feat-16", key: "Audit", name: "Audit", description: "Audit logs and activity tracking", category: "Management", isActive: true },
];

// ── Permissions (34 permissions with Feature.Action keys) ──

export const permissions: Permission[] = [
  // Core
  { id: "perm-1", key: "Portfolio.View", name: "View Portfolio", description: "Access portfolio overview", featureId: "feat-1" },
  { id: "perm-2", key: "Dashboard.View", name: "View Dashboard", description: "Access main dashboard", featureId: "feat-2" },
  { id: "perm-3", key: "Monitoring.View", name: "View Monitoring", description: "Access monitoring dashboards", featureId: "feat-3" },
  { id: "perm-4", key: "FacilityMap.View", name: "View Facility Map", description: "Access facility map", featureId: "feat-4" },

  // Energy
  { id: "perm-5", key: "Solar.View", name: "View Solar", description: "View solar energy data", featureId: "feat-5" },
  { id: "perm-6", key: "Bess.View", name: "View BESS", description: "View battery storage data", featureId: "feat-6" },
  { id: "perm-7", key: "Pcs.View", name: "View PCS", description: "View power conversion data", featureId: "feat-7" },
  { id: "perm-8", key: "GridAndLoads.View", name: "View Grid & Loads", description: "View grid and load data", featureId: "feat-8" },
  { id: "perm-9", key: "Scada.View", name: "View SCADA", description: "View SCADA data", featureId: "feat-9" },
  { id: "perm-10", key: "Scada.Control", name: "Control SCADA", description: "Send SCADA control commands", featureId: "feat-9" },

  // Intelligence
  { id: "perm-11", key: "Analytics.View", name: "View Analytics", description: "Access analytics dashboards", featureId: "feat-10" },
  { id: "perm-12", key: "Financials.View", name: "View Financials", description: "View financial data", featureId: "feat-11" },
  { id: "perm-13", key: "Reporting.View", name: "View Reporting", description: "View reports", featureId: "feat-12" },
  { id: "perm-14", key: "Reporting.Export", name: "Export Reports", description: "Export report data", featureId: "feat-12" },

  // Operations
  { id: "perm-15", key: "Operations.View", name: "View Operations", description: "View operational data", featureId: "feat-13" },
  { id: "perm-16", key: "Operations.Manage", name: "Manage Operations", description: "Manage operational tasks", featureId: "feat-13" },

  // Settings (Firm, Facility, Device)
  { id: "perm-17", key: "Firm.View", name: "View Firms", description: "View firm information", featureId: "feat-14" },
  { id: "perm-18", key: "Firm.Create", name: "Create Firms", description: "Create new firms", featureId: "feat-14" },
  { id: "perm-19", key: "Firm.Edit", name: "Edit Firms", description: "Edit firm information", featureId: "feat-14" },
  { id: "perm-20", key: "Firm.Delete", name: "Delete Firms", description: "Delete firms", featureId: "feat-14" },
  { id: "perm-21", key: "Facility.View", name: "View Facilities", description: "View facility information", featureId: "feat-14" },
  { id: "perm-22", key: "Facility.Create", name: "Create Facilities", description: "Create new facilities", featureId: "feat-14" },
  { id: "perm-23", key: "Facility.Edit", name: "Edit Facilities", description: "Edit facility information", featureId: "feat-14" },
  { id: "perm-24", key: "Facility.Delete", name: "Delete Facilities", description: "Delete facilities", featureId: "feat-14" },
  { id: "perm-25", key: "Device.View", name: "View Devices", description: "View device information", featureId: "feat-14" },
  { id: "perm-26", key: "Device.Create", name: "Create Devices", description: "Create new devices", featureId: "feat-14" },

  // User Management
  { id: "perm-27", key: "UserManagement.View", name: "View Users", description: "View user accounts", featureId: "feat-15" },
  { id: "perm-28", key: "UserManagement.Create", name: "Create Users", description: "Create new user accounts", featureId: "feat-15" },
  { id: "perm-29", key: "UserManagement.Edit", name: "Edit Users", description: "Edit user accounts", featureId: "feat-15" },
  { id: "perm-30", key: "UserManagement.Delete", name: "Delete Users", description: "Delete user accounts", featureId: "feat-15" },

  // Audit
  { id: "perm-31", key: "Audit.View", name: "View Audit Logs", description: "View audit trail", featureId: "feat-16" },
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
      "perm-1", "perm-2", "perm-3", "perm-4", "perm-5", "perm-6", "perm-7", "perm-8", "perm-9",
      "perm-11", "perm-12", "perm-13", "perm-14", "perm-15", "perm-16",
      "perm-17", "perm-18", "perm-19", "perm-20", "perm-21", "perm-22", "perm-23", "perm-24",
      "perm-25", "perm-26", "perm-27", "perm-28", "perm-29", "perm-30", "perm-31",
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
      "perm-1", "perm-2", "perm-3", "perm-4", "perm-5", "perm-6", "perm-7", "perm-8",
      "perm-9", "perm-10", "perm-11", "perm-15", "perm-16",
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
      "perm-1", "perm-2", "perm-3", "perm-4", "perm-5", "perm-6", "perm-7", "perm-8",
      "perm-9", "perm-11", "perm-12", "perm-13", "perm-15", "perm-17", "perm-21", "perm-25",
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
      "perm-1", "perm-2", "perm-3", "perm-4", "perm-5", "perm-6", "perm-7", "perm-8",
      "perm-9", "perm-11", "perm-12", "perm-13", "perm-15", "perm-17", "perm-21", "perm-25",
      "perm-27", "perm-31",
    ],
    userIds: ["user-7"],
  },
  {
    id: "role-6",
    tenantId: "tenant-1",
    name: "Field Technician",
    description: "Access to operations, BESS, and solar systems for field operations.",
    isActive: true,
    isSystem: false,
    createdAt: new Date("2024-08-10"),
    updatedAt: new Date("2025-01-05"),
    permissionIds: [
      "perm-1", "perm-2", "perm-3", "perm-5", "perm-6", "perm-7", "perm-9", "perm-10",
      "perm-15", "perm-16", "perm-25",
    ],
    userIds: ["user-2", "user-3", "user-5"],
  },
  {
    id: "role-7",
    tenantId: "tenant-1",
    name: "Financial Analyst",
    description: "Access to financials, analytics, and reporting for financial analysis.",
    isActive: false,
    isSystem: false,
    createdAt: new Date("2024-11-20"),
    updatedAt: new Date("2025-02-14"),
    permissionIds: [
      "perm-1", "perm-2", "perm-11", "perm-12", "perm-13", "perm-14",
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
