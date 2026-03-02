import type { UserAccount } from "./user-types";
import { roles } from "./mock-data";

function getRoleIdsForUser(userId: string): string[] {
  return roles.filter((r) => r.userIds.includes(userId)).map((r) => r.id);
}

export const userAccounts: UserAccount[] = [
  {
    id: "user-1",
    name: "John Doe",
    email: "john@energy.com",
    initials: "JD",
    department: "Operations",
    phone: "+1 (555) 123-4567",
    isActive: true,
    roleIds: getRoleIdsForUser("user-1"),
    createdAt: new Date("2024-01-15"),
    updatedAt: new Date("2025-12-10"),
    lastLogin: new Date("2026-03-01T09:15:00"),
  },
  {
    id: "user-2",
    name: "Jane Smith",
    email: "jane@energy.com",
    initials: "JS",
    department: "Field Services",
    phone: "+1 (555) 234-5678",
    isActive: true,
    roleIds: getRoleIdsForUser("user-2"),
    createdAt: new Date("2024-02-20"),
    updatedAt: new Date("2025-11-05"),
    lastLogin: new Date("2026-03-01T08:42:00"),
  },
  {
    id: "user-3",
    name: "Mike Johnson",
    email: "mike@energy.com",
    initials: "MJ",
    department: "Battery Systems",
    phone: "+1 (555) 345-6789",
    isActive: true,
    roleIds: getRoleIdsForUser("user-3"),
    createdAt: new Date("2024-03-10"),
    updatedAt: new Date("2025-10-18"),
    lastLogin: new Date("2026-02-28T16:30:00"),
  },
  {
    id: "user-4",
    name: "Sarah Wilson",
    email: "sarah@energy.com",
    initials: "SW",
    department: "Engineering",
    phone: "+1 (555) 456-7890",
    isActive: true,
    roleIds: getRoleIdsForUser("user-4"),
    createdAt: new Date("2024-04-05"),
    updatedAt: new Date("2025-09-22"),
    lastLogin: new Date("2026-02-27T14:10:00"),
  },
  {
    id: "user-5",
    name: "Tom Brown",
    email: "tom@energy.com",
    initials: "TB",
    department: "Grid Operations",
    phone: "+1 (555) 567-8901",
    isActive: true,
    roleIds: getRoleIdsForUser("user-5"),
    createdAt: new Date("2024-05-12"),
    updatedAt: new Date("2025-08-30"),
    lastLogin: new Date("2026-03-01T07:55:00"),
  },
  {
    id: "user-6",
    name: "Emily Chen",
    email: "emily@energy.com",
    initials: "EC",
    department: "Analytics",
    phone: "+1 (555) 678-9012",
    isActive: true,
    roleIds: getRoleIdsForUser("user-6"),
    createdAt: new Date("2024-07-18"),
    updatedAt: new Date("2025-12-01"),
    lastLogin: new Date("2026-02-26T11:20:00"),
  },
  {
    id: "user-7",
    name: "David Park",
    email: "david@energy.com",
    initials: "DP",
    department: "Compliance",
    phone: "+1 (555) 789-0123",
    isActive: false,
    roleIds: getRoleIdsForUser("user-7"),
    createdAt: new Date("2024-09-01"),
    updatedAt: new Date("2026-01-15"),
    lastLogin: new Date("2026-01-10T09:00:00"),
  },
  {
    id: "user-8",
    name: "Lisa Martinez",
    email: "lisa@energy.com",
    initials: "LM",
    department: "Field Services",
    phone: "+1 (555) 890-1234",
    isActive: true,
    roleIds: [],
    createdAt: new Date("2025-01-10"),
    updatedAt: new Date("2025-01-10"),
    lastLogin: undefined,
  },
];

export function getAllUserAccounts(): UserAccount[] {
  return userAccounts;
}

export function getUserAccountById(id: string): UserAccount | undefined {
  return userAccounts.find((u) => u.id === id);
}

export function getRolesForUser(userId: string) {
  const user = userAccounts.find((u) => u.id === userId);
  if (!user) return [];
  return roles.filter((r) => user.roleIds.includes(r.id));
}

export function getUserAccountCount() {
  return {
    total: userAccounts.length,
    active: userAccounts.filter((u) => u.isActive).length,
    inactive: userAccounts.filter((u) => !u.isActive).length,
  };
}
