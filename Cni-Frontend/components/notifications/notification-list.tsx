"use client";

import { useMemo } from "react";
import {
  isToday,
  isYesterday,
  isThisWeek,
} from "date-fns";
import { NotificationCard } from "./notification-card";
import type { SystemNotification } from "@/lib/alarms/types";

interface NotificationListProps {
  notifications: SystemNotification[];
  onToggleRead: (id: string) => void;
}

interface NotificationGroup {
  label: string;
  notifications: SystemNotification[];
}

function groupNotifications(notifications: SystemNotification[]): NotificationGroup[] {
  const today: SystemNotification[] = [];
  const yesterday: SystemNotification[] = [];
  const thisWeek: SystemNotification[] = [];
  const older: SystemNotification[] = [];

  for (const notif of notifications) {
    if (isToday(notif.timestamp)) {
      today.push(notif);
    } else if (isYesterday(notif.timestamp)) {
      yesterday.push(notif);
    } else if (isThisWeek(notif.timestamp)) {
      thisWeek.push(notif);
    } else {
      older.push(notif);
    }
  }

  const groups: NotificationGroup[] = [];
  if (today.length > 0) groups.push({ label: "Today", notifications: today });
  if (yesterday.length > 0) groups.push({ label: "Yesterday", notifications: yesterday });
  if (thisWeek.length > 0) groups.push({ label: "This Week", notifications: thisWeek });
  if (older.length > 0) groups.push({ label: "Older", notifications: older });

  return groups;
}

export function NotificationList({ notifications, onToggleRead }: NotificationListProps) {
  const groups = useMemo(() => groupNotifications(notifications), [notifications]);

  if (notifications.length === 0) {
    return (
      <div className="py-12 text-center text-muted-foreground">
        No notifications found.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {groups.map((group) => (
        <div key={group.label} className="space-y-3">
          <h3 className="text-sm font-medium text-muted-foreground px-1">
            {group.label}
          </h3>
          <div className="space-y-2">
            {group.notifications.map((notification) => (
              <NotificationCard
                key={notification.id}
                notification={notification}
                onToggleRead={onToggleRead}
              />
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
