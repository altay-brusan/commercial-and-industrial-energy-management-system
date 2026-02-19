"use client";

import { useState, useMemo } from "react";
import { Plus } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { Button } from "@/components/ui/button";
import { MaintenanceTaskList } from "@/components/maintenance/maintenance-task-list";
import { MaintenanceFilterPanel } from "@/components/maintenance/maintenance-filter-panel";
import { MaintenanceSidebar } from "@/components/maintenance/maintenance-sidebar";
import { TaskAssignmentModal } from "@/components/maintenance/task-assignment-modal";
import { maintenanceTasks } from "@/lib/maintenance/mock-data";
import type { TaskFilters } from "@/lib/maintenance/types";

export default function MaintenancePage() {
  const [filters, setFilters] = useState<TaskFilters>({});
  const [isModalOpen, setIsModalOpen] = useState(false);

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
                  onFiltersChange={setFilters}
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
              <MaintenanceTaskList tasks={filteredTasks} />
            ) : (
              <div className="py-12 text-center text-muted-foreground">
                No tasks found matching your filters.
              </div>
            )}

            {/* Task count footer */}
            <div className="px-6 py-3 border-t text-xs text-muted-foreground">
              Showing {filteredTasks.length} of {maintenanceTasks.length} tasks
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
