"use client";

import { useState, useMemo, useEffect, useCallback } from "react";
import { Plus, Search, Shield, Lock, Unlock, Loader2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { AccessTabs } from "@/components/auth/access-tabs";
import { RoleTable } from "@/components/auth/role-table";
import { RoleDetailPanel } from "@/components/auth/role-detail-panel";
import { RoleFormModal } from "@/components/auth/role-form-modal";
import { RoleDeleteDialog } from "@/components/auth/role-delete-dialog";
import { PermissionMatrix } from "@/components/auth/permission-matrix";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { roles as mockRoles, getUsersForRole } from "@/lib/auth/mock-data";
import { users } from "@/lib/maintenance/mock-data";
import * as roleService from "@/lib/auth/role-service";
import type { TenantRole, RoleFormData } from "@/lib/auth/types";

export default function RolesPage() {
  const [roleList, setRoleList] = useState<TenantRole[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedRole, setSelectedRole] = useState<TenantRole | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Modal states
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [permissionsDialogOpen, setPermissionsDialogOpen] = useState(false);

  // Permission editing state
  const [editingPermissionIds, setEditingPermissionIds] = useState<string[]>([]);

  // Fetch roles from API with mock fallback
  const fetchRoles = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await roleService.getRoles();
      setRoleList(data);
    } catch {
      setRoleList(mockRoles);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRoles();
  }, [fetchRoles]);

  // Filtered roles
  const filteredRoles = useMemo(() => {
    if (!searchQuery.trim()) return roleList;
    const q = searchQuery.toLowerCase();
    return roleList.filter(
      (r) =>
        r.name.toLowerCase().includes(q) ||
        r.description.toLowerCase().includes(q)
    );
  }, [roleList, searchQuery]);

  // Stats
  const stats = useMemo(() => ({
    total: roleList.length,
    active: roleList.filter((r) => r.isActive).length,
    system: roleList.filter((r) => r.isSystem).length,
    custom: roleList.filter((r) => !r.isSystem).length,
  }), [roleList]);

  // Role users for detail panel
  const selectedRoleUsers = useMemo(() => {
    if (!selectedRole) return [];
    return users.filter((u) => selectedRole.userIds.includes(u.id));
  }, [selectedRole]);

  // Handlers
  const handleCreateRole = async (data: RoleFormData) => {
    try {
      const created = await roleService.createRole(data);
      setRoleList((prev) => [...prev, created]);
    } catch {
      const newRole: TenantRole = {
        id: `role-${Date.now()}`,
        tenantId: "tenant-1",
        name: data.name,
        description: data.description,
        isActive: data.isActive,
        isSystem: false,
        createdAt: new Date(),
        updatedAt: new Date(),
        permissionIds: [],
        userIds: [],
      };
      setRoleList((prev) => [...prev, newRole]);
    }
  };

  const handleEditRole = async (data: RoleFormData) => {
    if (!selectedRole) return;

    try {
      const updated = await roleService.updateRole(selectedRole.id, data);
      setRoleList((prev) => prev.map((r) => (r.id === selectedRole.id ? updated : r)));
      setSelectedRole(updated);
    } catch {
      setRoleList((prev) =>
        prev.map((r) =>
          r.id === selectedRole.id
            ? { ...r, name: data.name, description: data.description, isActive: data.isActive, updatedAt: new Date() }
            : r
        )
      );
      setSelectedRole((prev) =>
        prev ? { ...prev, name: data.name, description: data.description, isActive: data.isActive, updatedAt: new Date() } : null
      );
    }
  };

  const handleDeleteRole = async () => {
    if (!selectedRole) return;

    try {
      await roleService.deleteRole(selectedRole.id);
    } catch {
      // Continue with local removal
    }

    setRoleList((prev) => prev.filter((r) => r.id !== selectedRole.id));
    setSelectedRole(null);
    setDeleteDialogOpen(false);
  };

  const handleOpenPermissions = () => {
    if (!selectedRole) return;
    setEditingPermissionIds([...selectedRole.permissionIds]);
    setPermissionsDialogOpen(true);
  };

  const handleSavePermissions = async () => {
    if (!selectedRole) return;

    try {
      await roleService.updateRolePermissions(selectedRole.id, editingPermissionIds);
    } catch {
      // Continue with local update
    }

    setRoleList((prev) =>
      prev.map((r) =>
        r.id === selectedRole.id
          ? { ...r, permissionIds: editingPermissionIds, updatedAt: new Date() }
          : r
      )
    );
    setSelectedRole((prev) =>
      prev ? { ...prev, permissionIds: editingPermissionIds, updatedAt: new Date() } : null
    );
    setPermissionsDialogOpen(false);
  };

  const handleAddUser = async (userId: string) => {
    if (!selectedRole) return;

    try {
      await roleService.addUserToRole(selectedRole.id, userId);
    } catch {
      // Continue with local update
    }

    const newUserIds = [...selectedRole.userIds, userId];
    setRoleList((prev) =>
      prev.map((r) =>
        r.id === selectedRole.id ? { ...r, userIds: newUserIds, updatedAt: new Date() } : r
      )
    );
    setSelectedRole((prev) =>
      prev ? { ...prev, userIds: newUserIds, updatedAt: new Date() } : null
    );
  };

  const handleRemoveUser = async (userId: string) => {
    if (!selectedRole) return;

    try {
      await roleService.removeUserFromRole(selectedRole.id, userId);
    } catch {
      // Continue with local update
    }

    const newUserIds = selectedRole.userIds.filter((id) => id !== userId);
    setRoleList((prev) =>
      prev.map((r) =>
        r.id === selectedRole.id ? { ...r, userIds: newUserIds, updatedAt: new Date() } : r
      )
    );
    setSelectedRole((prev) =>
      prev ? { ...prev, userIds: newUserIds, updatedAt: new Date() } : null
    );
  };

  const handleRoleSelect = (role: TenantRole) => {
    setSelectedRole(selectedRole?.id === role.id ? null : role);
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="configuration" />
      <div className="flex flex-1 overflow-hidden">
        <ConfigurationSidebar />

        {/* Main content */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Header */}
          <div className="shrink-0 px-6 py-3 border-b border-border">
            <div className="flex items-center justify-between mb-3">
              <div>
                <h1 className="text-lg font-semibold">Roles</h1>
                <p className="text-sm text-muted-foreground">
                  Manage roles and permission assignments
                </p>
              </div>
              <div className="flex items-center gap-2">
                <div className="relative">
                  <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
                  <Input
                    placeholder="Search roles..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="h-8 w-[200px] pl-8 text-sm"
                  />
                </div>
                <Button size="sm" onClick={() => setCreateModalOpen(true)}>
                  <Plus className="h-4 w-4 mr-1.5" />
                  New Role
                </Button>
              </div>
            </div>

            {/* Stats bar */}
            <div className="flex items-center gap-4 text-xs text-muted-foreground mb-3">
              <div className="flex items-center gap-1.5">
                <Shield className="h-3.5 w-3.5" />
                <span>{stats.total} total</span>
              </div>
              <div className="flex items-center gap-1.5">
                <span className="h-2 w-2 rounded-full bg-status-operational" />
                <span>{stats.active} active</span>
              </div>
              <div className="flex items-center gap-1.5">
                <Lock className="h-3 w-3" />
                <span>{stats.system} system</span>
              </div>
              <div className="flex items-center gap-1.5">
                <Unlock className="h-3 w-3" />
                <span>{stats.custom} custom</span>
              </div>
            </div>

            {/* Access Tabs */}
            <AccessTabs />
          </div>

          {/* Table */}
          <div className="flex-1 overflow-auto">
            {isLoading ? (
              <div className="flex items-center justify-center h-full">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : (
              <RoleTable
                roles={filteredRoles}
                selectedRoleId={selectedRole?.id}
                onRoleSelect={handleRoleSelect}
              />
            )}
          </div>

          {/* Footer */}
          <div className="shrink-0 px-6 py-2 border-t border-border text-xs text-muted-foreground">
            Showing {filteredRoles.length} of {roleList.length} roles
          </div>
        </div>

        {/* Detail Panel */}
        {selectedRole && (
          <RoleDetailPanel
            role={selectedRole}
            roleUsers={selectedRoleUsers}
            onClose={() => setSelectedRole(null)}
            onEdit={() => setEditModalOpen(true)}
            onManagePermissions={handleOpenPermissions}
            onDelete={() => setDeleteDialogOpen(true)}
          />
        )}
      </div>

      {/* Create Modal */}
      <RoleFormModal
        open={createModalOpen}
        onOpenChange={setCreateModalOpen}
        onSubmit={handleCreateRole}
      />

      {/* Edit Modal */}
      <RoleFormModal
        open={editModalOpen}
        onOpenChange={setEditModalOpen}
        onSubmit={handleEditRole}
        editRole={selectedRole}
      />

      {/* Delete Dialog */}
      <RoleDeleteDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        role={selectedRole}
        onConfirm={handleDeleteRole}
      />

      {/* Permissions Dialog */}
      <Dialog open={permissionsDialogOpen} onOpenChange={setPermissionsDialogOpen}>
        <DialogContent className="sm:max-w-[800px] max-h-[80vh] flex flex-col">
          <DialogHeader>
            <DialogTitle>
              Manage Permissions — {selectedRole?.name}
            </DialogTitle>
          </DialogHeader>
          <div className="flex-1 overflow-y-auto py-4">
            <PermissionMatrix
              selectedPermissionIds={editingPermissionIds}
              onPermissionChange={setEditingPermissionIds}
              disabled={selectedRole?.name === "Administrator"}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPermissionsDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleSavePermissions}
              disabled={selectedRole?.name === "Administrator"}
            >
              Save Permissions
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
