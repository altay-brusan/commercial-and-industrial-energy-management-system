"use client";

import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { AccessTabs } from "@/components/auth/access-tabs";
import { PermissionsOverview } from "@/components/auth/permissions-overview";
import { permissions, features } from "@/lib/auth/mock-data";

export default function PermissionsPage() {
  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="configuration" />
      <div className="flex flex-1 overflow-hidden">
        <ConfigurationSidebar />
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Header */}
          <div className="shrink-0 px-6 py-3 border-b border-border">
            <h1 className="text-lg font-semibold">Permissions</h1>
            <p className="text-sm text-muted-foreground mb-3">
              {permissions.length} permissions across {features.length} features
            </p>

            {/* Access Tabs */}
            <AccessTabs />
          </div>

          {/* Content */}
          <main className="flex-1 overflow-auto p-6">
            <div className="max-w-3xl">
              <PermissionsOverview />
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
