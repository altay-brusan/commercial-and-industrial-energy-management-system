"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getStatusLabel, getStatusColor } from "./task-status-badge";
import { cn } from "@/lib/utils";
import type { StatusChange } from "@/lib/maintenance/types";

interface TaskTimelineProps {
  statusChanges: StatusChange[];
}

function formatDateTime(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function getTimelineMessage(change: StatusChange): string {
  if (!change.fromStatus) {
    return `Task created with status "${getStatusLabel(change.toStatus)}"`;
  }
  return `Status changed from "${getStatusLabel(change.fromStatus)}" to "${getStatusLabel(change.toStatus)}"`;
}

export function TaskTimeline({ statusChanges }: TaskTimelineProps) {
  if (statusChanges.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Timeline</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground text-center py-4">
            No status changes recorded yet.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Timeline</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="relative">
          {/* Timeline line */}
          <div className="absolute left-3 top-3 bottom-3 w-px bg-border" />

          {/* Timeline items */}
          <div className="space-y-6">
            {statusChanges.map((change, index) => (
              <div key={change.id} className="relative flex gap-4">
                {/* Status dot */}
                <div
                  className={cn(
                    "relative z-10 h-6 w-6 rounded-full flex items-center justify-center ring-4 ring-background",
                    change.toStatus === "scheduled" && "bg-muted-foreground",
                    change.toStatus === "pending" && "bg-energy-battery",
                    change.toStatus === "in_progress" && "bg-energy-consumption",
                    change.toStatus === "completed" && "bg-energy-solar",
                    change.toStatus === "overdue" && "bg-energy-grid"
                  )}
                >
                  <div className="h-2 w-2 rounded-full bg-white" />
                </div>

                {/* Content */}
                <div className="flex-1 pb-2">
                  <div className="flex items-center gap-2 text-sm font-medium">
                    <span>{getTimelineMessage(change)}</span>
                  </div>

                  <div className="flex items-center gap-2 mt-1.5 text-xs text-muted-foreground">
                    <Avatar size="sm">
                      <AvatarImage src={change.changedBy.avatar} />
                      <AvatarFallback className="text-[8px]">
                        {change.changedBy.initials}
                      </AvatarFallback>
                    </Avatar>
                    <span>by {change.changedBy.name}</span>
                    <span>|</span>
                    <span>{formatDateTime(change.changedAt)}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
