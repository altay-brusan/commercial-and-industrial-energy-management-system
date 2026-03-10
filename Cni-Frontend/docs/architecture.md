# Architecture

## Application Structure

```
app/                          # Pages (Next.js App Router)
├── layout.tsx                # Root layout: ThemeProvider → AuthProvider → FacilityProvider
├── globals.css               # Tailwind config, CSS variables, color system
├── page.tsx                  # Dashboard (home)
├── login/                    # Login page
├── forgot-password/          # Password recovery
├── reset-password/           # Password reset
├── alarms/                   # Alarm monitoring
├── maintenance/              # Maintenance tasks
│   └── [id]/                 # Maintenance task detail
├── monitoring/               # Monitoring hub
│   ├── battery-storage/      # BESS monitoring
│   ├── pcs/                  # PCS monitoring
│   ├── solar-inverters/      # Solar inverter monitoring
│   ├── wind-turbines/        # Wind turbine monitoring
│   ├── consumption/          # Energy consumption
│   └── grid-loads/           # Grid & loads
├── financials/               # Financial data & day-ahead prices
├── notifications/            # System notifications
├── dashboard-2/              # Alternative dashboard layout
├── dashboard-3/              # Third dashboard variant
└── configuration/            # System configuration
    ├── users/                # User management
    ├── roles/                # Role management
    ├── permissions/           # Permission overview
    ├── capacity-limits/      # Capacity limit settings
    └── modes/                # Operation modes

components/
├── ui/                       # shadcn/ui base components (57 files)
├── layout/                   # TopBar (shared navigation)
├── dashboard/                # Dashboard charts, stats, power flow
├── dashboard-2/              # Alternative dashboard components
├── alarms/                   # Alarm table, filters, detail panel
├── maintenance/              # Task list, modals (assign/complete/close)
├── monitoring/               # Monitoring sidebar tree
├── bess/                     # Battery: hierarchy, overview, alarms, unit/cell detail
├── pcs/                      # Power conversion system components
├── solar-inverters/          # Inverter table, detail, fault/curve tabs
├── wind-turbines/            # Wind turbine components
├── consumption/              # Energy consumption tracking
├── grid-loads/               # Grid load monitoring
├── financials/               # Financial tracking & charts
├── notifications/            # Notification list, filters
├── scada/                    # SCADA monitoring
├── control/                  # Control interface
├── auth/                     # User/role/permission management
└── configuration/            # Configuration sidebar

lib/
├── utils.ts                  # cn() helper (clsx + tailwind-merge)
├── auth/                     # Auth types, services, context, mock data
├── facilities/               # Facility types, context, service
├── alarms/                   # Alarm types & mock data
├── maintenance/              # Maintenance types & mock data (shared)
├── bess/                     # Battery storage types & mock data
├── pcs/                      # PCS types & mock data
├── solar-inverters/          # Solar inverter types & mock data
├── wind-turbines/            # Wind turbine types & mock data
├── consumption/              # Consumption types & mock data
├── grid-loads/               # Grid loads types & mock data
├── financials/               # Financial types & mock data
├── dashboard/                # Dashboard types & mock data
├── dashboard-2/              # Dashboard-2 types & mock data
├── control/                  # Control types & mock data
└── scada/                    # SCADA types & mock data

hooks/
├── use-mobile.ts             # useIsMobile() — 768px breakpoint
└── use-scada-simulation.ts   # SCADA real-time simulation
```

## Provider Hierarchy

```
<html>
  <ThemeProvider>              ← Dark/light mode (next-themes)
    <AuthProvider>             ← JWT auth state, login/logout, route protection
      <FacilityProvider>       ← Selected facility context, facility list
        {children}
        <Toaster />            ← Toast notifications (sonner)
      </FacilityProvider>
    </AuthProvider>
  </ThemeProvider>
</html>
```

## Page Layout Pattern

All pages are `"use client"` and follow a consistent structure:

```
TopBar (sticky header)
├── Facility selector (dropdown)
├── Navigation tabs (dashboard, monitoring, alarms, etc.)
├── Alarm & notification badges
└── User menu (profile, logout)

Main Area
├── Sidebar (optional, left: config sidebar, monitoring tree)
├── Content
│   ├── Page header (sticky: title, stats, search, filters)
│   ├── Scrollable content (table, list, charts)
│   └── Footer (sticky: item counts)
└── Detail Panel (optional, 420px right aside)
```

## Key Patterns

### Sortable Tables
- `useMemo` for filtered/sorted data
- `SortableHeader` sub-component with ArrowUpDown/ArrowUp/ArrowDown icons

### Filter Panels
- `Popover` with multi-select `Checkbox` groups
- `Select` for single-select
- Integrated search inputs

### Badges
- Severity/priority: solid fill colors (`bg-severity-critical`, etc.)
- Status: outlined borders (`border-status-operational`, etc.)

### Detail Panels
- 420px wide right `<aside>`
- Sticky header + scrollable content
- Edit/delete action buttons

### Modals
- Controlled via `useState<boolean>`
- `Dialog`/`AlertDialog` from shadcn/ui
- Examples: `TaskAssignmentModal`, `TaskCompletionModal`, `UserFormModal`

### Data Fetching Pattern (API + Mock Fallback)
```tsx
const [data, setData] = useState([]);
const [isLoading, setIsLoading] = useState(true);

useEffect(() => {
  async function fetch() {
    try {
      const response = await apiService.getData();
      setData(response);
    } catch {
      setData(mockData); // fallback
    } finally {
      setIsLoading(false);
    }
  }
  fetch();
}, []);
```
