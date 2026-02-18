"use client";

import { useState } from "react";
import { CheckCircle2 } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import type { TaskCompletionDetails, User } from "@/lib/maintenance/types";

interface TaskCompletionModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  taskTitle: string;
  onComplete: (details: Omit<TaskCompletionDetails, "completedAt" | "completedBy">) => void;
}

export function TaskCompletionModal({
  open,
  onOpenChange,
  taskTitle,
  onComplete,
}: TaskCompletionModalProps) {
  const [resolution, setResolution] = useState("");
  const [rootCause, setRootCause] = useState("");
  const [laborHours, setLaborHours] = useState("");
  const [partsUsed, setPartsUsed] = useState("");
  const [recommendations, setRecommendations] = useState("");

  const handleSubmit = () => {
    if (!resolution || !laborHours) return;

    onComplete({
      resolution,
      rootCause: rootCause || undefined,
      laborHours: parseFloat(laborHours),
      partsUsed: partsUsed || undefined,
      recommendations: recommendations || undefined,
    });

    // Reset form
    setResolution("");
    setRootCause("");
    setLaborHours("");
    setPartsUsed("");
    setRecommendations("");
    onOpenChange(false);
  };

  const isFormValid = resolution.trim() && laborHours && parseFloat(laborHours) > 0;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[550px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CheckCircle2 className="h-5 w-5 text-energy-solar" />
            Complete Task
          </DialogTitle>
          <DialogDescription>
            Completing: <span className="font-medium text-foreground">{taskTitle}</span>
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Resolution - Required */}
          <div className="grid gap-2">
            <Label htmlFor="resolution">
              Resolution / Work Performed <span className="text-destructive">*</span>
            </Label>
            <Textarea
              id="resolution"
              placeholder="Describe what was done to complete this task..."
              value={resolution}
              onChange={(e) => setResolution(e.target.value)}
              className="min-h-[100px]"
            />
          </div>

          {/* Root Cause */}
          <div className="grid gap-2">
            <Label htmlFor="rootCause">Root Cause (if applicable)</Label>
            <Textarea
              id="rootCause"
              placeholder="What was the underlying cause of the issue?"
              value={rootCause}
              onChange={(e) => setRootCause(e.target.value)}
              className="min-h-[60px]"
            />
          </div>

          {/* Labor Hours & Parts Used */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label htmlFor="laborHours">
                Labor Hours <span className="text-destructive">*</span>
              </Label>
              <Input
                id="laborHours"
                type="number"
                step="0.5"
                min="0"
                placeholder="e.g., 2.5"
                value={laborHours}
                onChange={(e) => setLaborHours(e.target.value)}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="partsUsed">Parts/Materials Used</Label>
              <Input
                id="partsUsed"
                placeholder="e.g., Filter, Sensor T-03"
                value={partsUsed}
                onChange={(e) => setPartsUsed(e.target.value)}
              />
            </div>
          </div>

          {/* Recommendations */}
          <div className="grid gap-2">
            <Label htmlFor="recommendations">Recommendations / Follow-up</Label>
            <Textarea
              id="recommendations"
              placeholder="Any recommendations for future maintenance or follow-up actions..."
              value={recommendations}
              onChange={(e) => setRecommendations(e.target.value)}
              className="min-h-[60px]"
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={!isFormValid}
            className="bg-energy-solar hover:bg-energy-solar/90"
          >
            <CheckCircle2 className="mr-2 h-4 w-4" />
            Complete Task
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
