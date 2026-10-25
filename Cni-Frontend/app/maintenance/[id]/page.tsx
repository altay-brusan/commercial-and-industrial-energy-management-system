"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import {
  ArrowLeft,
  Clock,
  Play,
  Pause,
  CheckCircle2,
  AlertTriangle,
  Bell,
  Activity,
  Check,
  ChevronDown,
  XCircle,
  Mail,
  Phone,
  Building2,
  MapPin,
  Cpu,
  Calendar,
  Zap,
  Hash,
  User,
} from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { TaskCompletionModal } from "@/components/maintenance/task-completion-modal";
import { TaskCloseModal } from "@/components/maintenance/task-close-modal";
import { TaskPriorityBadge } from "@/components/maintenance/task-status-badge";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Progress } from "@/components/ui/progress";
import {
  HoverCard,
  HoverCardContent,
  HoverCardTrigger,
} from "@/components/ui/hover-card";
import { cn } from "@/lib/utils";
import {
  getTaskById,
  getStatusChangesForTask,
  getNotesForTask,
  getChecklistForTask,
  getWorkSessionsForTask,
  users,
} from "@/lib/maintenance/mock-data";
import type {
  MaintenanceTask,
  StatusChange,
  TaskNote,
  TaskStatus,
  ChecklistItem,
  WorkSession,
  TaskCompletionDetails,
  TaskCloseDetails,
  CloseReason,
} from "@/lib/maintenance/types";

