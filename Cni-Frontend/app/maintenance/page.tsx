"use client";

import { useState, useMemo } from "react";
import { Plus, ChevronLeft, ChevronRight } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { MaintenanceTaskList } from "@/components/maintenance/maintenance-task-list";
import { MaintenanceFilterPanel } from "@/components/maintenance/maintenance-filter-panel";
import { MaintenanceSidebar } from "@/components/maintenance/maintenance-sidebar";
import { TaskAssignmentModal } from "@/components/maintenance/task-assignment-modal";
import { maintenanceTasks } from "@/lib/maintenance/mock-data";
import type { TaskFilters } from "@/lib/maintenance/types";

const PAGE_SIZE_OPTIONS = [20, 40, 60] as const;

export default function MaintenancePage() {
  const [filters, setFilters] = useState<TaskFilters>({});
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState<number>(20);

  // Filter tasks based on current filters
  const filteredTasks = useMemo(() => {
    return maintenanceTasks.filter((task) => {
      // Status filter
      if (filters.status && filters.status.length > 0) {
        if (!filters.status.includes(task.status)) return false;
      }

      // Priority filter
      if (filters.priority && filters.priority.length > 0) {
        if (!filters.priority.includes(task.priority)) return false;
      }

      // Assigned to filter
      if (filters.assignedTo) {
        if (task.assignedTo.id !== filters.assignedTo) return false;
      }

      return true;
    });
  }, [filters]);

  // Reset to page 1 when filters change
  const totalPages = Math.ceil(filteredTasks.length / pageSize);
  const safeCurrentPage = Math.min(currentPage, Math.max(totalPages, 1));

  const paginatedTasks = useMemo(() => {
    const start = (safeCurrentPage - 1) * pageSize;
    return filteredTasks.slice(start, start + pageSize);
  }, [filteredTasks, safeCurrentPage, pageSize]);

  const handlePageSizeChange = (value: string) => {
    setPageSize(Number(value));
    setCurrentPage(1);
  };

  const startItem = filteredTasks.length === 0 ? 0 : (safeCurrentPage - 1) * pageSize + 1;
  const endItem = Math.min(safeCurrentPage * pageSize, filteredTasks.length);

  const handleCreateTask = (data: unknown) => {
    console.log("Creating task:", data);
    // In a real app, this would send the data to an API
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="maintenance" />

      <div className="flex flex-1 overflow-hidden">
        {/* Main Content */}
        <main className="flex-1 flex flex-col overflow-hidden">
          {/* Header - Sticky */}
          <div className="shrink-0 px-6 py-5 border-b border-border bg-background z-20">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-xl font-semibold">Maintenance Tasks</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Manage and track maintenance activities
                </p>
              </div>
              <div className="flex items-center gap-3">
                <MaintenanceFilterPanel
                  filters={filters}
                  onFiltersChange={(f) => {
                    setFilters(f);
                    setCurrentPage(1);
                  }}
                />
                <Button onClick={() => setIsModalOpen(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  New Task
                </Button>
              </div>
            </div>
          </div>

          {/* Task List - Scrollable */}
          <div className="flex-1 overflow-y-auto">
            {filteredTasks.length > 0 ? (
              <MaintenanceTaskList tasks={paginatedTasks} />
            ) : (
              <div className="py-12 text-center text-muted-foreground">
                No tasks found matching your filters.
              </div>
            )}
          </div>

          {/* Pagination Footer - Sticky */}
          <div className="shrink-0 px-6 py-3 border-t border-border bg-background flex items-center justify-between">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <span>Rows per page</span>
              <Select
                value={String(pageSize)}
                onValueChange={handlePageSizeChange}
              >
                <SelectTrigger className="h-8 w-[70px]">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {PAGE_SIZE_OPTIONS.map((size) => (
                    <SelectItem key={size} value={String(size)}>
                      {size}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center gap-4">
              <span className="text-sm text-muted-foreground">
                {startItem}-{endItem} of {filteredTasks.length}
              </span>
              <div className="flex items-center gap-1">
                <Button
                  variant="outline"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={safeCurrentPage <= 1}
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  className="h-8 w-8"
                  onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                  disabled={safeCurrentPage >= totalPages}
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>
        </main>

        {/* Sidebar */}
        <MaintenanceSidebar />
      </div>

      {/* Task Assignment Modal */}
      <TaskAssignmentModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onSubmit={handleCreateTask}
      />
    </div>
  );
}
