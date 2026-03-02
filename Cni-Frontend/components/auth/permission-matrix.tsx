"use client";

import { useState } from "react";
import { ChevronDown, ChevronRight } from "lucide-react";
import { Checkbox } from "@/components/ui/checkbox";
import { cn } from "@/lib/utils";
import { features, permissions } from "@/lib/auth/mock-data";
import type { FeatureCategory } from "@/lib/auth/types";

interface PermissionMatrixProps {
  selectedPermissionIds: string[];
  onPermissionChange: (permissionIds: string[]) => void;
  disabled?: boolean;
}

const categoryColors: Record<FeatureCategory, string> = {
  Core: "bg-energy-grid/10 text-energy-grid border-energy-grid/30",
  Energy: "bg-energy-solar/10 text-energy-solar border-energy-solar/30",
  Intelligence: "bg-energy-consumption/10 text-energy-consumption border-energy-consumption/30",
  Management: "bg-energy-battery/10 text-energy-battery border-energy-battery/30",
};

const categories: FeatureCategory[] = ["Core", "Energy", "Intelligence", "Management"];

export function PermissionMatrix({
  selectedPermissionIds,
  onPermissionChange,
  disabled = false,
}: PermissionMatrixProps) {
  const [expandedCategories, setExpandedCategories] = useState<Set<FeatureCategory>>(
    new Set(categories)
  );

  const toggleCategory = (category: FeatureCategory) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(category)) next.delete(category);
      else next.add(category);
      return next;
    });
  };

  const getCategoryPermissions = (category: FeatureCategory) => {
    const categoryFeatureIds = features
      .filter((f) => f.category === category)
      .map((f) => f.id);
    return permissions.filter((p) => categoryFeatureIds.includes(p.featureId));
  };

  const isCategoryAllSelected = (category: FeatureCategory) => {
    const catPerms = getCategoryPermissions(category);
    return catPerms.length > 0 && catPerms.every((p) => selectedPermissionIds.includes(p.id));
  };

  const isCategorySomeSelected = (category: FeatureCategory) => {
    const catPerms = getCategoryPermissions(category);
    return catPerms.some((p) => selectedPermissionIds.includes(p.id)) && !isCategoryAllSelected(category);
  };

  const toggleCategoryPermissions = (category: FeatureCategory) => {
    if (disabled) return;
    const catPerms = getCategoryPermissions(category);
    const catPermIds = catPerms.map((p) => p.id);
    if (isCategoryAllSelected(category)) {
      onPermissionChange(selectedPermissionIds.filter((id) => !catPermIds.includes(id)));
    } else {
      const newIds = new Set([...selectedPermissionIds, ...catPermIds]);
      onPermissionChange(Array.from(newIds));
    }
  };

  const togglePermission = (permId: string) => {
    if (disabled) return;
    if (selectedPermissionIds.includes(permId)) {
      onPermissionChange(selectedPermissionIds.filter((id) => id !== permId));
    } else {
      onPermissionChange([...selectedPermissionIds, permId]);
    }
  };

  const isAllSelected = permissions.every((p) => selectedPermissionIds.includes(p.id));
  const isSomeSelected = permissions.some((p) => selectedPermissionIds.includes(p.id)) && !isAllSelected;

  const toggleAll = () => {
    if (disabled) return;
    if (isAllSelected) {
      onPermissionChange([]);
    } else {
      onPermissionChange(permissions.map((p) => p.id));
    }
  };

  return (
    <div className="space-y-3">
      {/* Master Select All */}
      <div className="flex items-center gap-2 pb-2 border-b border-border">
        <Checkbox
          checked={isSomeSelected ? "indeterminate" : isAllSelected}
          onCheckedChange={toggleAll}
          disabled={disabled}
        />
        <span className="text-sm font-medium">
          Select All ({selectedPermissionIds.length}/{permissions.length})
        </span>
      </div>

      {/* Category sections */}
      {categories.map((category) => {
        const categoryFeatures = features.filter((f) => f.category === category);
        const isExpanded = expandedCategories.has(category);

        return (
          <div key={category} className="border border-border rounded-lg overflow-hidden">
            {/* Category header */}
            <div
              className={cn(
                "flex items-center gap-2 px-3 py-2 cursor-pointer select-none border-b border-border",
                categoryColors[category]
              )}
              onClick={() => toggleCategory(category)}
            >
              {isExpanded ? (
                <ChevronDown className="h-4 w-4 shrink-0" />
              ) : (
                <ChevronRight className="h-4 w-4 shrink-0" />
              )}
              <Checkbox
                checked={isCategorySomeSelected(category) ? "indeterminate" : isCategoryAllSelected(category)}
                onCheckedChange={() => toggleCategoryPermissions(category)}
                disabled={disabled}
                onClick={(e) => e.stopPropagation()}
              />
              <span className="text-sm font-semibold">{category}</span>
              <span className="text-xs opacity-70 ml-auto">
                {getCategoryPermissions(category).filter((p) => selectedPermissionIds.includes(p.id)).length}/
                {getCategoryPermissions(category).length}
              </span>
            </div>

            {/* Feature rows */}
            {isExpanded && (
              <div className="divide-y divide-border">
                {categoryFeatures.map((feature) => {
                  const featurePerms = permissions.filter((p) => p.featureId === feature.id);
                  return (
                    <div key={feature.id} className="px-3 py-2">
                      <div className="text-xs font-medium text-muted-foreground mb-1.5 pl-6">
                        {feature.name}
                      </div>
                      <div className="flex flex-wrap gap-x-6 gap-y-1.5 pl-6">
                        {featurePerms.map((perm) => (
                          <label
                            key={perm.id}
                            className={cn(
                              "flex items-center gap-2 text-sm",
                              disabled && "opacity-50 cursor-not-allowed"
                            )}
                          >
                            <Checkbox
                              checked={selectedPermissionIds.includes(perm.id)}
                              onCheckedChange={() => togglePermission(perm.id)}
                              disabled={disabled}
                            />
                            <span>{perm.name}</span>
                          </label>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
