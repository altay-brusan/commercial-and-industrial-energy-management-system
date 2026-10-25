"use client";

import { useState, useMemo, useEffect, useCallback } from "react";
import { Plus, Search, Users, UserCheck, UserX, Loader2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { AccessTabs } from "@/components/auth/access-tabs";
import { UserTable } from "@/components/auth/user-table";
import { UserDetailPanel } from "@/components/auth/user-detail-panel";
import { UserFormModal } from "@/components/auth/user-form-modal";
import { UserDeleteDialog } from "@/components/auth/user-delete-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { userAccounts as mockUsers } from "@/lib/auth/user-mock-data";
import * as userService from "@/lib/auth/user-service";
import type { UserAccount, UserFormData } from "@/lib/auth/user-types";
import { getFullName, getInitials } from "@/lib/auth/user-types";

export default function UsersPage() {
  const [userList, setUserList] = useState<UserAccount[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedUser, setSelectedUser] = useState<UserAccount | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal states
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const [editModalOpen, setEditModalOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  // Fetch users from API with mock fallback
  const fetchUsers = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await userService.getUsers();
      setUserList(response.items);
    } catch {
      // Fallback to mock data when API is unavailable
      setUserList(mockUsers);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  // Filtered users
  const filteredUsers = useMemo(() => {
    if (!searchQuery.trim()) return userList;
    const q = searchQuery.toLowerCase();
    return userList.filter(
      (u) =>
        getFullName(u).toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.department || "").toLowerCase().includes(q)
    );
  }, [userList, searchQuery]);

  // Stats
  const stats = useMemo(
    () => ({
      total: userList.length,
      active: userList.filter((u) => u.isActive).length,
      inactive: userList.filter((u) => !u.isActive).length,
    }),
    [userList]
  );

  // Handlers
  const handleCreateUser = async (data: UserFormData) => {
    try {
      const created = await userService.createUser(data);
      setUserList((prev) => [...prev, created]);
    } catch {
      // Fallback: create locally
      const initials = getInitials(data.firstName, data.lastName);
      const newUser: UserAccount = {
        id: `user-${Date.now()}`,
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        initials,
        department: data.department || undefined,
        phone: data.phone || undefined,
        isActive: data.isActive,
        roleIds: data.roleIds,
        createdAt: new Date(),
        updatedAt: new Date(),
      };
      setUserList((prev) => [...prev, newUser]);
    }
  };

  const handleEditUser = async (data: UserFormData) => {
    if (!selectedUser) return;

    try {
      const updated = await userService.updateUser(selectedUser.id, data);
      setUserList((prev) => prev.map((u) => (u.id === selectedUser.id ? updated : u)));
      setSelectedUser(updated);
    } catch {
      // Fallback: update locally
      const initials = getInitials(data.firstName, data.lastName);
      const updatedUser = {
        ...selectedUser,
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        initials,
        department: data.department || undefined,
        phone: data.phone || undefined,
        isActive: data.isActive,
        roleIds: data.roleIds,
        updatedAt: new Date(),
      };
      setUserList((prev) =>
        prev.map((u) => (u.id === selectedUser.id ? updatedUser : u))
      );
      setSelectedUser(updatedUser);
    }
  };

  const handleDeleteUser = async () => {
    if (!selectedUser) return;

    try {
      await userService.deleteUser(selectedUser.id);
    } catch {
      // Continue with local removal even if API fails
    }

    setUserList((prev) => prev.filter((u) => u.id !== selectedUser.id));
    setSelectedUser(null);
    setDeleteDialogOpen(false);
  };

  const handleUserSelect = (user: UserAccount) => {
    setSelectedUser(selectedUser?.id === user.id ? null : user);
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
                <h1 className="text-lg font-semibold">Users</h1>
                <p className="text-sm text-muted-foreground">
                  Manage user accounts and role assignments
                </p>
              </div>
              <div className="flex items-center gap-2">
                <div className="relative">
                  <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
                  <Input
                    placeholder="Search users..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="h-8 w-[200px] pl-8 text-sm"
                  />
                </div>
                <Button size="sm" onClick={() => setCreateModalOpen(true)}>
                  <Plus className="h-4 w-4 mr-1.5" />
                  New User
                </Button>
              </div>
            </div>

            {/* Stats bar */}
            <div className="flex items-center gap-4 text-xs text-muted-foreground mb-3">
              <div className="flex items-center gap-1.5">
                <Users className="h-3.5 w-3.5" />
                <span>{stats.total} total</span>
              </div>
              <div className="flex items-center gap-1.5">
                <UserCheck className="h-3.5 w-3.5" />
                <span>{stats.active} active</span>
              </div>
              <div className="flex items-center gap-1.5">
                <UserX className="h-3.5 w-3.5" />
                <span>{stats.inactive} inactive</span>
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
            ) : error ? (
              <div className="flex items-center justify-center h-full">
                <p className="text-sm text-destructive">{error}</p>
              </div>
            ) : (
              <UserTable
                users={filteredUsers}
                selectedUserId={selectedUser?.id}
                onUserSelect={handleUserSelect}
              />
            )}
          </div>

          {/* Footer */}
          <div className="shrink-0 px-6 py-2 border-t border-border text-xs text-muted-foreground">
            Showing {filteredUsers.length} of {userList.length} users
          </div>
        </div>

        {/* Detail Panel */}
        {selectedUser && (
          <UserDetailPanel
            user={selectedUser}
            onClose={() => setSelectedUser(null)}
            onEdit={() => setEditModalOpen(true)}
            onDelete={() => setDeleteDialogOpen(true)}
          />
        )}
      </div>

      {/* Create Modal */}
      <UserFormModal
        open={createModalOpen}
        onOpenChange={setCreateModalOpen}
        onSubmit={handleCreateUser}
      />

      {/* Edit Modal */}
      <UserFormModal
        open={editModalOpen}
        onOpenChange={setEditModalOpen}
        onSubmit={handleEditUser}
        editUser={selectedUser}
      />

      {/* Delete Dialog */}
      <UserDeleteDialog
        open={deleteDialogOpen}
        onOpenChange={setDeleteDialogOpen}
        user={selectedUser}
        onConfirm={handleDeleteUser}
      />
    </div>
  );
}
