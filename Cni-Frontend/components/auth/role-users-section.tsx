"use client";

import { useState } from "react";
import { Plus, X, Search } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import type { User } from "@/lib/maintenance/types";

interface RoleUsersSectionProps {
  assignedUsers: User[];
  availableUsers: User[];
  onAddUser: (userId: string) => void;
  onRemoveUser: (userId: string) => void;
  disabled?: boolean;
}

export function RoleUsersSection({
  assignedUsers,
  availableUsers,
  onAddUser,
  onRemoveUser,
  disabled = false,
}: RoleUsersSectionProps) {
  const [popoverOpen, setPopoverOpen] = useState(false);

  const unassignedUsers = availableUsers.filter(
    (u) => !assignedUsers.some((au) => au.id === u.id)
  );

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">
          Assigned Users ({assignedUsers.length})
        </span>
        {!disabled && (
          <Popover open={popoverOpen} onOpenChange={setPopoverOpen}>
            <PopoverTrigger asChild>
              <Button variant="outline" size="sm" className="h-7 text-xs gap-1">
                <Plus className="h-3 w-3" />
                Add User
              </Button>
            </PopoverTrigger>
            <PopoverContent className="p-0 w-[250px]" align="end">
              <Command>
                <CommandInput placeholder="Search users..." />
                <CommandList>
                  <CommandEmpty>No users found.</CommandEmpty>
                  <CommandGroup>
                    {unassignedUsers.map((user) => (
                      <CommandItem
                        key={user.id}
                        value={user.name}
                        onSelect={() => {
                          onAddUser(user.id);
                          setPopoverOpen(false);
                        }}
                      >
                        <Avatar className="h-6 w-6 mr-2">
                          <AvatarFallback className="text-[10px]">
                            {user.initials}
                          </AvatarFallback>
                        </Avatar>
                        <div className="min-w-0">
                          <p className="text-sm truncate">{user.name}</p>
                          <p className="text-[10px] text-muted-foreground truncate">
                            {user.email}
                          </p>
                        </div>
                      </CommandItem>
                    ))}
                  </CommandGroup>
                </CommandList>
              </Command>
            </PopoverContent>
          </Popover>
        )}
      </div>

      {assignedUsers.length === 0 ? (
        <p className="text-xs text-muted-foreground py-2">No users assigned</p>
      ) : (
        <div className="space-y-1.5">
          {assignedUsers.map((user) => (
            <div
              key={user.id}
              className="flex items-center gap-2 py-1.5 px-2 rounded-md hover:bg-muted/50 group"
            >
              <Avatar className="h-7 w-7">
                <AvatarFallback className="text-[10px]">
                  {user.initials}
                </AvatarFallback>
              </Avatar>
              <div className="min-w-0 flex-1">
                <p className="text-sm font-medium truncate">{user.name}</p>
                <p className="text-[10px] text-muted-foreground truncate">
                  {user.email}
                </p>
              </div>
              {!disabled && (
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-6 w-6 opacity-0 group-hover:opacity-100 transition-opacity"
                  onClick={() => onRemoveUser(user.id)}
                >
                  <X className="h-3 w-3" />
                </Button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
