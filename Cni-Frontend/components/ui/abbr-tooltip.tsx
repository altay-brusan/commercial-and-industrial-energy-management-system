"use client";

import { HelpCircle } from "lucide-react";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

interface AbbrTooltipProps {
  label: string;
  title: string;
  description: string;
}

export function AbbrTooltip({ label, title, description }: AbbrTooltipProps) {
  return (
    <TooltipProvider delayDuration={200}>
      <span className="inline-flex items-center gap-1">
        {label}
        <Tooltip>
          <TooltipTrigger asChild>
            <HelpCircle className="h-3.5 w-3.5 text-muted-foreground/60 cursor-help shrink-0" />
          </TooltipTrigger>
          <TooltipContent side="top" className="max-w-[240px] text-xs font-normal">
            <span className="font-semibold">{title}</span>
            <br />
            {description}
          </TooltipContent>
        </Tooltip>
      </span>
    </TooltipProvider>
  );
}
