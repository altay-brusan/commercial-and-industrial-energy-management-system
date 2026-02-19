"use client";

import Link from "next/link";
import { useTheme } from "next-themes";
import { useState } from "react";
import { Bell, AlertTriangle, Sun, Moon, Wrench, RefreshCw, CheckCircle2, CalendarClock, ChevronsUpDown, Check } from "lucide-react";
import { RiAlarmWarningLine } from "react-icons/ri";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { getActiveAlarmCount, getUnreadNotificationCount, alarms, notifications } from "@/lib/alarms/mock-data";
import { useFacility } from "@/lib/facilities/facility-context";
import { format } from "date-fns";
import type { NotificationType } from "@/lib/alarms/types";

interface TopBarProps {
  activeTab?: string;
}

export function TopBar({ activeTab = "dashboard" }: TopBarProps) {
  const { theme, setTheme } = useTheme();
  const { selectedFacilityId, setSelectedFacilityId, selectedFacility, facilities } = useFacility();
  const [facilityOpen, setFacilityOpen] = useState(false);
  const activeAlarmCount = getActiveAlarmCount();
  const unreadNotificationCount = getUnreadNotificationCount();

  const recentAlarms = alarms
    .filter((a) => a.status === "active")
    .sort((a, b) => b.triggeredAt.getTime() - a.triggeredAt.getTime())
    .slice(0, 3);

  const recentNotifications = notifications
    .filter((n) => !n.read)
    .sort((a, b) => b.timestamp.getTime() - a.timestamp.getTime())
    .slice(0, 3);

  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background">
      <div className="flex h-14 items-center justify-between px-6">
        <div className="flex items-center gap-8">
          <Link href="/" className="text-xl tracking-tight" style={{ fontFamily: "'Cocon', sans-serif" }}>
            N2N Energy
          </Link>

          <Tabs value={activeTab} className="hidden md:block">
            <TabsList className="bg-transparent h-auto p-0 gap-1">
              <TabsTrigger
                value="dashboard"
                className="data-[state=active]:bg-secondary data-[state=active]:shadow-none rounded-md px-3 py-1.5 text-sm"
                asChild
              >
                <Link href="/">Dashboard</Link>
              </TabsTrigger>
              <TabsTrigger
                value="monitoring"
                className="data-[state=active]:bg-secondary data-[state=active]:shadow-none rounded-md px-3 py-1.5 text-sm"
                asChild
              >
                <Link href="/monitoring">Monitoring</Link>
              </TabsTrigger>
              <TabsTrigger
                value="alarms"
                className="data-[state=active]:bg-secondary data-[state=active]:shadow-none rounded-md px-3 py-1.5 text-sm"
                asChild
              >
                <Link href="/alarms">Alarms</Link>
              </TabsTrigger>
              <TabsTrigger
                value="analytics"
                className="data-[state=active]:bg-secondary data-[state=active]:shadow-none rounded-md px-3 py-1.5 text-sm"
                asChild
              >
                <Link href="/analytics">Analytics</Link>
              </TabsTrigger>
              <TabsTrigger
                value="financials"
                className="data-[state=active]:bg-secondary data-[state=active]:shadow-none rounded-md px-3 py-1.5 text-sm"
                asChild
              >
                <Link href="/financials">Financials</Link>
              </TabsTrigger>
            </TabsList>
          </Tabs>
        </div>

        <div className="flex items-center gap-4">
          {/* Facility Selector */}
          <Popover open={facilityOpen} onOpenChange={setFacilityOpen}>
            <PopoverTrigger asChild>
              <Button variant="outline" role="combobox" aria-expanded={facilityOpen} className="w-[260px] h-9 justify-between font-normal">
                <span className="truncate">{selectedFacility.name}</span>
                <ChevronsUpDown className="h-4 w-4 shrink-0 opacity-50" />
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[260px] p-0" align="start">
              <Command>
                {facilities.length > 4 && <CommandInput placeholder="Tesis ara..." />}
                <CommandList>
                  <CommandEmpty>Tesis bulunamadı.</CommandEmpty>
                  <CommandGroup>
                    {facilities.map((facility) => (
                      <CommandItem
                        key={facility.id}
                        value={facility.name}
                        onSelect={() => {
                          setSelectedFacilityId(facility.id);
                          setFacilityOpen(false);
                        }}
                      >
                        <div className="flex flex-col items-start">
                          <span>{facility.name}</span>
                          <span className="text-xs text-muted-foreground">{facility.location}, {facility.country}</span>
                        </div>
                        {selectedFacilityId === facility.id && (
                          <Check className="ml-auto h-4 w-4 shrink-0" />
                        )}
                      </CommandItem>
                    ))}
                  </CommandGroup>
                </CommandList>
              </Command>
            </PopoverContent>
          </Popover>

          {/* Maintenance Button */}
          <Button className="bg-sky-600 hover:bg-sky-700 text-white" asChild>
            <Link href="/maintenance">Maintenance</Link>
          </Button>

          {/* Theme Toggle */}
          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
          >
            <Sun className="h-5 w-5 rotate-0 scale-100 transition-transform dark:rotate-90 dark:scale-0" />
            <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-transform dark:rotate-0 dark:scale-100" />
            <span className="sr-only">Toggle theme</span>
          </Button>

          {/* Alarms */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="relative">
                <RiAlarmWarningLine className="h-5 w-5" />
                {activeAlarmCount > 0 && (
                  <Badge
                    variant="destructive"
                    className="absolute -top-1 -right-1 h-5 w-5 p-0 text-[10px] font-semibold animate-pulse"
                  >
                    {activeAlarmCount}
                  </Badge>
                )}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-80">
              <DropdownMenuLabel className="flex items-center justify-between">
                <span>Alarms</span>
                <Badge variant="destructive" className="text-[10px]">{activeAlarmCount} Active</Badge>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              {recentAlarms.map((alarm) => {
                const severityConfig = {
                  critical: { bg: "bg-energy-grid/10", color: "text-energy-grid" },
                  high: { bg: "bg-energy-consumption/10", color: "text-energy-consumption" },
                  medium: { bg: "bg-energy-battery/10", color: "text-energy-battery" },
                  low: { bg: "bg-muted", color: "text-muted-foreground" },
                }[alarm.severity];

                return (
                  <DropdownMenuItem key={alarm.id} className="flex items-start gap-3 py-3 cursor-pointer">
                    <div className={`mt-0.5 rounded-full ${severityConfig.bg} p-1.5`}>
                      <AlertTriangle className={`h-4 w-4 ${severityConfig.color}`} />
                    </div>
                    <div className="flex-1 space-y-1">
                      <p className="text-sm font-medium">{alarm.message}</p>
                      <p className="text-xs text-muted-foreground">{alarm.source}{alarm.value ? ` - ${alarm.value}` : ""}</p>
                      <p className="text-xs text-muted-foreground">{format(alarm.triggeredAt, "MMM d, HH:mm")}</p>
                    </div>
                  </DropdownMenuItem>
                );
              })}
              <DropdownMenuSeparator />
              <DropdownMenuItem asChild className="justify-center text-sm text-primary cursor-pointer">
                <Link href="/alarms">View all alarms</Link>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* Notifications */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="relative">
                <Bell className="h-5 w-5" />
                {unreadNotificationCount > 0 && (
                  <Badge
                    variant="secondary"
                    className="absolute -top-1 -right-1 h-5 w-5 p-0 text-[10px] font-semibold"
                  >
                    {unreadNotificationCount}
                  </Badge>
                )}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-80">
              <DropdownMenuLabel className="flex items-center justify-between">
                <span>Notifications</span>
                {unreadNotificationCount > 0 && (
                  <Badge variant="secondary" className="text-[10px]">{unreadNotificationCount} Unread</Badge>
                )}
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              {recentNotifications.map((notif) => {
                const iconConfig: Record<NotificationType, { icon: React.ElementType; bg: string; color: string }> = {
                  maintenance_completed: { icon: Wrench, bg: "bg-energy-solar/10", color: "text-energy-solar" },
                  system_update: { icon: RefreshCw, bg: "bg-energy-battery/10", color: "text-energy-battery" },
                  alarm_resolved: { icon: CheckCircle2, bg: "bg-energy-solar/10", color: "text-energy-solar" },
                  schedule_reminder: { icon: CalendarClock, bg: "bg-energy-battery/10", color: "text-energy-battery" },
                  system_alert: { icon: AlertTriangle, bg: "bg-energy-grid/10", color: "text-energy-grid" },
                };
                const config = iconConfig[notif.type];
                const NotifIcon = config.icon;

                return (
                  <DropdownMenuItem key={notif.id} className="flex items-start gap-3 py-3 cursor-pointer">
                    <div className={`mt-0.5 rounded-full ${config.bg} p-1.5`}>
                      <NotifIcon className={`h-4 w-4 ${config.color}`} />
                    </div>
                    <div className="flex-1 space-y-1">
                      <p className="text-sm font-medium">{notif.title}</p>
                      <p className="text-xs text-muted-foreground line-clamp-1">{notif.message}</p>
                      <p className="text-xs text-muted-foreground">{format(notif.timestamp, "MMM d, HH:mm")}</p>
                    </div>
                  </DropdownMenuItem>
                );
              })}
              {recentNotifications.length === 0 && (
                <div className="py-4 text-center text-sm text-muted-foreground">
                  No unread notifications
                </div>
              )}
              <DropdownMenuSeparator />
              <DropdownMenuItem asChild className="justify-center text-sm text-primary cursor-pointer">
                <Link href="/notifications">View all notifications</Link>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          {/* User Menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button className="flex items-center gap-3 outline-none">
                <Avatar className="h-8 w-8">
                <AvatarImage src="" alt="User" />
                <AvatarFallback className="text-xs">JD</AvatarFallback>
              </Avatar>
              <div className="hidden sm:flex flex-col items-start">
                <span className="text-sm font-medium">John Doe</span>
                <span className="text-xs text-muted-foreground">john@energy.com</span>
              </div>
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-48">
            <DropdownMenuLabel className="font-normal">
              <div className="flex flex-col gap-1">
                <p className="text-sm font-medium">John Doe</p>
                <p className="text-xs text-muted-foreground">john@energy.com</p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem>Profile</DropdownMenuItem>
            <DropdownMenuItem>Billing</DropdownMenuItem>
            <DropdownMenuItem>Notifications</DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem>Sign out</DropdownMenuItem>
          </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
  );
}
