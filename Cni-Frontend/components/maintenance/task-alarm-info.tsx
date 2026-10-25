"use client";

import { AlertTriangle, Bell, Clock, User, Activity } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { cn } from "@/lib/utils";
import type { RelatedAlarm, AlarmSeverity } from "@/lib/maintenance/types";

interface TaskAlarmInfoProps {
  alarm: RelatedAlarm;
}

function getSeverityColor(severity: AlarmSeverity): string {
  switch (severity) {
    case "critical":
      return "bg-energy-grid/10 text-energy-grid border-energy-grid/30";
    case "high":
      return "bg-energy-consumption/10 text-energy-consumption border-energy-consumption/30";
    case "medium":
      return "bg-energy-battery/10 text-energy-battery border-energy-battery/30";
    case "low":
      return "bg-muted text-muted-foreground border-muted-foreground/30";
  }
}

function getSeverityBadgeColor(severity: AlarmSeverity): string {
  switch (severity) {
    case "critical":
      return "bg-energy-grid text-white";
    case "high":
      return "bg-energy-consumption text-white";
    case "medium":
      return "bg-energy-battery text-white";
    case "low":
      return "bg-muted text-muted-foreground";
  }
}

function formatDateTime(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function TaskAlarmInfo({ alarm }: TaskAlarmInfoProps) {
  return (
    <Card className={cn("border-l-4", getSeverityColor(alarm.severity))}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base flex items-center gap-2">
            <Bell className="h-4 w-4" />
            Source Alarm
          </CardTitle>
          <Badge className={getSeverityBadgeColor(alarm.severity)}>
            {alarm.severity.toUpperCase()}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Alarm Message */}
        <div className="flex items-start gap-3 p-3 rounded-lg bg-muted/50">
          <AlertTriangle className="h-5 w-5 text-energy-grid shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium">{alarm.message}</p>
            <p className="text-xs text-muted-foreground mt-1 font-mono">
              {alarm.code}
            </p>
          </div>
        </div>

        {/* Alarm Details */}
        <div className="grid gap-3 text-sm">
          <div className="flex items-center justify-between">
            <span className="text-muted-foreground flex items-center gap-2">
              <Activity className="h-4 w-4" />
              Source
            </span>
            <span className="font-medium">{alarm.source}</span>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-muted-foreground flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Triggered
            </span>
            <span>{formatDateTime(alarm.triggeredAt)}</span>
          </div>

          {alarm.value && (
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground">Measured Value</span>
              <span className="font-mono text-energy-grid font-medium">
                {alarm.value}
              </span>
            </div>
          )}

          {alarm.threshold && (
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground">Threshold</span>
              <span className="font-mono">{alarm.threshold}</span>
            </div>
          )}

          <div className="flex items-center justify-between">
            <span className="text-muted-foreground">Alarm ID</span>
            <span className="font-mono text-xs">{alarm.id}</span>
          </div>
        </div>

        {/* Acknowledgment Info */}
        {alarm.acknowledgedAt && alarm.acknowledgedBy && (
          <div className="pt-3 border-t">
            <p className="text-xs text-muted-foreground mb-2">Acknowledged</p>
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Avatar size="sm">
                  <AvatarImage src={alarm.acknowledgedBy.avatar} />
                  <AvatarFallback className="text-[10px]">
                    {alarm.acknowledgedBy.initials}
                  </AvatarFallback>
                </Avatar>
                <span className="text-sm">{alarm.acknowledgedBy.name}</span>
              </div>
              <span className="text-xs text-muted-foreground">
                {formatDateTime(alarm.acknowledgedAt)}
              </span>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
