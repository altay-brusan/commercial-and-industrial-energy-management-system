"use client";

import { useState, useEffect } from "react";
import { Play, Pause, Clock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { WorkSession } from "@/lib/maintenance/types";

interface TaskWorkTimerProps {
  sessions: WorkSession[];
  isWorking: boolean;
  currentSessionStart?: Date;
  onStartWork: () => void;
  onStopWork: () => void;
  disabled?: boolean;
}

function formatDuration(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  if (hours > 0) {
    return `${hours}h ${minutes}m ${seconds}s`;
  }
  if (minutes > 0) {
    return `${minutes}m ${seconds}s`;
  }
  return `${seconds}s`;
}

function calculateTotalTime(sessions: WorkSession[]): number {
  return sessions.reduce((total, session) => {
    if (session.endedAt) {
      return total + (session.endedAt.getTime() - session.startedAt.getTime());
    }
    return total;
  }, 0);
}

export function TaskWorkTimer({
  sessions,
  isWorking,
  currentSessionStart,
  onStartWork,
  onStopWork,
  disabled,
}: TaskWorkTimerProps) {
  const [currentTime, setCurrentTime] = useState(0);

  // Update current session time every second when working
  useEffect(() => {
    if (!isWorking || !currentSessionStart) {
      setCurrentTime(0);
      return;
    }

    const interval = setInterval(() => {
      setCurrentTime(Date.now() - currentSessionStart.getTime());
    }, 1000);

    return () => clearInterval(interval);
  }, [isWorking, currentSessionStart]);

  const totalPreviousTime = calculateTotalTime(sessions);
  const totalTime = totalPreviousTime + currentTime;

  return (
    <Card className={cn(isWorking && "border-energy-consumption")}>
      <CardContent className="p-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div
              className={cn(
                "p-2 rounded-full",
                isWorking
                  ? "bg-energy-consumption/10 text-energy-consumption"
                  : "bg-muted text-muted-foreground"
              )}
            >
              <Clock className="h-5 w-5" />
            </div>
            <div>
              <p className="text-sm font-medium">Work Time</p>
              <p className="text-2xl font-semibold tracking-tight">
                {formatDuration(totalTime)}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {isWorking ? (
              <>
                <span className="flex items-center gap-1.5 text-sm text-energy-consumption">
                  <span className="h-2 w-2 rounded-full bg-energy-consumption animate-pulse" />
                  Working
                </span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={onStopWork}
                  disabled={disabled}
                  className="border-energy-consumption text-energy-consumption hover:bg-energy-consumption/10"
                >
                  <Pause className="mr-1.5 h-4 w-4" />
                  Pause
                </Button>
              </>
            ) : (
              <Button
                size="sm"
                onClick={onStartWork}
                disabled={disabled}
                className="bg-energy-solar hover:bg-energy-solar/90"
              >
                <Play className="mr-1.5 h-4 w-4" />
                Start Work
              </Button>
            )}
          </div>
        </div>

        {sessions.length > 0 && (
          <div className="mt-3 pt-3 border-t">
            <p className="text-xs text-muted-foreground mb-2">Work Sessions</p>
            <div className="space-y-1">
              {sessions.slice(0, 3).map((session, index) => (
                <div
                  key={session.id}
                  className="flex items-center justify-between text-xs"
                >
                  <span className="text-muted-foreground">
                    {session.startedAt.toLocaleTimeString("en-US", {
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                    {session.endedAt &&
                      ` - ${session.endedAt.toLocaleTimeString("en-US", {
                        hour: "2-digit",
                        minute: "2-digit",
                      })}`}
                  </span>
                  <span className="font-medium">
                    {session.endedAt
                      ? formatDuration(
                          session.endedAt.getTime() - session.startedAt.getTime()
                        )
                      : "In progress"}
                  </span>
                </div>
              ))}
              {sessions.length > 3 && (
                <p className="text-xs text-muted-foreground">
                  +{sessions.length - 3} more sessions
                </p>
              )}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
