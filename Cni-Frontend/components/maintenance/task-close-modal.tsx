"use client";

import { useState } from "react";
import { XCircle } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { CloseReason, TaskCloseDetails } from "@/lib/maintenance/types";

interface TaskCloseModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskTitle: string;
  onClose: (details: Omit<TaskCloseDetails, "closedAt" | "closedBy">) => void;
}

const closeReasons: { value: CloseReason; label: string; description: string }[] = [
  { value: "cancelled", label: "Cancelled", description: "Task is no longer needed" },
  { value: "duplicate", label: "Duplicate", description: "Another task covers this work" },
  { value: "not_needed", label: "Not Needed", description: "Issue resolved itself or requirements changed" },
  { value: "transferred", label: "Transferred", description: "Moved to another system or team" },
  { value: "other", label: "Other", description: "Other reason" },
];

export function TaskCloseModal({
  open,
  onOpenChange,
  taskTitle,
  onClose,
}: TaskCloseModalProps) {
  const [reason, setReason] = useState<CloseReason | "">("");
  const [explanation, setExplanation] = useState("");

  const handleSubmit = () => {
    if (!reason || !explanation.trim()) return;

    onClose({
      reason,
      explanation: explanation.trim(),
    });

    // Reset form
    setReason("");
    setExplanation("");
    onOpenChange(false);
  };

  const isFormValid = reason && explanation.trim();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <XCircle className="h-5 w-5 text-muted-foreground" />
            Close Task
          </DialogTitle>
          <DialogDescription>
            Closing: <span className="font-medium text-foreground">{taskTitle}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Close Reason */}
          <div className="grid gap-2">
            <Label htmlFor="reason">
              Reason <span className="text-destructive">*</span>
            </Label>
            <Select value={reason} onValueChange={(v) => setReason(v as CloseReason)}>
              <SelectTrigger id="reason">
                <SelectValue placeholder="Select a reason" />
              </SelectTrigger>
              <SelectContent>
                {closeReasons.map((r) => (
                  <SelectItem key={r.value} value={r.value}>
                    <div>
                      <span className="font-medium">{r.label}</span>
                      <span className="text-muted-foreground ml-2 text-xs">
                        - {r.description}
                      </span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Explanation */}
          <div className="grid gap-2">
            <Label htmlFor="explanation">
              Explanation <span className="text-destructive">*</span>
            </Label>
            <Textarea
              id="explanation"
              placeholder="Provide details about why this task is being closed..."
              value={explanation}
              onChange={(e) => setExplanation(e.target.value)}
              className="min-h-[120px]"
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            variant="secondary"
            onClick={handleSubmit}
            disabled={!isFormValid}
          >
            <XCircle className="mr-2 h-4 w-4" />
            Close Task
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
