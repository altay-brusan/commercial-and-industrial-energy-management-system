"use client";

import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";

export default function Monitoring() {
  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />
        <main className="flex-1 overflow-y-auto p-6">
          {/* İçerik buraya eklenecek */}
        </main>
      </div>
    </div>
  );
}
