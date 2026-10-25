"use client";

import { Check } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";
import type { ChecklistItem } from "@/lib/maintenance/types";

interface TaskChecklistProps {
  items: ChecklistItem[];
  onItemToggle: (itemId: string, completed: boolean) => void;
  disabled?: boolean;
}

export function TaskChecklist({ items, onItemToggle, disabled }: TaskChecklistProps) {
  const completedCount = items.filter((item) => item.completed).length;
  const progress = items.length > 0 ? (completedCount / items.length) * 100 : 0;

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">Checklist</CardTitle>
          <span className="text-sm text-muted-foreground">
            {completedCount} / {items.length}
          </span>
        </div>
        <Progress value={progress} className="h-2" />
      </CardHeader>
      <CardContent>
        {items.length > 0 ? (
          <div className="space-y-3">
            {items.map((item) => (
              <div
                key={item.id}
                className={cn(
                  "flex items-start gap-3 p-3 rounded-lg border transition-colors",
                  item.completed
                    ? "bg-energy-solar/5 border-energy-solar/20"
                    : "bg-background hover:bg-muted/50"
                )}
              >
                <Checkbox
                  id={item.id}
                  checked={item.completed}
                  onCheckedChange={(checked) =>
                    onItemToggle(item.id, checked as boolean)
                  }
                  disabled={disabled}
                  className="mt-0.5"
                />
                <div className="flex-1 min-w-0">
                  <Label
                    htmlFor={item.id}
                    className={cn(
                      "text-sm font-medium cursor-pointer",
                      item.completed && "line-through text-muted-foreground"
                    )}
                  >
                    {item.label}
                  </Label>
                  {item.completed && item.completedBy && (
                    <p className="text-xs text-muted-foreground mt-1">
                      Completed by {item.completedBy.name}
                    </p>
                  )}
                </div>
                {item.completed && (
                  <Check className="h-4 w-4 text-energy-solar shrink-0" />
                )}
              </div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground text-center py-4">
            No checklist items for this task.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
