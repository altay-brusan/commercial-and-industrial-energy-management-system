"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";
import { TaskNoteForm } from "./task-note-form";
import type { TaskNote } from "@/lib/maintenance/types";

interface TaskNotesProps {
  notes: TaskNote[];
  onAddNote?: (content: string) => void;
}

function formatDateTime(date: Date): string {
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function TaskNotes({ notes, onAddNote }: TaskNotesProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">
          Notes
          {notes.length > 0 && (
            <span className="ml-2 text-sm font-normal text-muted-foreground">
              ({notes.length})
            </span>
          )}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Notes List */}
        {notes.length > 0 ? (
          <ScrollArea className="h-[280px] pr-4">
            <div className="space-y-4">
              {notes.map((note) => (
                <div
                  key={note.id}
                  className="flex gap-3 p-3 rounded-lg bg-muted/50"
                >
                  <Avatar size="sm">
                    <AvatarImage src={note.author.avatar} />
                    <AvatarFallback className="text-[10px]">
                      {note.author.initials}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-sm font-medium">
                        {note.author.name}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {formatDateTime(note.createdAt)}
                      </span>
                    </div>
                    <p className="text-sm text-foreground whitespace-pre-wrap">
                      {note.content}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </ScrollArea>
        ) : (
          <div className="py-8 text-center text-sm text-muted-foreground">
            No notes yet. Add the first note below.
          </div>
        )}

        {/* Add Note Form */}
        {onAddNote && <TaskNoteForm onSubmit={onAddNote} />}
      </CardContent>
    </Card>
  );
}
