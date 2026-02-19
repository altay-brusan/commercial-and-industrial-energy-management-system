"use client";

import { useState } from "react";
import Link from "next/link";
import { format } from "date-fns";
import {
  X,
  Clock,
  MapPin,
  Gauge,
  CheckCircle2,
  Eye,
  Wrench,
  ArrowRight,
  AlertTriangle,
  Zap,
  Battery,
  Sun,
  Radio,
  Server,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { AlarmSeverityBadge } from "./alarm-severity-badge";
import { AlarmStatusBadge } from "./alarm-status-badge";
import { TaskAssignmentModal } from "@/components/maintenance/task-assignment-modal";
import { getRelatedMaintenanceTask, getAlarmsBySource } from "@/lib/alarms/mock-data";
import { cn } from "@/lib/utils";
import type { Alarm } from "@/lib/alarms/types";

interface AlarmDetailPanelProps {
  alarm: Alarm;
  onClose: () => void;
  onAcknowledge?: (alarm: Alarm) => void;
  onResolve?: (alarm: Alarm) => void;
}

function getSourceIcon(sourceType: string) {
  switch (sourceType) {
    case "solar":
      return Sun;
    case "bess":
      return Battery;
    case "grid":
      return Zap;
    case "inverter":
      return Radio;
    case "system":
      return Server;
    default:
      return AlertTriangle;
  }
}

function getAssetStatusColor(status?: string) {
  switch (status) {
    case "operational":
      return "text-energy-solar";
    case "warning":
      return "text-energy-consumption";
    case "maintenance":
      return "text-energy-battery";
    case "offline":
      return "text-muted-foreground";
    default:
      return "text-muted-foreground";
  }
}

export function AlarmDetailPanel({
  alarm,
  onClose,
  onAcknowledge,
  onResolve,
}: AlarmDetailPanelProps) {
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);

  const relatedTask = alarm.relatedMaintenanceTaskId
    ? getRelatedMaintenanceTask(alarm.relatedMaintenanceTaskId)
    : undefined;

  const sourceAlarms = getAlarmsBySource(alarm.source).filter(
    (a) => a.id !== alarm.id
  );

  const SourceIcon = getSourceIcon(alarm.sourceType);

  return (
    <>
      <aside className="w-[420px] shrink-0 border-l border-border flex flex-col bg-background overflow-hidden">
        {/* Sticky Header */}
        <div className="shrink-0 flex items-center justify-between px-5 py-4 border-b border-border bg-background">
          <div>
            <p className="text-sm font-mono text-muted-foreground">{alarm.id}</p>
            <p className="text-sm font-semibold mt-0.5">{alarm.code}</p>
          </div>
          <Button variant="ghost" size="icon" onClick={onClose}>
            <X className="h-4 w-4" />
          </Button>
        </div>

        {/* Scrollable Content */}
        <div className="flex-1 overflow-y-auto">
          <div className="p-5 space-y-6">
            {/* Alarm Info */}
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                <AlarmSeverityBadge severity={alarm.severity} />
                <AlarmStatusBadge status={alarm.status} />
              </div>
              <p className="text-sm">{alarm.message}</p>
              {(alarm.value || alarm.threshold) && (
                <div className="grid grid-cols-2 gap-3">
                  {alarm.value && (
                    <div className="rounded-lg border border-border p-3">
                      <p className="text-xs text-muted-foreground">Current Value</p>
                      <p className="text-sm font-semibold mt-1">{alarm.value}</p>
                    </div>
                  )}
                  {alarm.threshold && (
                    <div className="rounded-lg border border-border p-3">
                      <p className="text-xs text-muted-foreground">Threshold</p>
                      <p className="text-sm font-semibold mt-1">{alarm.threshold}</p>
                    </div>
                  )}
                </div>
              )}
            </div>

            <Separator />

            {/* Device / Asset Info */}
            <div className="space-y-3">
              <h4 className="text-sm font-medium">Device Information</h4>
              <div className="rounded-lg border border-border p-4 space-y-3">
                <div className="flex items-center gap-3">
                  <div className="rounded-md bg-muted p-2">
                    <SourceIcon className="h-4 w-4 text-muted-foreground" />
                  </div>
                  <div>
                    <p className="text-sm font-medium">{alarm.source}</p>
                    <p className="text-xs text-muted-foreground capitalize">{alarm.sourceType}</p>
                  </div>
                </div>
                {alarm.relatedAsset && (
                  <div className="grid grid-cols-2 gap-2 text-sm">
                    {alarm.relatedAsset.capacity && (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Gauge className="h-3.5 w-3.5" />
                        <span>{alarm.relatedAsset.capacity}</span>
                      </div>
                    )}
                    {alarm.relatedAsset.status && (
                      <div className={cn("flex items-center gap-2", getAssetStatusColor(alarm.relatedAsset.status))}>
                        <div className="h-2 w-2 rounded-full bg-current" />
                        <span className="capitalize">{alarm.relatedAsset.status}</span>
                      </div>
                    )}
                  </div>
                )}
                {alarm.location && (
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <MapPin className="h-3.5 w-3.5" />
                    <span>{alarm.location}</span>
                  </div>
                )}
              </div>
            </div>

            <Separator />

            {/* Timeline */}
            <div className="space-y-3">
              <h4 className="text-sm font-medium">Timeline</h4>
              <div className="space-y-0">
                {/* Triggered */}
                <div className="flex gap-3">
                  <div className="flex flex-col items-center">
                    <div className="h-2.5 w-2.5 rounded-full bg-energy-grid mt-1.5" />
                    <div className="w-px flex-1 bg-border" />
                  </div>
                  <div className="pb-4">
                    <p className="text-sm font-medium">Triggered</p>
                    <p className="text-xs text-muted-foreground">
                      {format(alarm.triggeredAt, "MMM d, yyyy HH:mm")}
                    </p>
                  </div>
                </div>

                {/* Acknowledged */}
                <div className="flex gap-3">
                  <div className="flex flex-col items-center">
                    <div className={cn(
                      "h-2.5 w-2.5 rounded-full mt-1.5",
                      alarm.acknowledgedAt ? "bg-energy-consumption" : "bg-muted"
                    )} />
                    <div className="w-px flex-1 bg-border" />
                  </div>
                  <div className="pb-4">
                    <p className={cn("text-sm font-medium", !alarm.acknowledgedAt && "text-muted-foreground")}>
                      Acknowledged
                    </p>
                    {alarm.acknowledgedAt ? (
                      <div className="space-y-1">
                        <p className="text-xs text-muted-foreground">
                          {format(alarm.acknowledgedAt, "MMM d, yyyy HH:mm")}
                        </p>
                        {alarm.acknowledgedBy && (
                          <div className="flex items-center gap-1.5">
                            <Avatar className="h-4 w-4">
                              <AvatarImage src={alarm.acknowledgedBy.avatar} />
                              <AvatarFallback className="text-[8px]">
                                {alarm.acknowledgedBy.initials}
                              </AvatarFallback>
                            </Avatar>
                            <span className="text-xs text-muted-foreground">
                              {alarm.acknowledgedBy.name}
                            </span>
                          </div>
                        )}
                      </div>
                    ) : (
                      <p className="text-xs text-muted-foreground">Pending</p>
                    )}
                  </div>
                </div>

                {/* Resolved */}
                <div className="flex gap-3">
                  <div className="flex flex-col items-center">
                    <div className={cn(
                      "h-2.5 w-2.5 rounded-full mt-1.5",
                      alarm.resolvedAt ? "bg-energy-solar" : "bg-muted"
                    )} />
                  </div>
                  <div>
                    <p className={cn("text-sm font-medium", !alarm.resolvedAt && "text-muted-foreground")}>
                      Resolved
                    </p>
                    {alarm.resolvedAt ? (
                      <div className="space-y-1">
                        <p className="text-xs text-muted-foreground">
                          {format(alarm.resolvedAt, "MMM d, yyyy HH:mm")}
                        </p>
                        {alarm.resolvedBy && (
                          <div className="flex items-center gap-1.5">
                            <Avatar className="h-4 w-4">
                              <AvatarImage src={alarm.resolvedBy.avatar} />
                              <AvatarFallback className="text-[8px]">
                                {alarm.resolvedBy.initials}
                              </AvatarFallback>
                            </Avatar>
                            <span className="text-xs text-muted-foreground">
                              {alarm.resolvedBy.name}
                            </span>
                          </div>
                        )}
                      </div>
                    ) : (
                      <p className="text-xs text-muted-foreground">Pending</p>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <Separator />

            {/* Related Maintenance Task */}
            {relatedTask && (
              <>
                <div className="space-y-3">
                  <h4 className="text-sm font-medium">Related Maintenance Task</h4>
                  <Link
                    href={`/maintenance/${relatedTask.id}`}
                    className="block rounded-lg border border-border p-4 hover:bg-accent/50 transition-colors"
                  >
                    <div className="flex items-center justify-between">
                      <p className="text-sm font-medium">{relatedTask.title}</p>
                      <ArrowRight className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div className="flex items-center gap-2 mt-2">
                      <Badge
                        variant="outline"
                        className="text-xs capitalize"
                      >
                        {relatedTask.status.replace("_", " ")}
                      </Badge>
                      <span className="text-xs text-muted-foreground">
                        Assigned to {relatedTask.assignedTo.name}
                      </span>
                    </div>
                  </Link>
                </div>
                <Separator />
              </>
            )}

            {/* Actions */}
            <div className="space-y-3">
              <h4 className="text-sm font-medium">Actions</h4>
              <div className="space-y-2">
                {alarm.status === "active" && (
                  <Button
                    variant="outline"
                    className="w-full justify-start gap-2"
                    onClick={() => onAcknowledge?.(alarm)}
                  >
                    <Eye className="h-4 w-4" />
                    Acknowledge Alarm
                  </Button>
                )}
                {alarm.status === "acknowledged" && (
                  <Button
                    variant="outline"
                    className="w-full justify-start gap-2"
                    onClick={() => onResolve?.(alarm)}
                  >
                    <CheckCircle2 className="h-4 w-4" />
                    Resolve Alarm
                  </Button>
                )}
                {!alarm.relatedMaintenanceTaskId && alarm.status !== "resolved" && (
                  <Button
                    variant="outline"
                    className="w-full justify-start gap-2"
                    onClick={() => setIsAssignModalOpen(true)}
                  >
                    <Wrench className="h-4 w-4" />
                    Assign to Maintenance
                  </Button>
                )}
              </div>
            </div>

            {/* Alarm History (from same source) */}
            {sourceAlarms.length > 0 && (
              <>
                <Separator />
                <div className="space-y-3">
                  <h4 className="text-sm font-medium">
                    Alarm History ({alarm.source})
                  </h4>
                  <div className="space-y-2">
                    {sourceAlarms.slice(0, 5).map((a) => (
                      <div
                        key={a.id}
                        className="flex items-center justify-between rounded-lg border border-border p-3"
                      >
                        <div className="space-y-1">
                          <p className="text-xs font-mono text-muted-foreground">{a.code}</p>
                          <p className="text-sm truncate max-w-[250px]">{a.message}</p>
                        </div>
                        <div className="flex items-center gap-2 shrink-0">
                          <AlarmSeverityBadge severity={a.severity} className="text-[10px] px-1.5 py-0" />
                          <span className="text-xs text-muted-foreground">
                            {format(a.triggeredAt, "MMM d")}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      </aside>

      {/* Task Assignment Modal */}
      <TaskAssignmentModal
        open={isAssignModalOpen}
        onOpenChange={setIsAssignModalOpen}
        defaultValues={{
          title: alarm.message,
          description: `Alarm ${alarm.code}: ${alarm.message}${alarm.value ? `\nCurrent Value: ${alarm.value}` : ""}${alarm.threshold ? `\nThreshold: ${alarm.threshold}` : ""}`,
          priority: alarm.severity,
          relatedAsset: alarm.relatedAsset?.id,
        }}
        onSubmit={(data) => {
          console.log("Creating maintenance task from alarm:", data);
          setIsAssignModalOpen(false);
        }}
      />
    </>
  );
}
