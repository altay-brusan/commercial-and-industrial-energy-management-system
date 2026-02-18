"use client";

import { CheckCircle2, Play, AlertTriangle } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { TaskStatus } from "@/lib/maintenance/types";

interface TaskActionsProps {
  status: TaskStatus;
  isWorking: boolean;
  onStartWork: () => void;
  onCompleteTask: () => void;
  onMarkOverdue?: () => void;
}

export function TaskActions({
  status,
  isWorking,
  onStartWork,
  onCompleteTask,
  onMarkOverdue,
}: TaskActionsProps) {
  // Don't show actions for completed tasks
  if (status === "completed") {
    return null;
  }

  const canStartWork = !isWorking && status !== "scheduled";
  const canComplete = status !== "scheduled";

  return (
    <div className="flex items-center gap-3">
      {/* Start Work - show only if not already working and not scheduled */}
      {canStartWork && (
        <Button
          variant="outline"
          onClick={onStartWork}
          className="border-energy-solar text-energy-solar hover:bg-energy-solar/10"
        >
          <Play className="mr-2 h-4 w-4" />
          Start Work
        </Button>
      )}

      {/* Complete Task */}
      {canComplete && (
        <Button
          onClick={onCompleteTask}
          className="bg-energy-solar hover:bg-energy-solar/90"
        >
          <CheckCircle2 className="mr-2 h-4 w-4" />
          Complete Task
        </Button>
      )}

      {/* Mark as Overdue - optional action */}
      {status === "pending" && onMarkOverdue && (
        <Button
          variant="ghost"
          size="sm"
          onClick={onMarkOverdue}
          className="text-energy-grid hover:text-energy-grid hover:bg-energy-grid/10"
        >
          <AlertTriangle className="mr-2 h-4 w-4" />
          Mark Overdue
        </Button>
      )}
    </div>
  );
}
