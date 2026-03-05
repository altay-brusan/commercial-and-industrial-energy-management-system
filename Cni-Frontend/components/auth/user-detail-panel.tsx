"use client";

import { format } from "date-fns";
import { X, Pencil, Trash2, Mail, Phone, Building2, Calendar } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { UserStatusBadge } from "./user-status-badge";
import { roles } from "@/lib/auth/mock-data";
import type { UserAccount } from "@/lib/auth/user-types";

interface UserDetailPanelProps {
  user: UserAccount;
  onClose: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

export function UserDetailPanel({
  user,
  onClose,
  onEdit,
  onDelete,
}: UserDetailPanelProps) {
  const userRoles = roles.filter((r) => user.roleIds.includes(r.id));

  return (
    <aside className="w-[420px] shrink-0 border-l border-border bg-background flex flex-col overflow-hidden">
      {/* Header */}
      <div className="shrink-0 flex items-center justify-between px-4 py-3 border-b border-border">
        <h2 className="text-sm font-semibold truncate">{user.name}</h2>
        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={onClose}>
          <X className="h-4 w-4" />
        </Button>
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-y-auto">
        {/* Info Section */}
        <div className="px-4 py-4 space-y-4">
          <div className="flex items-center gap-3">
            <Avatar className="h-12 w-12">
              <AvatarFallback className="text-base">{user.initials}</AvatarFallback>
            </Avatar>
            <div className="min-w-0">
              <p className="text-sm font-medium">{user.name}</p>
              <UserStatusBadge isActive={user.isActive} />
            </div>
          </div>

          <div className="space-y-2">
            <div className="flex items-center gap-2 text-sm">
              <Mail className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
              <span className="text-muted-foreground truncate">{user.email}</span>
            </div>
            {user.department && (
              <div className="flex items-center gap-2 text-sm">
                <Building2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
                <span className="text-muted-foreground">{user.department}</span>
              </div>
            )}
            {user.phone && (
              <div className="flex items-center gap-2 text-sm">
                <Phone className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
                <span className="text-muted-foreground">{user.phone}</span>
              </div>
            )}
          </div>

          <div className="grid grid-cols-2 gap-2 text-xs">
            <div>
              <span className="text-muted-foreground">Created</span>
              <p className="font-medium">{format(user.createdAt, "MMM d, yyyy")}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Updated</span>
              <p className="font-medium">{format(user.updatedAt, "MMM d, yyyy")}</p>
            </div>
            <div className="col-span-2">
              <div className="flex items-center gap-1 text-muted-foreground">
                <Calendar className="h-3 w-3" />
                <span>Last Login</span>
              </div>
              <p className="font-medium">
                {user.lastLogin
                  ? format(user.lastLogin, "MMM d, yyyy 'at' HH:mm")
                  : "Never"}
              </p>
            </div>
          </div>
        </div>

        <Separator />

        {/* Roles Section */}
        <div className="px-4 py-4 space-y-3">
          <span className="text-sm font-medium">
            Roles ({userRoles.length})
          </span>

          {userRoles.length === 0 ? (
            <p className="text-xs text-muted-foreground">No roles assigned</p>
          ) : (
            <div className="space-y-1.5">
              {userRoles.map((role) => (
                <div
                  key={role.id}
                  className="flex items-center justify-between py-1"
                >
                  <div className="min-w-0">
                    <p className="text-xs font-medium">{role.name}</p>
                    <p className="text-[10px] text-muted-foreground truncate">
                      {role.description}
                    </p>
                  </div>
                  <Badge
                    variant="outline"
                    className="text-[10px] shrink-0 ml-2"
                  >
                    {role.permissionIds.length} perms
                  </Badge>
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
            Edit User
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="w-full justify-start gap-2 text-destructive hover:text-destructive"
            onClick={onDelete}
          >
            <Trash2 className="h-3.5 w-3.5" />
            Delete User
          </Button>
        </div>
      </div>
    </aside>
  );
}
