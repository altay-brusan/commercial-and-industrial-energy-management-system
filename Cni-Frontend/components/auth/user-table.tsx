"use client";

import { useState, useMemo } from "react";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import { format } from "date-fns";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { UserStatusBadge } from "./user-status-badge";
import { cn } from "@/lib/utils";
import { roles } from "@/lib/auth/mock-data";
import type { UserAccount } from "@/lib/auth/user-types";
import { getFullName } from "@/lib/auth/user-types";

interface UserTableProps {
  users: UserAccount[];
  selectedUserId?: string;
  onUserSelect: (user: UserAccount) => void;
}

type SortField = "name" | "email" | "department" | "roles" | "status" | "lastLogin";
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

export function UserTable({ users, selectedUserId, onUserSelect }: UserTableProps) {
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

  const sortedUsers = useMemo(() => {
    if (!sortField) return users;

    return [...users].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "name":
          comparison = getFullName(a).localeCompare(getFullName(b));
          break;
        case "email":
          comparison = a.email.localeCompare(b.email);
          break;
        case "department":
          comparison = (a.department || "").localeCompare(b.department || "");
          break;
        case "roles":
          comparison = a.roleIds.length - b.roleIds.length;
          break;
        case "status":
          comparison = Number(b.isActive) - Number(a.isActive);
          break;
        case "lastLogin":
          comparison =
            (a.lastLogin?.getTime() || 0) - (b.lastLogin?.getTime() || 0);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [users, sortField, sortDirection]);

  const getRoleNames = (roleIds: string[]) => {
    return roleIds
      .map((id) => roles.find((r) => r.id === id))
      .filter(Boolean)
      .map((r) => r!.name);
  };

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
            className="pl-6 w-[200px]"
          />
          <SortableHeader
            label="Email"
            field="email"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[200px]"
          />
          <SortableHeader
            label="Department"
            field="department"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[140px]"
          />
          <SortableHeader
            label="Roles"
            field="roles"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[200px]"
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
            label="Last Login"
            field="lastLogin"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pr-6 w-[140px]"
          />
        </TableRow>
      </TableHeader>
      <TableBody>
        {sortedUsers.map((user) => {
          const roleNames = getRoleNames(user.roleIds);
          const fullName = getFullName(user);

          return (
            <TableRow
              key={user.id}
              className={cn(
                "cursor-pointer transition-colors",
                selectedUserId === user.id && "bg-accent"
              )}
              onClick={() => onUserSelect(user)}
            >
              <TableCell className="py-3 pl-6">
                <div className="flex items-center gap-2.5">
                  <Avatar className="h-7 w-7">
                    <AvatarFallback className="text-[10px]">
                      {user.initials}
                    </AvatarFallback>
                  </Avatar>
                  <span className="text-sm font-medium">{fullName}</span>
                </div>
              </TableCell>
              <TableCell className="py-3">
                <span className="text-sm text-muted-foreground">
                  {user.email}
                </span>
              </TableCell>
              <TableCell className="py-3">
                <span className="text-sm text-muted-foreground">
                  {user.department || "\u2014"}
                </span>
              </TableCell>
              <TableCell className="py-3">
                {roleNames.length === 0 ? (
                  <span className="text-sm text-muted-foreground">\u2014</span>
                ) : roleNames.length <= 2 ? (
                  <div className="flex flex-wrap gap-1">
                    {roleNames.map((name) => (
                      <Badge
                        key={name}
                        variant="secondary"
                        className="text-[10px] font-normal"
                      >
                        {name}
                      </Badge>
                    ))}
                  </div>
                ) : (
                  <div className="flex items-center gap-1">
                    <Badge
                      variant="secondary"
                      className="text-[10px] font-normal"
                    >
                      {roleNames[0]}
                    </Badge>
                    <span className="text-xs text-muted-foreground">
                      +{roleNames.length - 1} more
                    </span>
                  </div>
                )}
              </TableCell>
              <TableCell className="py-3">
                <UserStatusBadge isActive={user.isActive} />
              </TableCell>
              <TableCell className="py-3 pr-6">
                <span className="text-sm text-muted-foreground">
                  {user.lastLogin
                    ? format(user.lastLogin, "MMM d, HH:mm")
                    : "Never"}
                </span>
              </TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
}
