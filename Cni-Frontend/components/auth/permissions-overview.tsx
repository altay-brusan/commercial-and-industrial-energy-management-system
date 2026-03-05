"use client";

import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { features, permissions } from "@/lib/auth/mock-data";
import type { FeatureCategory } from "@/lib/auth/types";

const categoryColors: Record<FeatureCategory, string> = {
  Core: "bg-energy-grid/10 text-energy-grid border-energy-grid/30",
  Energy: "bg-energy-solar/10 text-energy-solar border-energy-solar/30",
  Intelligence: "bg-energy-consumption/10 text-energy-consumption border-energy-consumption/30",
  Management: "bg-energy-battery/10 text-energy-battery border-energy-battery/30",
};

const categories: FeatureCategory[] = ["Core", "Energy", "Intelligence", "Management"];

export function PermissionsOverview() {
  return (
    <div className="space-y-6">
      {categories.map((category) => {
        const categoryFeatures = features.filter((f) => f.category === category);
        return (
          <section key={category}>
            <div className="flex items-center gap-2 mb-3">
              <Badge
                variant="outline"
                className={cn("text-xs font-semibold", categoryColors[category])}
              >
                {category}
              </Badge>
              <span className="text-xs text-muted-foreground">
                {categoryFeatures.length} features
              </span>
            </div>

            <div className="grid gap-3">
              {categoryFeatures.map((feature) => {
                const featurePerms = permissions.filter(
                  (p) => p.featureId === feature.id
                );
                return (
                  <div
                    key={feature.id}
                    className="border border-border rounded-lg"
                  >
                    <div className="px-4 py-2.5 border-b border-border bg-muted/30">
                      <div className="flex items-center justify-between">
                        <h3 className="text-sm font-semibold">{feature.name}</h3>
                        {!feature.isActive && (
                          <Badge variant="outline" className="text-[10px] border-muted-foreground text-muted-foreground">
                            Inactive
                          </Badge>
                        )}
                      </div>
                      <p className="text-xs text-muted-foreground mt-0.5">
                        {feature.description}
                      </p>
                    </div>
                    <div className="divide-y divide-border">
                      {featurePerms.map((perm) => (
                        <div
                          key={perm.id}
                          className="px-4 py-2 flex items-start gap-3"
                        >
                          <code className="text-[11px] font-mono bg-muted px-1.5 py-0.5 rounded shrink-0 mt-0.5">
                            {perm.key}
                          </code>
                          <div className="min-w-0">
                            <p className="text-sm font-medium">{perm.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {perm.description}
                            </p>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>
          </section>
        );
      })}
    </div>
  );
}
