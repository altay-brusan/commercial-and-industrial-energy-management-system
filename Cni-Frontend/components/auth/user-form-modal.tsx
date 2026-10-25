"use client";

import { useState, useEffect } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Checkbox } from "@/components/ui/checkbox";
import { roles } from "@/lib/auth/mock-data";
import type { UserAccount, UserFormData } from "@/lib/auth/user-types";

interface UserFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: UserFormData) => void;
  editUser?: UserAccount | null;
}

export function UserFormModal({
  open,
  onOpenChange,
  onSubmit,
  editUser,
}: UserFormModalProps) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [department, setDepartment] = useState("");
  const [phone, setPhone] = useState("");
  const [isActive, setIsActive] = useState(true);
  const [roleIds, setRoleIds] = useState<string[]>([]);

  useEffect(() => {
    if (open) {
      if (editUser) {
        setFirstName(editUser.firstName);
        setLastName(editUser.lastName);
        setEmail(editUser.email);
        setDepartment(editUser.department || "");
        setPhone(editUser.phone || "");
        setIsActive(editUser.isActive);
        setRoleIds([...editUser.roleIds]);
      } else {
        setFirstName("");
        setLastName("");
        setEmail("");
        setDepartment("");
        setPhone("");
        setIsActive(true);
        setRoleIds([]);
      }
    }
  }, [open, editUser]);

  const handleSubmit = () => {
    if (!firstName.trim() || !lastName.trim() || !email.trim()) return;

    onSubmit({
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      email: email.trim(),
      department: department.trim(),
      phone: phone.trim(),
      isActive,
      roleIds,
    });

    setFirstName("");
    setLastName("");
    setEmail("");
    setDepartment("");
    setPhone("");
    setIsActive(true);
    setRoleIds([]);
    onOpenChange(false);
  };

  const handleRoleToggle = (roleId: string) => {
    setRoleIds((prev) =>
      prev.includes(roleId)
        ? prev.filter((id) => id !== roleId)
        : [...prev, roleId]
    );
  };

  const isFormValid =
    firstName.trim().length > 0 &&
    lastName.trim().length > 0 &&
    email.trim().length > 0;

  const activeRoles = roles.filter((r) => r.isActive);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {editUser ? "Edit User" : "New User"}
          </DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-2 gap-3">
            <div className="grid gap-2">
              <Label htmlFor="user-first-name">
                First Name <span className="text-destructive">*</span>
              </Label>
              <Input
                id="user-first-name"
                placeholder="First name"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="user-last-name">
                Last Name <span className="text-destructive">*</span>
              </Label>
              <Input
                id="user-last-name"
                placeholder="Last name"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
              />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="user-email">
              Email <span className="text-destructive">*</span>
            </Label>
            <Input
              id="user-email"
              type="email"
              placeholder="Enter email address"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="grid gap-2">
              <Label htmlFor="user-department">Department</Label>
              <Input
                id="user-department"
                placeholder="Department"
                value={department}
                onChange={(e) => setDepartment(e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="user-phone">Phone</Label>
              <Input
                id="user-phone"
                placeholder="Phone number"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
              />
            </div>
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label htmlFor="user-active">Active</Label>
              <p className="text-xs text-muted-foreground">
                Inactive users cannot log in to the system
              </p>
            </div>
            <Switch
              id="user-active"
              checked={isActive}
              onCheckedChange={setIsActive}
            />
          </div>

          {/* Role Assignment */}
          <div className="grid gap-2">
            <Label>Roles</Label>
            <div className="border rounded-md p-3 space-y-2 max-h-[180px] overflow-y-auto">
              {activeRoles.length === 0 ? (
                <p className="text-xs text-muted-foreground">No active roles available</p>
              ) : (
                activeRoles.map((role) => (
                  <div
                    key={role.id}
                    className="flex items-start gap-2"
                  >
                    <Checkbox
                      id={`role-${role.id}`}
                      checked={roleIds.includes(role.id)}
                      onCheckedChange={() => handleRoleToggle(role.id)}
                      className="mt-0.5"
                    />
                    <label
                      htmlFor={`role-${role.id}`}
                      className="text-sm cursor-pointer leading-tight"
                    >
                      <span className="font-medium">{role.name}</span>
                      <p className="text-xs text-muted-foreground mt-0.5">
                        {role.description}
                      </p>
                    </label>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!isFormValid}>
            {editUser ? "Save Changes" : "Create User"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
