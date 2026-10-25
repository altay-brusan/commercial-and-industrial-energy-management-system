"use client";

import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
import { TopBar } from "@/components/layout/top-bar";
import { ConfigurationSidebar } from "@/components/configuration/configuration-sidebar";
import { AccessTabs } from "@/components/auth/access-tabs";
import { PermissionsOverview } from "@/components/auth/permissions-overview";
import {
  permissions as mockPermissions,
  features as mockFeatures,
} from "@/lib/auth/mock-data";
import * as permissionService from "@/lib/auth/permission-service";
import type { PermissionGroup } from "@/lib/auth/types";

export default function PermissionsPage() {
  const [isLoading, setIsLoading] = useState(true);
  const [permissionGroups, setPermissionGroups] = useState<PermissionGroup[]>([]);
  const [totalPermissions, setTotalPermissions] = useState(0);
  const [totalFeatures, setTotalFeatures] = useState(0);

  useEffect(() => {
    async function fetchPermissions() {
      setIsLoading(true);
      try {
        const groups = await permissionService.getPermissions();
        setPermissionGroups(groups);
        setTotalFeatures(groups.length);
        setTotalPermissions(groups.reduce((acc, g) => acc + g.permissions.length, 0));
      } catch {
        // Fallback to mock data
        const groups = mockFeatures.map((feature) => ({
          feature,
          permissions: mockPermissions.filter((p) => p.featureId === feature.id),
        }));
        setPermissionGroups(groups);
        setTotalFeatures(mockFeatures.length);
        setTotalPermissions(mockPermissions.length);
      } finally {
        setIsLoading(false);
      }
    }

    fetchPermissions();
  }, []);

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
              {totalPermissions} permissions across {totalFeatures} features
            </p>

            {/* Access Tabs */}
            <AccessTabs />
          </div>

          {/* Content */}
          <main className="flex-1 overflow-auto p-6">
            <div className="max-w-3xl">
              {isLoading ? (
                <div className="flex items-center justify-center py-12">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : (
                <PermissionsOverview permissionGroups={permissionGroups} />
              )}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
