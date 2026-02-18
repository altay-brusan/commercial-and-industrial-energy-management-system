"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { ArrowUpDown, ArrowUp, ArrowDown } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { TaskStatusBadge, TaskPriorityBadge } from "./task-status-badge";
import { cn } from "@/lib/utils";
import type { MaintenanceTask, TaskStatus, TaskPriority } from "@/lib/maintenance/types";

interface MaintenanceTaskListProps {
  tasks: MaintenanceTask[];
}

type SortField = "title" | "status" | "priority" | "assignedTo" | "dueDate" | "asset";
type SortDirection = "asc" | "desc";

const statusOrder: Record<TaskStatus, number> = {
  overdue: 0,
  in_progress: 1,
  pending: 2,
  scheduled: 3,
  completed: 4,
  closed: 5,
};

const priorityOrder: Record<TaskPriority, number> = {
  critical: 0,
  high: 1,
  medium: 2,
  low: 3,
};

function formatDate(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}

interface SortableHeaderProps {
  label: string;
  field: SortField;
  currentField: SortField | null;
  direction: SortDirection;
  onSort: (field: SortField) => void;
  className?: string;
}

function SortableHeader({ label, field, currentField, direction, onSort, className }: SortableHeaderProps) {
  const isActive = currentField === field;

  return (
    <TableHead
      className={cn(
        "cursor-pointer select-none hover:bg-muted/80 transition-colors",
        "sticky top-0 z-20 bg-muted shadow-[0_1px_2px_rgba(0,0,0,0.05)] dark:shadow-[0_1px_2px_rgba(0,0,0,0.3)]",
        className
      )}
      onClick={() => onSort(field)}
    >
      <div className="flex items-center gap-1.5">
        {label}
        {isActive ? (
          direction === "asc" ? (
            <ArrowUp className="h-3.5 w-3.5 text-foreground" />
          ) : (
            <ArrowDown className="h-3.5 w-3.5 text-foreground" />
          )
        ) : (
          <ArrowUpDown className="h-3.5 w-3.5 text-muted-foreground/50" />
        )}
      </div>
    </TableHead>
  );
}

export function MaintenanceTaskList({ tasks }: MaintenanceTaskListProps) {
  const [sortField, setSortField] = useState<SortField | null>("dueDate");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sortedTasks = useMemo(() => {
    if (!sortField) return tasks;

    return [...tasks].sort((a, b) => {
      let comparison = 0;

      switch (sortField) {
        case "title":
          comparison = a.title.localeCompare(b.title);
          break;
        case "status":
          comparison = statusOrder[a.status] - statusOrder[b.status];
          break;
        case "priority":
          comparison = priorityOrder[a.priority] - priorityOrder[b.priority];
          break;
        case "assignedTo":
          comparison = a.assignedTo.name.localeCompare(b.assignedTo.name);
          break;
        case "dueDate":
          comparison = a.dueDate.getTime() - b.dueDate.getTime();
          break;
        case "asset":
          const assetA = a.relatedAsset?.name || "";
          const assetB = b.relatedAsset?.name || "";
          comparison = assetA.localeCompare(assetB);
          break;
      }

      return sortDirection === "asc" ? comparison : -comparison;
    });
  }, [tasks, sortField, sortDirection]);

  return (
    <Table>
      <TableHeader>
        <TableRow className="hover:bg-transparent">
          <SortableHeader
            label="Task"
            field="title"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="w-[300px] pl-6"
          />
          <SortableHeader
            label="Status"
            field="status"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Priority"
            field="priority"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Assigned To"
            field="assignedTo"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Due Date"
            field="dueDate"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
          />
          <SortableHeader
            label="Asset"
            field="asset"
            currentField={sortField}
            direction={sortDirection}
            onSort={handleSort}
            className="pr-6"
          />
        </TableRow>
      </TableHeader>
      <TableBody>
        {sortedTasks.map((task) => (
          <TableRow key={task.id} className="cursor-pointer">
            <TableCell className="py-3 pl-6">
              <Link
                href={`/maintenance/${task.id}`}
                className="block hover:underline"
              >
                <div className="font-medium">{task.title}</div>
                <div className="text-xs text-muted-foreground truncate max-w-[280px]">
                  {task.description}
                </div>
              </Link>
            </TableCell>
            <TableCell className="py-3">
              <TaskStatusBadge status={task.status} />
            </TableCell>
            <TableCell className="py-3">
              <TaskPriorityBadge priority={task.priority} />
            </TableCell>
            <TableCell className="py-3">
              <div className="flex items-center gap-2">
                <Avatar size="sm">
                  <AvatarImage src={task.assignedTo.avatar} />
                  <AvatarFallback className="text-[10px]">
                    {task.assignedTo.initials}
                  </AvatarFallback>
                </Avatar>
                <span className="text-sm">{task.assignedTo.name}</span>
              </div>
            </TableCell>
            <TableCell className="py-3">
              <span
                className={
                  task.status === "overdue"
                    ? "text-energy-grid font-medium"
                    : ""
                }
              >
                {formatDate(task.dueDate)}
              </span>
            </TableCell>
            <TableCell className="py-3 pr-6">
              {task.relatedAsset ? (
                <span className="text-sm text-muted-foreground">
                  {task.relatedAsset.name}
                </span>
              ) : (
                <span className="text-sm text-muted-foreground">-</span>
              )}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
