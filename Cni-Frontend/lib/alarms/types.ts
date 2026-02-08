import type { AlarmSeverity, User, Asset } from "@/lib/maintenance/types";

// Alarm Status
export type AlarmStatus = "active" | "acknowledged" | "resolved";

// Alarm Source Type
export type AlarmSourceType = "solar" | "bess" | "grid" | "inverter" | "system";

// Alarm
export interface Alarm {
  id: string;
  code: string;
  message: string;
  severity: AlarmSeverity;
  status: AlarmStatus;
  source: string;
  sourceType: AlarmSourceType;
  triggeredAt: Date;
  value?: string;
  threshold?: string;
  acknowledgedAt?: Date;
  acknowledgedBy?: User;
  resolvedAt?: Date;
  resolvedBy?: User;
  relatedAsset?: Asset;
  relatedMaintenanceTaskId?: string;
  location?: string;
}

// Alarm Filters
export interface AlarmFilters {
  severity?: AlarmSeverity[];
  status?: AlarmStatus[];
  sourceType?: AlarmSourceType[];
  dateRange?: { from: Date; to: Date };
  alarmCode?: string;
  maintenanceTaskStatus?: "has_task" | "no_task" | "task_in_progress" | "task_completed";
  location?: string;
}

// Notification Type
export type NotificationType =
  | "maintenance_completed"
  | "system_update"
  | "alarm_resolved"
  | "schedule_reminder"
  | "system_alert";

// System Notification
export interface SystemNotification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  timestamp: Date;
  read: boolean;
  relatedAlarmId?: string;
  relatedTaskId?: string;
}

// Notification Filters
export interface NotificationFilters {
  type?: NotificationType;
  readStatus?: "all" | "read" | "unread";
}
