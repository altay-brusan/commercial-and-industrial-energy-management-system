"use client";

import { useState, useMemo } from "react";
import { CheckCheck } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { NotificationFilterBar } from "@/components/notifications/notification-filter-bar";
import { NotificationList } from "@/components/notifications/notification-list";
import { notifications as allNotifications } from "@/lib/alarms/mock-data";
import type { SystemNotification, NotificationFilters } from "@/lib/alarms/types";

export default function NotificationsPage() {
  const [notifications, setNotifications] = useState<SystemNotification[]>(allNotifications);
  const [filters, setFilters] = useState<NotificationFilters>({});

  const unreadCount = useMemo(
    () => notifications.filter((n) => !n.read).length,
    [notifications]
  );

  const filteredNotifications = useMemo(() => {
    return notifications
      .filter((n) => {
        // Type filter
        if (filters.type && n.type !== filters.type) return false;

        // Read status filter
        if (filters.readStatus === "read" && !n.read) return false;
        if (filters.readStatus === "unread" && n.read) return false;

        return true;
      })
      .sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime());
  }, [notifications, filters]);

  const handleToggleRead = (id: string) => {
    setNotifications((prev) =>
      prev.map((n) => (n.id === id ? { ...n, read: !n.read } : n))
    );
  };

  const handleMarkAllRead = () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="notifications" />

      <main className="flex-1 flex flex-col overflow-hidden">
        {/* Header - Sticky */}
        <div className="shrink-0 border-b border-border bg-background z-20">
          <div className="max-w-3xl mx-auto px-6 py-5 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <h1 className="text-xl font-semibold">Notifications</h1>
                {unreadCount > 0 && (
                  <Badge variant="secondary" className="text-xs">
                    {unreadCount} unread
                  </Badge>
                )}
              </div>
              {unreadCount > 0 && (
                <Button
                  variant="outline"
                  size="sm"
                  className="gap-2"
                  onClick={handleMarkAllRead}
                >
                  <CheckCheck className="h-4 w-4" />
                  Mark All as Read
                </Button>
              )}
            </div>

            <NotificationFilterBar
              filters={filters}
              onFiltersChange={setFilters}
            />
          </div>
        </div>

        {/* Notification List - Scrollable */}
        <div className="flex-1 overflow-y-auto">
          <div className="max-w-3xl mx-auto px-6 py-6">
            <NotificationList
              notifications={filteredNotifications}
              onToggleRead={handleToggleRead}
            />
          </div>
        </div>
      </main>
    </div>
  );
}
