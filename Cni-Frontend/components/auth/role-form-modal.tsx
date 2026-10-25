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
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import type { RoleFormData, TenantRole } from "@/lib/auth/types";

interface RoleFormModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: RoleFormData) => void;
  editRole?: TenantRole | null;
}

export function RoleFormModal({
  open,
  onOpenChange,
  onSubmit,
  editRole,
}: RoleFormModalProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    if (open) {
      if (editRole) {
        setName(editRole.name);
        setDescription(editRole.description);
        setIsActive(editRole.isActive);
      } else {
        setName("");
        setDescription("");
        setIsActive(true);
      }
    }
  }, [open, editRole]);

  const handleSubmit = () => {
    if (!name.trim()) return;

    onSubmit({ name: name.trim(), description: description.trim(), isActive });

    setName("");
    setDescription("");
    setIsActive(true);
    onOpenChange(false);
  };

  const isFormValid = name.trim().length > 0;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>
            {editRole ? "Edit Role" : "New Role"}
          </DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="role-name">
              Name <span className="text-destructive">*</span>
            </Label>
            <Input
              id="role-name"
              placeholder="Enter role name"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="role-description">Description</Label>
            <Textarea
              id="role-description"
              placeholder="Enter role description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="min-h-[80px]"
            />
          </div>

          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label htmlFor="role-active">Active</Label>
              <p className="text-xs text-muted-foreground">
                Inactive roles cannot be assigned to users
              </p>
            </div>
            <Switch
              id="role-active"
              checked={isActive}
              onCheckedChange={setIsActive}
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!isFormValid}>
            {editRole ? "Save Changes" : "Create Role"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
