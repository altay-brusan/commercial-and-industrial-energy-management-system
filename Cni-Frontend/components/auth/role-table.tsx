"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { RoleStatusBadge } from "./role-status-badge";
import { cn } from "@/lib/utils";
import type { TenantRole } from "@/lib/auth/types";

interface RoleTableProps {
  roles: TenantRole[];
  selectedRoleId?: string;
  onRoleSelect: (role: TenantRole) => void;
}

type SortField = "name" | "description" | "status" | "type" | "permissions" | "users";
type SortDirection = "asc" | "desc";

interface SortableHeaderProps {
  label: string;
  field: SortField;
  currentField: SortField | null;
  direction: SortDirection;
  onSort: (field: SortField) => void;
  className?: string;
}

function SortableHeader({ label, field, currentField, direction, onSort, className }: SortableHeaderProps) {
  const isActive = currentField === field;

  return (
    <TableHead
      className={cn(
        "cursor-pointer select-none hover:bg-muted/80 transition-colors",
        "sticky top-0 z-20 bg-muted shadow-[0_1px_2px_rgba(0,0,0,0.05)]",
        className
      )}
      onClick={() => onSort(field)}
    >
      <div className="flex items-center gap-1.5">
        {label}
        {isActive ? (
          direction === "asc" ? (
            <ArrowUp className="h-3.5 w-3.5 text-foreground" />
          ) : (
            <ArrowDown className="h-3.5 w-3.5 text-foreground" />
          )
        ) : (
          <ArrowUpDown className="h-3.5 w-3.5 text-muted-foreground/50" />
        )}
      </div>
    </TableHead>
  );
}

export function RoleTable({ roles, selectedRoleId, onRoleSelect }: RoleTableProps) {
  const [sortField, setSortField] = useState<SortField | null>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sortedRoles = useMemo(() => {
    if (!sortField) return roles;

    return [...roles].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "name":
          comparison = a.name.localeCompare(b.name);
          break;
        case "description":
          comparison = a.description.localeCompare(b.description);
          break;
        case "status":
          comparison = Number(b.isActive) - Number(a.isActive);
          break;
        case "type":
          comparison = Number(b.isSystem) - Number(a.isSystem);
          break;
        case "permissions":
          comparison = a.permissionIds.length - b.permissionIds.length;
          break;
        case "users":
          comparison = a.userIds.length - b.userIds.length;
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [roles, sortField, sortDirection]);

  return (
    <Table>
      <TableHeader>
        <TableRow className="hover:bg-transparent">
          <SortableHeader
            label="Name"
            field="name"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pl-6 w-[180px]"
          />
          <SortableHeader
            label="Description"
            field="description"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Status"
            field="status"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[100px]"
          />
          <SortableHeader
            label="Type"
            field="type"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[100px]"
          />
          <SortableHeader
            label="Permissions"
            field="permissions"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[110px]"
          />
          <SortableHeader
            label="Users"
            field="users"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pr-6 w-[80px]"
          />
        </TableRow>
      </TableHeader>
      <TableBody>
        {sortedRoles.map((role) => (
          <TableRow
            key={role.id}
            className={cn(
              "cursor-pointer transition-colors",
              selectedRoleId === role.id && "bg-accent"
            )}
            onClick={() => onRoleSelect(role)}
          >
            <TableCell className="py-3 pl-6">
              <span className="text-sm font-medium">{role.name}</span>
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm text-muted-foreground truncate max-w-[300px] block">
                {role.description}
              </span>
            </TableCell>
            <TableCell className="py-3">
              <RoleStatusBadge isActive={role.isActive} />
            </TableCell>
            <TableCell className="py-3">
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
            </TableCell>
            <TableCell className="py-3">
              <span className="text-sm text-muted-foreground">
                {role.permissionIds.length}
              </span>
            </TableCell>
            <TableCell className="py-3 pr-6">
              <span className="text-sm text-muted-foreground">
                {role.userIds.length}
              </span>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
