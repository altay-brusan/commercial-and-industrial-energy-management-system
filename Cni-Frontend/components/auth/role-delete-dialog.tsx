"use client";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { TenantRole } from "@/lib/auth/types";

interface RoleDeleteDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  role: TenantRole | null;
  onConfirm: () => void;
}

export function RoleDeleteDialog({
  open,
  onOpenChange,
  role,
  onConfirm,
}: RoleDeleteDialogProps) {
  if (!role) return null;

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Role</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete the role &ldquo;{role.name}&rdquo;?
            This will remove all permission assignments and unassign{" "}
            {role.userIds.length} user(s) from this role. This action cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={onConfirm}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            Delete Role
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
