export interface UserAccount {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  initials: string;
  avatar?: string;
  department?: string;
  phone?: string;
  isActive: boolean;
  roleIds: string[];
  createdAt: Date;
  updatedAt: Date;
  lastLogin?: Date;
}

export interface UserFormData {
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  phone: string;
  isActive: boolean;
  roleIds: string[];
}

export function getFullName(user: { firstName: string; lastName: string }): string {
  return `${user.firstName} ${user.lastName}`.trim();
}

export function getInitials(firstName: string, lastName: string): string {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}
