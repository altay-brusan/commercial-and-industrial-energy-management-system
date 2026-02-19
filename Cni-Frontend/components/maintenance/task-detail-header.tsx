"use client";

import Link from "next/link";
import { ArrowLeft, Clock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { TaskPriorityBadge } from "./task-status-badge";
import { TaskStatusDropdown } from "./task-status-dropdown";
import type { MaintenanceTask, TaskStatus } from "@/lib/maintenance/types";

interface TaskDetailHeaderProps {
  task: MaintenanceTask;
  onStatusChange?: (newStatus: TaskStatus) => void;
}

function formatDate(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function getRelativeTime(date: Date): string {
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / (1000 * 60));
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  if (diffMins < 60) return `${diffMins} minutes ago`;
  if (diffHours < 24) return `${diffHours} hours ago`;
  return `${diffDays} days ago`;
}

export function TaskDetailHeader({ task, onStatusChange }: TaskDetailHeaderProps) {
  return (
    <div className="border-b border-border bg-background">
      <div className="px-6 py-4">
        {/* Back button and status */}
        <div className="flex items-center justify-between mb-4">
          <Link href="/maintenance">
            <Button variant="ghost" size="sm" className="gap-2">
              <ArrowLeft className="h-4 w-4" />
              Back
            </Button>
          </Link>

          <TaskStatusDropdown
            currentStatus={task.status}
            onStatusChange={onStatusChange}
          />
        </div>

        {/* Task title and info */}
        <div className="space-y-3">
          <div className="flex items-start gap-3">
            <h1 className="text-xl font-semibold flex-1">
              <span className="text-muted-foreground font-normal">
                Task #{task.id.replace("task-", "")}:
              </span>{" "}
              {task.title}
            </h1>
            <TaskPriorityBadge priority={task.priority} />
          </div>

          <p className="text-sm text-muted-foreground max-w-2xl">
            {task.description}
          </p>

          {/* Meta info */}
          <div className="flex flex-wrap items-center gap-6 text-sm">
            {/* Assigned By */}
            <div className="flex items-center gap-2">
              <span className="text-muted-foreground">Assigned by:</span>
              <div className="flex items-center gap-1.5">
                <Avatar size="sm">
                  <AvatarImage src={task.assignedBy.avatar} />
                  <AvatarFallback className="text-[10px]">
                    {task.assignedBy.initials}
                  </AvatarFallback>
                </Avatar>
                <span>{task.assignedBy.name}</span>
              </div>
            </div>

            {/* Assigned To */}
            <div className="flex items-center gap-2">
              <span className="text-muted-foreground">Assigned to:</span>
              <div className="flex items-center gap-1.5">
                <Avatar size="sm">
                  <AvatarImage src={task.assignedTo.avatar} />
                  <AvatarFallback className="text-[10px]">
                    {task.assignedTo.initials}
                  </AvatarFallback>
                </Avatar>
                <span>{task.assignedTo.name}</span>
              </div>
            </div>

            {/* Due Date */}
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Due:</span>
              <span
                className={task.status === "overdue" ? "text-energy-grid font-medium" : ""}
              >
                {formatDate(task.dueDate)}
              </span>
            </div>

            {/* Related Asset */}
            {task.relatedAsset && (
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">Asset:</span>
                <span>{task.relatedAsset.name}</span>
              </div>
            )}
          </div>

          {/* Last updated */}
          <p className="text-xs text-muted-foreground">
            Assigned on {formatDate(task.assignedDate)}
          </p>
        </div>
      </div>
    </div>
  );
}
