"use client";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

interface QuickAction {
  label: string;
  description: string;
  variant?: "default" | "outline" | "secondary";
}

const actions: QuickAction[] = [
  {
    label: "Charge BESS",
    description: "Start charging battery",
    variant: "default",
  },
  {
    label: "Curtail Solar",
    description: "Reduce solar output",
    variant: "outline",
  },
  {
    label: "Generate Report",
    description: "Export daily report",
    variant: "outline",
  },
  {
    label: "Settings",
    description: "System configuration",
    variant: "secondary",
  },
];

export function QuickActions() {
  return (
    <Card>
      <CardHeader className="pb-3">
        <CardTitle className="text-base">Quick Actions</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 gap-2">
          {actions.map((action) => (
            <Button
              key={action.label}
              variant={action.variant}
              className="h-auto flex-col items-start gap-1 py-3 px-3"
              onClick={() => console.log(`Action: ${action.label}`)}
            >
              <span className="font-medium">{action.label}</span>
              <span className="text-xs opacity-70 font-normal">
                {action.description}
              </span>
            </Button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
