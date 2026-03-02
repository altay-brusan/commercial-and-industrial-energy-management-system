export type FeatureCategory = "Core" | "Energy" | "Intelligence" | "Management";

export interface Feature {
  id: string;
  key: string;
  name: string;
  description: string;
  category: FeatureCategory;
  isActive: boolean;
}

export interface Permission {
  id: string;
  key: string; // Feature.Action format e.g. "Dashboard.View"
  name: string;
  description: string;
  featureId: string;
}

export interface PermissionGroup {
  feature: Feature;
  permissions: Permission[];
}

export interface TenantRole {
  id: string;
  tenantId: string;
  name: string;
  description: string;
  isActive: boolean;
  isSystem: boolean;
  createdAt: Date;
  updatedAt: Date;
  permissionIds: string[];
  userIds: string[];
}

export interface RoleFilters {
  status: ("active" | "inactive")[];
  search: string;
}

export interface RoleFormData {
  name: string;
  description: string;
  isActive: boolean;
}
