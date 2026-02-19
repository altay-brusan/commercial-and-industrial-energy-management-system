"use client";

import { format } from "date-fns";
import Link from "next/link";
import {
  Wrench,
  RefreshCw,
  CheckCircle2,
  CalendarClock,
  AlertTriangle,
  Eye,
  EyeOff,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { SystemNotification, NotificationType } from "@/lib/alarms/types";

interface NotificationCardProps {
  notification: SystemNotification;
  onToggleRead: (id: string) => void;
}

function getNotificationIcon(type: NotificationType) {
  switch (type) {
    case "maintenance_completed":
      return { icon: Wrench, color: "text-energy-solar", bg: "bg-energy-solar/10" };
    case "system_update":
      return { icon: RefreshCw, color: "text-energy-battery", bg: "bg-energy-battery/10" };
    case "alarm_resolved":
      return { icon: CheckCircle2, color: "text-energy-solar", bg: "bg-energy-solar/10" };
    case "schedule_reminder":
      return { icon: CalendarClock, color: "text-energy-battery", bg: "bg-energy-battery/10" };
    case "system_alert":
      return { icon: AlertTriangle, color: "text-energy-grid", bg: "bg-energy-grid/10" };
  }
}

export function NotificationCard({ notification, onToggleRead }: NotificationCardProps) {
  const { icon: Icon, color, bg } = getNotificationIcon(notification.type);

  const linkHref = notification.relatedAlarmId
    ? `/alarms`
    : notification.relatedTaskId
      ? `/maintenance/${notification.relatedTaskId}`
      : undefined;

  return (
    <div
      className={cn(
        "flex items-start gap-4 rounded-lg border border-border p-4 transition-colors",
        !notification.read && "bg-accent/30 border-energy-battery/20"
      )}
    >
      {/* Unread indicator */}
      <div className="flex flex-col items-center gap-2 pt-1">
        <div className={cn("rounded-full p-2", bg)}>
          <Icon className={cn("h-4 w-4", color)} />
        </div>
        {!notification.read && (
          <div className="h-2 w-2 rounded-full bg-energy-battery" />
        )}
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-start justify-between gap-2">
          <h4 className={cn("text-sm", !notification.read ? "font-semibold" : "font-medium")}>
            {notification.title}
          </h4>
          <span className="text-xs text-muted-foreground shrink-0">
            {format(notification.timestamp, "HH:mm")}
          </span>
        </div>
        <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
          {notification.message}
        </p>
        <div className="flex items-center gap-2 mt-2">
          {linkHref && (
            <Link
              href={linkHref}
              className="text-xs text-primary hover:underline"
            >
              View details
            </Link>
          )}
          <Button
            variant="ghost"
            size="sm"
            className="h-auto p-1 text-xs text-muted-foreground ml-auto"
            onClick={() => onToggleRead(notification.id)}
          >
            {notification.read ? (
              <>
                <EyeOff className="mr-1 h-3 w-3" />
                Mark unread
              </>
            ) : (
              <>
                <Eye className="mr-1 h-3 w-3" />
                Mark read
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
