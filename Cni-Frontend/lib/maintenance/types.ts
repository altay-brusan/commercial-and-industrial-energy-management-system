// Görev Durumları
export type TaskStatus = "scheduled" | "pending" | "in_progress" | "completed" | "closed" | "overdue";

// Kapatma Nedeni
export type CloseReason = "cancelled" | "duplicate" | "not_needed" | "transferred" | "other";

// Öncelik Seviyeleri
export type TaskPriority = "low" | "medium" | "high" | "critical";

// Kullanıcı
export interface User {
  id: string;
  name: string;
  email: string;
  avatar?: string;
  initials: string;
  role?: string;
  department?: string;
  phone?: string;
}

// İlgili Ekipman
export interface Asset {
  id: string;
  name: string;
  type: "solar" | "bess" | "grid" | "inverter" | "other";
  location?: string;
  capacity?: string;
  status?: "operational" | "maintenance" | "offline" | "warning";
  lastMaintenance?: Date;
  serialNumber?: string;
}

// Alarm Severity
export type AlarmSeverity = "critical" | "high" | "medium" | "low";

// İlişkili Alarm
export interface RelatedAlarm {
  id: string;
  code: string;
  message: string;
  severity: AlarmSeverity;
  source: string;
  triggeredAt: Date;
  value?: string;
  threshold?: string;
  acknowledgedAt?: Date;
  acknowledgedBy?: User;
}

// Checklist Item
export interface ChecklistItem {
  id: string;
  label: string;
  completed: boolean;
  completedAt?: Date;
  completedBy?: User;
}

// Görev Tamamlama Detayları
export interface TaskCompletionDetails {
  resolution: string;
  rootCause?: string;
  laborHours: number;
  partsUsed?: string;
  recommendations?: string;
  completedAt: Date;
  completedBy: User;
}

// Görev Kapatma Detayları
export interface TaskCloseDetails {
  reason: CloseReason;
  explanation: string;
  closedAt: Date;
  closedBy: User;
}

// Çalışma Oturumu
export interface WorkSession {
  id: string;
  taskId: string;
  startedAt: Date;
  endedAt?: Date;
  user: User;
}

// Bakım Görevi
export interface MaintenanceTask {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  assignedTo: User;
  assignedBy: User;
  assignedDate: Date;
  dueDate: Date;
  relatedAsset?: Asset;
  relatedAlarm?: RelatedAlarm;
  checklist?: ChecklistItem[];
  workSessions?: WorkSession[];
  completionDetails?: TaskCompletionDetails;
  closeDetails?: TaskCloseDetails;
}

// Durum Değişiklik Geçmişi
export interface StatusChange {
  id: string;
  taskId: string;
  fromStatus: TaskStatus | null;
  toStatus: TaskStatus;
  changedBy: User;
  changedAt: Date;
}

// Görev Notu
export interface TaskNote {
  id: string;
  taskId: string;
  content: string;
  author: User;
  createdAt: Date;
}

// Planlı Bakım
export interface ScheduledMaintenance {
  id: string;
  title: string;
  scheduledDate: Date;
  assetName: string;
  recurrence?: "daily" | "weekly" | "monthly" | "quarterly" | "yearly";
}

// Filtre Seçenekleri
export interface TaskFilters {
  status?: TaskStatus[];
  priority?: TaskPriority[];
  assignedTo?: string;
  dateRange?: {
    from: Date;
    to: Date;
  };
}
