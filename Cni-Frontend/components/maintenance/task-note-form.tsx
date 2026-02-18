"use client";

import { useState } from "react";
import { Send } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";

interface TaskNoteFormProps {
  onSubmit: (content: string) => void;
}

export function TaskNoteForm({ onSubmit }: TaskNoteFormProps) {
  const [content, setContent] = useState("");

  const handleSubmit = () => {
    if (!content.trim()) return;
    onSubmit(content.trim());
    setContent("");
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && (e.metaKey || e.ctrlKey)) {
      e.preventDefault();
      handleSubmit();
    }
  };

  return (
    <div className="space-y-3">
      <Textarea
        placeholder="Add a note..."
        value={content}
        onChange={(e) => setContent(e.target.value)}
        onKeyDown={handleKeyDown}
        className="min-h-[80px] resize-none"
      />
      <div className="flex items-center justify-between">
        <p className="text-xs text-muted-foreground">
          Press Cmd+Enter to submit
        </p>
        <Button
          size="sm"
          onClick={handleSubmit}
          disabled={!content.trim()}
        >
          <Send className="mr-2 h-4 w-4" />
          Add Note
        </Button>
      </div>
    </div>
  );
}
