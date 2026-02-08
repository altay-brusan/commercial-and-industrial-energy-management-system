# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Energy management dashboard UI kit built with Next.js 16, React 19, Tailwind CSS 4, and shadcn/ui (new-york style). Provides monitoring, alarm management, maintenance tracking, and notification interfaces for a DERMS (Distributed Energy Resource Management System).

## Commands

```bash
npm run dev      # Start development server (http://localhost:3000)
npm run build    # Production build
npm run lint     # Run ESLint
```

## Architecture

- **Next.js 16 App Router** with React 19 and compiler support
- **TypeScript** in strict mode with `@/*` path alias mapping to project root
- **Tailwind CSS 4** using `@import "tailwindcss"` syntax and `@theme inline` for custom properties
- **shadcn/ui** (new-york style) — 58+ base UI components in `components/ui/`
- **Icons**: lucide-react (primary), react-icons (supplementary)
- **Charts**: recharts for data visualization
- **Forms**: react-hook-form + zod validation
- **Dates**: date-fns

### Code Organization

```
app/                    # Pages (App Router)
components/
  ui/                   # shadcn/ui base components
  layout/               # TopBar (shared across all pages)
  dashboard/            # Dashboard charts, stats, power flow
  alarms/               # Alarm table, filters, detail panel
  maintenance/          # Task list, assignment/completion modals
  monitoring/           # Monitoring sidebar
  notifications/        # Notification list, filters
lib/
  utils.ts              # cn() helper (clsx + tailwind-merge)
  alarms/types.ts       # Alarm, SystemNotification types
  alarms/mock-data.ts   # Mock alarms & notifications (imports maintenance data)
  maintenance/types.ts  # MaintenanceTask, User, Asset, AlarmSeverity types
  maintenance/mock-data.ts  # Mock users, assets, tasks (shared across domains)
hooks/
  use-mobile.ts         # useIsMobile() — 768px breakpoint detection
```

### Color System

Domain-specific colors defined as CSS variables in `app/globals.css` with light/dark mode support:

| Variable                | Semantic Use               | Tailwind Class              |
|------------------------|----------------------------|-----------------------------|
| `--energy-solar`       | Green — solar, operational | `bg-energy-solar`           |
| `--energy-grid`        | Red — critical severity    | `bg-energy-grid`            |
| `--energy-battery`     | Purple — medium severity   | `bg-energy-battery`         |
| `--energy-consumption` | Orange — high severity     | `bg-energy-consumption`     |
| `--status-operational` | Alias for energy-solar     | `bg-status-operational`     |
| `--status-warning`     | Alias for energy-consumption | `bg-status-warning`       |
| `--status-critical`    | Alias for energy-grid      | `bg-status-critical`        |

Severity-to-color mapping: `critical → energy-grid`, `high → energy-consumption`, `medium → energy-battery`, `low → muted`.

### Page Layout Pattern

All pages are `"use client"` and follow the same structure:

```
TopBar (sticky header with nav tabs, alarm/notification dropdowns, user menu)
├─ Main content (flex-1, overflow handling)
│  ├─ Page header (sticky: title, stats, search/filter controls)
│  ├─ Scrollable content (table, list, or charts)
│  └─ Footer (sticky: item counts)
├─ Sidebar (optional, left or right: calendar, monitoring tree)
└─ Detail Panel (optional, 420px right aside: selected item details + actions)
```

### Key Patterns

- **Sortable tables**: `useMemo` for filtered/sorted data + `SortableHeader` sub-component using ArrowUpDown/ArrowUp/ArrowDown icons from lucide-react
- **Filter panels**: Popover with multi-select checkboxes, Select for single-select, integrated search
- **Badges**: Severity/priority use solid fill colors; status uses outlined borders
- **Detail panels**: 420px wide right aside with sticky header and scrollable content
- **Modals**: TaskAssignmentModal, TaskCompletionModal, TaskCloseModal — controlled via `useState`
- **Fonts**: Geist Sans/Mono via `next/font/google`, custom Cocon font (logo only) from `app/fonts/` accessed via `.font-logo` utility class

### Mock Data

All mock data is in `lib/`. Types and data are shared:
- `lib/maintenance/mock-data.ts` exports `users`, `assets`, `maintenanceTasks` (used by multiple domains)
- `lib/alarms/mock-data.ts` exports `alarms`, `notifications` and helper functions like `getActiveAlarmCount()`
- `AlarmSeverity` type is defined in `lib/maintenance/types.ts` and shared across alarms and maintenance