function formatDate(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function formatDateTime(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

function formatDuration(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;
  if (hours > 0) return `${hours}h ${minutes}m ${seconds}s`;
  if (minutes > 0) return `${minutes}m ${seconds}s`;
  return `${seconds}s`;
}

function getStatusLabel(status: TaskStatus): string {
  const labels: Record<TaskStatus, string> = {
    scheduled: "Scheduled",
    pending: "Pending",
    in_progress: "In Progress",
    completed: "Completed",
    closed: "Closed",
    overdue: "Overdue",
  };
  return labels[status];
}

function getCloseReasonLabel(reason: CloseReason): string {
  const labels: Record<CloseReason, string> = {
    cancelled: "Cancelled",
    duplicate: "Duplicate",
    not_needed: "Not Needed",
    transferred: "Transferred",
    other: "Other",
  };
  return labels[reason];
}

const statusOptions: TaskStatus[] = ["scheduled", "pending", "in_progress", "completed", "closed", "overdue"];

export default function TaskDetailPage() {
  const params = useParams();
  const taskId = params.id as string;

  const [task, setTask] = useState<MaintenanceTask | null>(null);
  const [statusChanges, setStatusChanges] = useState<StatusChange[]>([]);
  const [notes, setNotes] = useState<TaskNote[]>([]);
  const [checklist, setChecklist] = useState<ChecklistItem[]>([]);
  const [workSessions, setWorkSessions] = useState<WorkSession[]>([]);
  const [loading, setLoading] = useState(true);
  const [noteContent, setNoteContent] = useState("");

  const [isWorking, setIsWorking] = useState(false);
  const [currentSessionStart, setCurrentSessionStart] = useState<Date | undefined>();
  const [currentTime, setCurrentTime] = useState(0);
  const [showCompletionModal, setShowCompletionModal] = useState(false);
  const [showCloseModal, setShowCloseModal] = useState(false);

  useEffect(() => {
    const foundTask = getTaskById(taskId);
    if (foundTask) {
      setTask(foundTask);
      setStatusChanges(getStatusChangesForTask(taskId));
      setNotes(getNotesForTask(taskId));
      setChecklist(getChecklistForTask(taskId));
      setWorkSessions(getWorkSessionsForTask(taskId));
    }
    setLoading(false);
  }, [taskId]);

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

  if (loading) {
    return (
      <div className="h-screen flex flex-col bg-background">
        <TopBar activeTab="maintenance" />
        <div className="flex items-center justify-center flex-1">
          <p className="text-muted-foreground">Loading...</p>
        </div>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="h-screen flex flex-col bg-background">
        <TopBar activeTab="maintenance" />
        <div className="flex items-center justify-center flex-1">
          <p className="text-muted-foreground">Task not found</p>
        </div>
      </div>
    );
  }

  const handleStatusChange = (newStatus: TaskStatus) => {
    const previousStatus = task.status;
    setTask({ ...task, status: newStatus });
    const newChange: StatusChange = {
      id: `change-${Date.now()}`,
      taskId: task.id,
      fromStatus: previousStatus,
      toStatus: newStatus,
      changedBy: users[0],
      changedAt: new Date(),
    };
    setStatusChanges([newChange, ...statusChanges]);
  };

  const handleAddNote = () => {
    if (!noteContent.trim()) return;
    const newNote: TaskNote = {
      id: `note-${Date.now()}`,
      taskId: task.id,
      content: noteContent.trim(),
      author: users[0],
      createdAt: new Date(),
    };
    setNotes([newNote, ...notes]);
    setNoteContent("");
  };

  const handleChecklistToggle = (itemId: string, completed: boolean) => {
    setChecklist(
      checklist.map((item) =>
        item.id === itemId
          ? { ...item, completed, completedBy: completed ? users[0] : undefined, completedAt: completed ? new Date() : undefined }
          : item
      )
    );
  };

  const handleStartWork = () => {
    setIsWorking(true);
    setCurrentSessionStart(new Date());
    if (task.status === "pending") {
      handleStatusChange("in_progress");
    }
  };

  const handleStopWork = () => {
    if (currentSessionStart) {
      const newSession: WorkSession = {
        id: `ws-${Date.now()}`,
        taskId: task.id,
        startedAt: currentSessionStart,
        endedAt: new Date(),
        user: users[0],
      };
      setWorkSessions([...workSessions, newSession]);
    }
    setIsWorking(false);
    setCurrentSessionStart(undefined);
  };

  const handleCompleteTask = (details: Omit<TaskCompletionDetails, "completedAt" | "completedBy">) => {
    if (isWorking) handleStopWork();
    setTask({
      ...task,
      status: "completed",
      completionDetails: { ...details, completedAt: new Date(), completedBy: users[0] },
    });
    const newChange: StatusChange = {
      id: `change-${Date.now()}`,
      taskId: task.id,
      fromStatus: task.status,
      toStatus: "completed",
      changedBy: users[0],
      changedAt: new Date(),
    };
    setStatusChanges([newChange, ...statusChanges]);
    const completionNote: TaskNote = {
      id: `note-${Date.now()}`,
      taskId: task.id,
      content: `**Task Completed**\n\n**Resolution:** ${details.resolution}${details.rootCause ? `\n\n**Root Cause:** ${details.rootCause}` : ""}\n\n**Labor Hours:** ${details.laborHours}h${details.partsUsed ? `\n**Parts Used:** ${details.partsUsed}` : ""}${details.recommendations ? `\n\n**Recommendations:** ${details.recommendations}` : ""}`,
      author: users[0],
      createdAt: new Date(),
    };
    setNotes([completionNote, ...notes]);
  };

  const handleCloseTask = (details: Omit<TaskCloseDetails, "closedAt" | "closedBy">) => {
    if (isWorking) handleStopWork();
    setTask({
      ...task,
      status: "closed",
      closeDetails: { ...details, closedAt: new Date(), closedBy: users[0] },
    });
    const newChange: StatusChange = {
      id: `change-${Date.now()}`,
      taskId: task.id,
      fromStatus: task.status,
      toStatus: "closed",
      changedBy: users[0],
      changedAt: new Date(),
    };
    setStatusChanges([newChange, ...statusChanges]);
    const closeNote: TaskNote = {
      id: `note-${Date.now()}`,
      taskId: task.id,
      content: `**Task Closed**\n\n**Reason:** ${getCloseReasonLabel(details.reason)}\n\n**Explanation:** ${details.explanation}`,
      author: users[0],
      createdAt: new Date(),
    };
    setNotes([closeNote, ...notes]);
  };

  const isTaskCompleted = task.status === "completed";
  const isTaskClosed = task.status === "closed";
  const isTaskFinished = isTaskCompleted || isTaskClosed;
  const totalPreviousTime = workSessions.reduce((total, session) => {
    if (session.endedAt) return total + (session.endedAt.getTime() - session.startedAt.getTime());
    return total;
  }, 0);
  const totalTime = totalPreviousTime + currentTime;
  const completedChecklistCount = checklist.filter((item) => item.completed).length;
  const checklistProgress = checklist.length > 0 ? (completedChecklistCount / checklist.length) * 100 : 0;

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="maintenance" />

      <div className="flex-1 overflow-y-auto">
        {/* Header */}
        <div className="px-6 py-5 border-b border-border">
          <div className="flex items-center justify-between mb-4">
            <Link href="/maintenance">
              <Button variant="ghost" size="sm" className="gap-2 -ml-2">
                <ArrowLeft className="h-4 w-4" />
                Back
              </Button>
            </Link>
            <div className="flex items-center gap-2">
              <TaskPriorityBadge priority={task.priority} />
              {/* Status Dropdown */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" size="sm" className="gap-1.5">
                    <span className={cn(
                      "h-2 w-2 rounded-full",
                      task.status === "scheduled" && "bg-muted-foreground",
                      task.status === "pending" && "bg-energy-battery",
                      task.status === "in_progress" && "bg-energy-consumption",
                      task.status === "completed" && "bg-energy-solar",
                      task.status === "closed" && "bg-muted-foreground",
                      task.status === "overdue" && "bg-energy-grid"
                    )} />
                    {getStatusLabel(task.status)}
                    <ChevronDown className="h-3 w-3 text-muted-foreground" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  {statusOptions.filter(s => s !== "completed" && s !== "closed").map((status) => (
                    <DropdownMenuItem
                      key={status}
                      onClick={() => handleStatusChange(status)}
                      className={cn("gap-2", task.status === status && "bg-accent")}
                    >
                      <span className={cn(
                        "h-2 w-2 rounded-full",
                        status === "scheduled" && "bg-muted-foreground",
                        status === "pending" && "bg-energy-battery",
                        status === "in_progress" && "bg-energy-consumption",
                        status === "overdue" && "bg-energy-grid"
                      )} />
                      {getStatusLabel(status)}
                    </DropdownMenuItem>
                  ))}
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onClick={() => setShowCompletionModal(true)}
                    className="gap-2 text-energy-solar"
                  >
                    <CheckCircle2 className="h-4 w-4" />
                    Complete Task...
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => setShowCloseModal(true)}
                    className="gap-2 text-muted-foreground"
                  >
                    <XCircle className="h-4 w-4" />
                    Close Task...
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            </div>
          </div>

          <h1 className="text-xl font-semibold mb-2">
            <span className="text-muted-foreground font-normal">#{task.id.replace("task-", "")}</span>{" "}
            {task.title}
          </h1>
          <p className="text-sm text-muted-foreground mb-4 max-w-3xl">{task.description}</p>

          <div className="flex flex-wrap items-center gap-6 text-sm">
            {/* Assigned By */}
            <div className="flex items-center gap-2">
              <span className="text-muted-foreground">Assigned by:</span>
              <HoverCard openDelay={200}>
                <HoverCardTrigger asChild>
                  <button className="flex items-center gap-2 hover:bg-muted px-2 py-1 -mx-2 -my-1 rounded-md transition-colors cursor-pointer">
                    <Avatar size="sm">
                      <AvatarImage src={task.assignedBy.avatar} />
                      <AvatarFallback className="text-[10px]">{task.assignedBy.initials}</AvatarFallback>
                    </Avatar>
                    <span className="underline decoration-dashed underline-offset-2">{task.assignedBy.name}</span>
                  </button>
                </HoverCardTrigger>
                <HoverCardContent className="w-72" align="start">
                  <div className="flex gap-3">
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={task.assignedBy.avatar} />
                      <AvatarFallback>{task.assignedBy.initials}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <h4 className="font-semibold">{task.assignedBy.name}</h4>
                      {task.assignedBy.role && (
                        <p className="text-sm text-muted-foreground">{task.assignedBy.role}</p>
                      )}
                    </div>
                  </div>
                  <div className="mt-3 space-y-2 text-sm">
                    {task.assignedBy.department && (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Building2 className="h-3.5 w-3.5" />
                        <span>{task.assignedBy.department}</span>
                      </div>
                    )}
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Mail className="h-3.5 w-3.5" />
                      <a href={`mailto:${task.assignedBy.email}`} className="text-primary hover:underline">
                        {task.assignedBy.email}
                      </a>
                    </div>
                    {task.assignedBy.phone && (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Phone className="h-3.5 w-3.5" />
                        <a href={`tel:${task.assignedBy.phone}`} className="text-primary hover:underline">
                          {task.assignedBy.phone}
                        </a>
                      </div>
                    )}
                  </div>
                </HoverCardContent>
              </HoverCard>
            </div>

            {/* Assigned To */}
            <div className="flex items-center gap-2">
              <span className="text-muted-foreground">Assigned to:</span>
              <HoverCard openDelay={200}>
                <HoverCardTrigger asChild>
                  <button className="flex items-center gap-2 hover:bg-muted px-2 py-1 -mx-2 -my-1 rounded-md transition-colors cursor-pointer">
                    <Avatar size="sm">
                      <AvatarImage src={task.assignedTo.avatar} />
                      <AvatarFallback className="text-[10px]">{task.assignedTo.initials}</AvatarFallback>
                    </Avatar>
                    <span className="underline decoration-dashed underline-offset-2">{task.assignedTo.name}</span>
                  </button>
                </HoverCardTrigger>
                <HoverCardContent className="w-72" align="start">
                  <div className="flex gap-3">
                    <Avatar className="h-12 w-12">
                      <AvatarImage src={task.assignedTo.avatar} />
                      <AvatarFallback>{task.assignedTo.initials}</AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <h4 className="font-semibold">{task.assignedTo.name}</h4>
                      {task.assignedTo.role && (
                        <p className="text-sm text-muted-foreground">{task.assignedTo.role}</p>
                      )}
                    </div>
                  </div>
                  <div className="mt-3 space-y-2 text-sm">
                    {task.assignedTo.department && (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Building2 className="h-3.5 w-3.5" />
                        <span>{task.assignedTo.department}</span>
                      </div>
                    )}
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Mail className="h-3.5 w-3.5" />
                      <a href={`mailto:${task.assignedTo.email}`} className="text-primary hover:underline">
                        {task.assignedTo.email}
                      </a>
                    </div>
                    {task.assignedTo.phone && (
                      <div className="flex items-center gap-2 text-muted-foreground">
                        <Phone className="h-3.5 w-3.5" />
                        <a href={`tel:${task.assignedTo.phone}`} className="text-primary hover:underline">
                          {task.assignedTo.phone}
                        </a>
                      </div>
                    )}
                  </div>
                </HoverCardContent>
              </HoverCard>
            </div>

            {/* Due Date */}
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-muted-foreground" />
              <span className="text-muted-foreground">Due:</span>
              <span className={task.status === "overdue" ? "text-energy-grid font-medium" : ""}>
                {formatDate(task.dueDate)}
              </span>
            </div>

            {/* Asset */}
            {task.relatedAsset && (
              <div className="flex items-center gap-2">
                <span className="text-muted-foreground">Asset:</span>
                <HoverCard openDelay={200}>
                  <HoverCardTrigger asChild>
                    <button className="flex items-center gap-1.5 hover:bg-muted px-2 py-1 -mx-2 -my-1 rounded-md transition-colors cursor-pointer">
                      <Cpu className="h-3.5 w-3.5 text-muted-foreground" />
                      <span className="underline decoration-dashed underline-offset-2">{task.relatedAsset.name}</span>
                    </button>
                  </HoverCardTrigger>
                  <HoverCardContent className="w-80" align="start">
                    <div className="flex items-start gap-3">
                      <div className={cn(
                        "h-10 w-10 rounded-lg flex items-center justify-center",
                        task.relatedAsset.type === "solar" && "bg-energy-solar/10 text-energy-solar",
                        task.relatedAsset.type === "bess" && "bg-energy-battery/10 text-energy-battery",
                        task.relatedAsset.type === "grid" && "bg-energy-grid/10 text-energy-grid",
                        task.relatedAsset.type === "inverter" && "bg-energy-consumption/10 text-energy-consumption",
                        task.relatedAsset.type === "other" && "bg-muted text-muted-foreground"
                      )}>
                        <Zap className="h-5 w-5" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <h4 className="font-semibold">{task.relatedAsset.name}</h4>
                        <div className="flex items-center gap-2 mt-0.5">
                          <Badge variant="outline" className="text-[10px] px-1.5 py-0 capitalize">
                            {task.relatedAsset.type}
                          </Badge>
                          {task.relatedAsset.status && (
                            <Badge className={cn(
                              "text-[10px] px-1.5 py-0",
                              task.relatedAsset.status === "operational" && "bg-energy-solar/10 text-energy-solar border-energy-solar/20",
                              task.relatedAsset.status === "warning" && "bg-energy-battery/10 text-energy-battery border-energy-battery/20",
                              task.relatedAsset.status === "maintenance" && "bg-energy-consumption/10 text-energy-consumption border-energy-consumption/20",
                              task.relatedAsset.status === "offline" && "bg-muted text-muted-foreground"
                            )} variant="outline">
                              {task.relatedAsset.status}
                            </Badge>
                          )}
                        </div>
                      </div>
                    </div>
                    <div className="mt-3 space-y-2 text-sm">
                      {task.relatedAsset.location && (
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <MapPin className="h-3.5 w-3.5" />
                          <span>{task.relatedAsset.location}</span>
                        </div>
                      )}
                      {task.relatedAsset.capacity && (
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <Zap className="h-3.5 w-3.5" />
                          <span>Capacity: <span className="text-foreground font-medium">{task.relatedAsset.capacity}</span></span>
                        </div>
                      )}
                      {task.relatedAsset.serialNumber && (
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <Hash className="h-3.5 w-3.5" />
                          <span className="font-mono text-xs">{task.relatedAsset.serialNumber}</span>
                        </div>
                      )}
                      {task.relatedAsset.lastMaintenance && (
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <Calendar className="h-3.5 w-3.5" />
                          <span>Last maintenance: {formatDate(task.relatedAsset.lastMaintenance)}</span>
                        </div>
                      )}
                    </div>
                  </HoverCardContent>
                </HoverCard>
              </div>
            )}
          </div>
        </div>

        {/* Actions Bar */}
        {!isTaskFinished && (
          <div className="px-6 py-4 border-b border-border bg-muted/30 flex items-center gap-3">
            {!isWorking && task.status !== "scheduled" && (
              <Button variant="outline" onClick={handleStartWork} className="border-energy-solar text-energy-solar hover:bg-energy-solar/10">
                <Play className="mr-2 h-4 w-4" />
                Start Work
              </Button>
            )}
            {isWorking && (
              <Button variant="outline" onClick={handleStopWork} className="border-energy-consumption text-energy-consumption hover:bg-energy-consumption/10">
                <Pause className="mr-2 h-4 w-4" />
                Pause Work
              </Button>
            )}
            {task.status !== "scheduled" && (
              <>
                <Button onClick={() => setShowCompletionModal(true)} className="bg-energy-solar hover:bg-energy-solar/90">
                  <CheckCircle2 className="mr-2 h-4 w-4" />
                  Complete Task
                </Button>
                <Button variant="secondary" onClick={() => setShowCloseModal(true)}>
                  <XCircle className="mr-2 h-4 w-4" />
                  Close Task
                </Button>
              </>
            )}
            {isWorking && (
              <div className="ml-auto flex items-center gap-2 text-energy-consumption">
                <span className="h-2 w-2 rounded-full bg-energy-consumption animate-pulse" />
                <span className="font-mono text-lg font-semibold">{formatDuration(totalTime)}</span>
              </div>
            )}
          </div>
        )}

        {/* Completion Summary */}
        {isTaskCompleted && task.completionDetails && (
          <div className="px-6 py-4 border-b border-border bg-energy-solar/5">
            <div className="flex items-start gap-3">
              <CheckCircle2 className="h-5 w-5 text-energy-solar mt-0.5" />
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <h3 className="font-medium text-energy-solar">Task Completed</h3>
                  <Badge variant="outline" className="text-xs">{task.completionDetails.laborHours}h labor</Badge>
                </div>
                <p className="text-sm text-muted-foreground">{task.completionDetails.resolution}</p>
                {task.completionDetails.rootCause && (
                  <p className="text-sm text-muted-foreground mt-1">
                    <span className="font-medium">Root Cause:</span> {task.completionDetails.rootCause}
                  </p>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Close Summary */}
        {isTaskClosed && task.closeDetails && (
          <div className="px-6 py-4 border-b border-border bg-muted/50">
            <div className="flex items-start gap-3">
              <XCircle className="h-5 w-5 text-muted-foreground mt-0.5" />
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <h3 className="font-medium">Task Closed</h3>
                  <Badge variant="secondary" className="text-xs">{getCloseReasonLabel(task.closeDetails.reason)}</Badge>
                </div>
                <p className="text-sm text-muted-foreground">{task.closeDetails.explanation}</p>
              </div>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3">
          {/* Left Column */}
          <div className="lg:col-span-2 border-r border-border">
            {/* Work Timer Section */}
            {!isTaskFinished && workSessions.length > 0 && (
              <div className="px-6 py-5 border-b border-border">
                <div className="flex items-center justify-between mb-3">
                  <h2 className="text-sm font-semibold">Work Time</h2>
                  <span className="text-2xl font-semibold font-mono">{(totalTime / (1000 * 60 * 60)).toFixed(1)}h</span>
                </div>
                <div className="space-y-1">
                  {workSessions.slice(0, 3).map((session) => (
                    <div key={session.id} className="flex items-center justify-between text-xs text-muted-foreground">
                      <span>
                        {session.startedAt.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}
                        {session.endedAt && ` - ${session.endedAt.toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit" })}`}
                      </span>
                      <span className="font-medium text-foreground">
                        {session.endedAt ? formatDuration(session.endedAt.getTime() - session.startedAt.getTime()) : "In progress"}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Alarm Info Section */}
            {task.relatedAlarm && (
              <div className="px-6 py-5 border-b border-border">
                <div className="flex items-center gap-2 mb-4">
                  <Bell className="h-4 w-4 text-muted-foreground" />
                  <h2 className="text-sm font-semibold">Source Alarm</h2>
                  <Badge className={cn(
                    task.relatedAlarm.severity === "critical" && "bg-energy-grid text-white",
                    task.relatedAlarm.severity === "high" && "bg-energy-consumption text-white",
                    task.relatedAlarm.severity === "medium" && "bg-energy-battery text-white",
                    task.relatedAlarm.severity === "low" && "bg-muted text-muted-foreground"
                  )}>
                    {task.relatedAlarm.severity.toUpperCase()}
                  </Badge>
                </div>

                <div className="p-3 rounded-lg bg-muted/50 mb-4">
                  <div className="flex items-start gap-3">
                    <AlertTriangle className="h-5 w-5 text-energy-grid shrink-0 mt-0.5" />
                    <div>
                      <p className="text-sm font-medium">{task.relatedAlarm.message}</p>
                      <p className="text-xs text-muted-foreground mt-1 font-mono">{task.relatedAlarm.code}</p>
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Source</span>
                    <span className="font-medium">{task.relatedAlarm.source}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Triggered</span>
                    <span>{formatDateTime(task.relatedAlarm.triggeredAt)}</span>
                  </div>
                  {task.relatedAlarm.value && (
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Value</span>
                      <span className="font-mono text-energy-grid font-medium">{task.relatedAlarm.value}</span>
                    </div>
                  )}
                  {task.relatedAlarm.threshold && (
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Threshold</span>
                      <span className="font-mono">{task.relatedAlarm.threshold}</span>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Timeline Section */}
            <div className="px-6 py-5 border-b border-border">
              <h2 className="text-sm font-semibold mb-4">Timeline</h2>
              {statusChanges.length > 0 ? (
                <div className="relative">
                  <div className="absolute left-3 top-3 bottom-3 w-px bg-border" />
                  <div className="space-y-4">
                    {statusChanges.map((change) => (
                      <div key={change.id} className="relative flex gap-4">
                        <div className={cn(
                          "relative z-10 h-6 w-6 rounded-full flex items-center justify-center ring-4 ring-background",
                          change.toStatus === "scheduled" && "bg-muted-foreground",
                          change.toStatus === "pending" && "bg-energy-battery",
                          change.toStatus === "in_progress" && "bg-energy-consumption",
                          change.toStatus === "completed" && "bg-energy-solar",
                          change.toStatus === "overdue" && "bg-energy-grid"
                        )}>
                          <div className="h-2 w-2 rounded-full bg-white" />
                        </div>
                        <div className="flex-1 pb-2">
                          <p className="text-sm font-medium">
                            {!change.fromStatus
                              ? `Task created with status "${getStatusLabel(change.toStatus)}"`
                              : `Status changed from "${getStatusLabel(change.fromStatus)}" to "${getStatusLabel(change.toStatus)}"`}
                          </p>
                          <div className="flex items-center gap-2 mt-1 text-xs text-muted-foreground">
                            <Avatar size="sm">
                              <AvatarFallback className="text-[8px]">{change.changedBy.initials}</AvatarFallback>
                            </Avatar>
                            <span>{change.changedBy.name}</span>
                            <span>|</span>
                            <span>{formatDateTime(change.changedAt)}</span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">No status changes recorded.</p>
              )}
            </div>

            {/* Notes Section */}
            <div className="px-6 py-5">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-sm font-semibold">Notes</h2>
                {notes.length > 0 && <span className="text-xs text-muted-foreground">{notes.length} notes</span>}
              </div>

              {!isTaskFinished && (
                <div className="mb-4">
                  <Textarea
                    placeholder="Add a note..."
                    value={noteContent}
                    onChange={(e) => setNoteContent(e.target.value)}
                    className="min-h-[80px] resize-none mb-2"
                  />
                  <div className="flex justify-end">
                    <Button size="sm" onClick={handleAddNote} disabled={!noteContent.trim()}>
                      Add Note
                    </Button>
                  </div>
                </div>
              )}

              {notes.length > 0 ? (
                <div className="space-y-3">
                  {notes.map((note) => (
                    <div key={note.id} className="flex gap-3 p-3 rounded-lg bg-muted/30">
                      <Avatar size="sm">
                        <AvatarFallback className="text-[10px]">{note.author.initials}</AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="text-sm font-medium">{note.author.name}</span>
                          <span className="text-xs text-muted-foreground">{formatDateTime(note.createdAt)}</span>
                        </div>
                        <p className="text-sm text-foreground whitespace-pre-wrap">{note.content}</p>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground text-center py-4">No notes yet.</p>
              )}
            </div>
          </div>

          {/* Right Column */}
          <div>
            {/* Checklist Section */}
            <div className="px-6 py-5 border-b border-border">
              <div className="flex items-center justify-between mb-3">
                <h2 className="text-sm font-semibold">Checklist</h2>
                <span className="text-xs text-muted-foreground">{completedChecklistCount} / {checklist.length}</span>
              </div>
              {checklist.length > 0 && <Progress value={checklistProgress} className="h-1.5 mb-4" />}

              {checklist.length > 0 ? (
                <div className="space-y-2">
                  {checklist.map((item) => (
                    <div
                      key={item.id}
                      className={cn(
                        "flex items-start gap-3 p-3 rounded-lg border transition-colors",
                        item.completed ? "bg-energy-solar/5 border-energy-solar/20" : "bg-background hover:bg-muted/50"
                      )}
                    >
                      <Checkbox
                        id={item.id}
                        checked={item.completed}
                        onCheckedChange={(checked) => handleChecklistToggle(item.id, checked as boolean)}
                        disabled={isTaskFinished}
                        className="mt-0.5"
                      />
                      <div className="flex-1 min-w-0">
                        <Label
                          htmlFor={item.id}
                          className={cn("text-sm cursor-pointer", item.completed && "line-through text-muted-foreground")}
                        >
                          {item.label}
                        </Label>
                        {item.completed && item.completedBy && (
                          <p className="text-xs text-muted-foreground mt-1">by {item.completedBy.name}</p>
                        )}
                      </div>
                      {item.completed && <Check className="h-4 w-4 text-energy-solar shrink-0" />}
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground text-center py-4">No checklist for this task.</p>
              )}
            </div>

            {/* Task Info Section */}
            <div className="px-6 py-5">
              <h2 className="text-sm font-semibold mb-4">Details</h2>
              <div className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Task ID</span>
                  <span className="font-mono">{task.id}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Created</span>
                  <span>{formatDate(task.assignedDate)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Assigned by</span>
                  <span>{task.assignedBy.name}</span>
                </div>
                {workSessions.length > 0 && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Total Work Time</span>
                    <span className="font-medium">{(totalPreviousTime / (1000 * 60 * 60)).toFixed(1)}h</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      <TaskCompletionModal
        open={showCompletionModal}
        onOpenChange={setShowCompletionModal}
        taskTitle={task.title}
        onComplete={handleCompleteTask}
      />

      <TaskCloseModal
        open={showCloseModal}
        onOpenChange={setShowCloseModal}
        taskTitle={task.title}
        onClose={handleCloseTask}
      />
    </div>
  );
}
