"use client";

import { format } from "date-fns";
import {
  X,
  Pencil,
  Shield,
  Trash2,
  Lock,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { RoleStatusBadge } from "./role-status-badge";
import { cn } from "@/lib/utils";
import { features, permissions } from "@/lib/auth/mock-data";
import type { TenantRole, FeatureCategory } from "@/lib/auth/types";
import type { User } from "@/lib/maintenance/types";

interface RoleDetailPanelProps {
  role: TenantRole;
  roleUsers: User[];
  onClose: () => void;
  onEdit: () => void;
  onManagePermissions: () => void;
  onDelete: () => void;
}

const categoryOrder: FeatureCategory[] = ["Core", "Energy", "Intelligence", "Management"];

export function RoleDetailPanel({
  role,
  roleUsers,
  onClose,
  onEdit,
  onManagePermissions,
  onDelete,
}: RoleDetailPanelProps) {
  const rolePermissions = permissions.filter((p) =>
    role.permissionIds.includes(p.id)
  );

  // Group permissions by category
  const groupedPermissions = categoryOrder
    .map((category) => {
      const categoryFeatures = features.filter((f) => f.category === category);
      const categoryPerms = rolePermissions.filter((p) =>
        categoryFeatures.some((f) => f.id === p.featureId)
      );
      return { category, permissions: categoryPerms };
    })
    .filter((g) => g.permissions.length > 0);

  return (
    <aside className="w-[420px] shrink-0 border-l border-border bg-background flex flex-col overflow-hidden">
      {/* Header */}
      <div className="shrink-0 flex items-center justify-between px-4 py-3 border-b border-border">
        <h2 className="text-sm font-semibold truncate">{role.name}</h2>
        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-y-auto">
        {/* Info Section */}
        <div className="px-4 py-4 space-y-3">
          <div className="flex items-center gap-2">
            <RoleStatusBadge isActive={role.isActive} />
            <Badge
              variant="outline"
              className={cn(
                "text-xs",
                role.isSystem
                  ? "border-primary/30 text-primary bg-primary/5"
                  : "border-muted-foreground/30 text-muted-foreground"
              )}
            >
              {role.isSystem ? "System" : "Custom"}
            </Badge>
          </div>

          <p className="text-sm text-muted-foreground">{role.description}</p>

          <div className="grid grid-cols-2 gap-2 text-xs">
            <div>
              <span className="text-muted-foreground">Created</span>
              <p className="font-medium">{format(role.createdAt, "MMM d, yyyy")}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Updated</span>
              <p className="font-medium">{format(role.updatedAt, "MMM d, yyyy")}</p>
            </div>
          </div>
        </div>

        <Separator />

        {/* Permissions Section */}
        <div className="px-4 py-4 space-y-3">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium">
              Permissions ({rolePermissions.length})
            </span>
          </div>

          {groupedPermissions.length === 0 ? (
            <p className="text-xs text-muted-foreground">No permissions assigned</p>
          ) : (
            <div className="space-y-2">
              {groupedPermissions.map(({ category, permissions: catPerms }) => (
                <div key={category}>
                  <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-wider mb-1">
                    {category}
                  </p>
                  <div className="flex flex-wrap gap-1">
                    {catPerms.map((perm) => (
                      <Badge
                        key={perm.id}
                        variant="secondary"
                        className="text-[10px] font-normal"
                      >
                        {perm.name}
                      </Badge>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <Separator />

        {/* Users Section */}
        <div className="px-4 py-4 space-y-3">
          <span className="text-sm font-medium">
            Users ({roleUsers.length})
          </span>

          {roleUsers.length === 0 ? (
            <p className="text-xs text-muted-foreground">No users assigned</p>
          ) : (
            <div className="space-y-1.5">
              {roleUsers.map((user) => (
                <div key={user.id} className="flex items-center gap-2 py-1">
                  <Avatar className="h-6 w-6">
                    <AvatarFallback className="text-[10px]">
                      {user.initials}
                    </AvatarFallback>
                  </Avatar>
                  <div className="min-w-0">
                    <p className="text-xs font-medium truncate">{user.name}</p>
                    <p className="text-[10px] text-muted-foreground truncate">
                      {user.email}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <Separator />

        {/* Actions */}
        <div className="px-4 py-4 space-y-2">
          <Button
            variant="outline"
            size="sm"
            className="w-full justify-start gap-2"
            onClick={onEdit}
          >
            <Pencil className="h-3.5 w-3.5" />
            Edit Role
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="w-full justify-start gap-2"
            onClick={onManagePermissions}
            disabled={role.name === "Administrator"}
          >
            <Shield className="h-3.5 w-3.5" />
            Manage Permissions
            {role.name === "Administrator" && (
              <Lock className="h-3 w-3 ml-auto text-muted-foreground" />
            )}
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="w-full justify-start gap-2 text-destructive hover:text-destructive"
            onClick={onDelete}
            disabled={role.isSystem}
          >
            <Trash2 className="h-3.5 w-3.5" />
            Delete Role
            {role.isSystem && (
              <Lock className="h-3 w-3 ml-auto text-muted-foreground" />
            )}
          </Button>
        </div>
      </div>
    </aside>
  );
}
