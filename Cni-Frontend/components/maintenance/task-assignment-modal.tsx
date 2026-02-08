"use client";

import { useState, useEffect } from "react";
import { CalendarIcon } from "lucide-react";
import { format } from "date-fns";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import { cn } from "@/lib/utils";
import { users, assets } from "@/lib/maintenance/mock-data";
import type { TaskPriority } from "@/lib/maintenance/types";

interface TaskAssignmentModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit?: (data: TaskFormData) => void;
  defaultValues?: Partial<TaskFormData>;
}

export interface TaskFormData {
  title: string;
  description: string;
  assignedTo: string;
  priority: TaskPriority;
  dueDate: Date;
  relatedAsset?: string;
}

export function TaskAssignmentModal({
  open,
  onOpenChange,
  onSubmit,
  defaultValues,
}: TaskAssignmentModalProps) {
  const [title, setTitle] = useState(defaultValues?.title || "");
  const [description, setDescription] = useState(defaultValues?.description || "");
  const [assignedTo, setAssignedTo] = useState(defaultValues?.assignedTo || "");
  const [priority, setPriority] = useState<TaskPriority>(defaultValues?.priority || "medium");
  const [dueDate, setDueDate] = useState<Date | undefined>(defaultValues?.dueDate);
  const [relatedAsset, setRelatedAsset] = useState(defaultValues?.relatedAsset || "");

  useEffect(() => {
    if (open && defaultValues) {
      setTitle(defaultValues.title || "");
      setDescription(defaultValues.description || "");
      setAssignedTo(defaultValues.assignedTo || "");
      setPriority(defaultValues.priority || "medium");
      setDueDate(defaultValues.dueDate);
      setRelatedAsset(defaultValues.relatedAsset || "");
    }
  }, [open, defaultValues]);

  const handleSubmit = () => {
    if (!title || !assignedTo || !dueDate) return;

    onSubmit?.({
      title,
      description,
      assignedTo,
      priority,
      dueDate,
      relatedAsset: relatedAsset || undefined,
    });

    // Reset form
    setTitle("");
    setDescription("");
    setAssignedTo("");
    setPriority("medium");
    setDueDate(undefined);
    setRelatedAsset("");
    onOpenChange(false);
  };

  const isFormValid = title && assignedTo && dueDate;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>New Maintenance Task</DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Title */}
          <div className="grid gap-2">
            <Label htmlFor="title">
              Title <span className="text-destructive">*</span>
            </Label>
            <Input
              id="title"
              placeholder="Enter task title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
          </div>

          {/* Description */}
          <div className="grid gap-2">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              placeholder="Enter task description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="min-h-[80px]"
            />
          </div>

          {/* Assigned To & Priority */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label htmlFor="assignedTo">
                Assigned To <span className="text-destructive">*</span>
              </Label>
              <Select value={assignedTo} onValueChange={setAssignedTo}>
                <SelectTrigger id="assignedTo">
                  <SelectValue placeholder="Select user" />
                </SelectTrigger>
                <SelectContent>
                  {users.map((user) => (
                    <SelectItem key={user.id} value={user.id}>
                      {user.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="priority">Priority</Label>
              <Select
                value={priority}
                onValueChange={(v) => setPriority(v as TaskPriority)}
              >
                <SelectTrigger id="priority">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="low">Low</SelectItem>
                  <SelectItem value="medium">Medium</SelectItem>
                  <SelectItem value="high">High</SelectItem>
                  <SelectItem value="critical">Critical</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Due Date & Related Asset */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label>
                Due Date <span className="text-destructive">*</span>
              </Label>
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className={cn(
                      "justify-start text-left font-normal",
                      !dueDate && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {dueDate ? format(dueDate, "MMM d, yyyy") : "Select date"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar
                    mode="single"
                    selected={dueDate}
                    onSelect={setDueDate}
                    disabled={(date) => date < new Date()}
                  />
                </PopoverContent>
              </Popover>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="relatedAsset">Related Asset</Label>
              <Select value={relatedAsset} onValueChange={setRelatedAsset}>
                <SelectTrigger id="relatedAsset">
                  <SelectValue placeholder="Select asset" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">None</SelectItem>
                  {assets.map((asset) => (
                    <SelectItem key={asset.id} value={asset.id}>
                      {asset.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!isFormValid}>
            Create Task
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
