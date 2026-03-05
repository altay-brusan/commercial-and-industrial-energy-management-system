export interface UserAccount {
  id: string;
  name: string;
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
  name: string;
  email: string;
  department: string;
  phone: string;
  isActive: boolean;
  roleIds: string[];
}
