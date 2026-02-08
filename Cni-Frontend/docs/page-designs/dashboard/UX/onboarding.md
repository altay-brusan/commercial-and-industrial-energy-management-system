# Onboarding - EMS Dashboard

İlk kullanıcı deneyimi ve dashboard tanıtımı.

---

## İlk Giriş Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                    Welcome to EMS Dashboard                     │
│                                                                 │
│          Your energy management system at a glance              │
│                                                                 │
│                                                                 │
│                     [Take a Quick Tour]                         │
│                                                                 │
│                     [Skip, I'll explore]                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Tour Steps

### Step 1: System Health

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [1/6] [X]  │
│  ┌─────────────────────────────────┐                                 │
│  │     SYSTEM HEALTH STATUS        │  ←───┐                          │
│  │     ████████████████████        │      │                          │
│  │     All Systems Operational     │      │                          │
│  └─────────────────────────────────┘      │                          │
│                                           │                          │
│  ┌────────────────────────────────────────┴─────────────────────┐   │
│  │                                                               │   │
│  │  System Health Overview                                       │   │
│  │  ─────────────────────                                        │   │
│  │                                                               │   │
│  │  This widget shows the overall health of your energy system. │   │
│  │  Green means all systems are operating normally.              │   │
│  │                                                               │   │
│  │  Click on any subsystem for more details.                     │   │
│  │                                                               │   │
│  │                              [Back] [Next: Alarms →]          │   │
│  │                                                               │   │
│  └───────────────────────────────────────────────────────────────┘   │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Step 2: Active Alarms

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [2/6] [X]  │
│                                                                       │
│                        ┌─────────────────────────────────┐           │
│                        │       ACTIVE ALARMS             │  ←───┐    │
│                        │  2 Critical  5 Warning          │      │    │
│                        │  [View All]                     │      │    │
│                        └─────────────────────────────────┘      │    │
│                                                                  │    │
│  ┌───────────────────────────────────────────────────────────────┴─┐ │
│  │                                                                  │ │
│  │  Alarm Notifications                                             │ │
│  │  ───────────────────                                             │ │
│  │                                                                  │ │
│  │  Active alarms are shown here, sorted by priority.               │ │
│  │  Critical alarms (red) require immediate attention.              │ │
│  │                                                                  │ │
│  │  Click [ACK] to acknowledge an alarm.                            │ │
│  │  Click the arrow to see alarm details.                           │ │
│  │                                                                  │ │
│  │                              [← Back] [Next: Power Flow →]       │ │
│  │                                                                  │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Step 3: Power Flow

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [3/6] [X]  │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │                      POWER FLOW DIAGRAM                         │  │
│  │                                                                  │  │
│  │     Solar ──┐                                                    │  │
│  │      125 kW │     ┌─────────┐      ┌─────────┐                  │  │
│  │             ├────→│  BUS    │─────→│  LOAD   │                  │  │
│  │     BESS ──┤     └─────────┘      └─────────┘                  │  │
│  │                                                                  │  │
│  └────────────────────────────────────────────────────────────────┘  │
│          ↑                                                            │
│          │                                                            │
│  ┌───────┴────────────────────────────────────────────────────────┐  │
│  │                                                                 │  │
│  │  Live Power Flow                                                │  │
│  │  ───────────────                                                │  │
│  │                                                                 │  │
│  │  This diagram shows real-time energy flow between sources       │  │
│  │  and loads. Animated arrows indicate power direction.           │  │
│  │                                                                 │  │
│  │  Hover over any node to see detailed information.               │  │
│  │  Click to navigate to the detailed view.                        │  │
│  │                                                                 │  │
│  │                              [← Back] [Next: Status Cards →]    │  │
│  │                                                                 │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Step 4: Status Cards

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [4/6] [X]  │
│                                                                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌───────────┐ │
│  │ BESS STATUS  │  │ SOLAR STATUS │  │ GRID STATUS  │  │ WEATHER   │ │
│  │              │  │              │  │              │  │           │ │
│  │ SOC: 75%    │  │ Power: 125kW │  │ Import: 275kW│  │ 28°C      │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └───────────┘ │
│          ↑                                                            │
│          │                                                            │
│  ┌───────┴────────────────────────────────────────────────────────┐  │
│  │                                                                 │  │
│  │  Equipment Status Cards                                         │  │
│  │  ──────────────────────                                         │  │
│  │                                                                 │  │
│  │  Each card shows the current status of a system component:      │  │
│  │                                                                 │  │
│  │  • BESS: Battery charge level and power flow                    │  │
│  │  • Solar: Current production and efficiency                     │  │
│  │  • Grid: Import/export and connection quality                   │  │
│  │  • Weather: Conditions affecting solar production               │  │
│  │                                                                 │  │
│  │                              [← Back] [Next: Quick Actions →]   │  │
│  │                                                                 │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Step 5: Quick Actions

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [5/6] [X]  │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │  QUICK ACTIONS                                                  │  │
│  │  [Charge BESS] [Curtail Solar] [Generate Report] [Settings]     │  │
│  └────────────────────────────────────────────────────────────────┘  │
│          ↑                                                            │
│          │                                                            │
│  ┌───────┴────────────────────────────────────────────────────────┐  │
│  │                                                                 │  │
│  │  Quick Actions                                                  │  │
│  │  ─────────────                                                  │  │
│  │                                                                 │  │
│  │  Common operations are available here for quick access:         │  │
│  │                                                                 │  │
│  │  • Charge BESS: Start battery charging                          │  │
│  │  • Curtail Solar: Reduce solar output                           │  │
│  │  • Generate Report: Export energy data                          │  │
│  │  • Settings: Configure system parameters                        │  │
│  │                                                                 │  │
│  │  You can customize these shortcuts in Settings.                 │  │
│  │                                                                 │  │
│  │                              [← Back] [Next: Customize →]       │  │
│  │                                                                 │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

### Step 6: Customization

```
┌───────────────────────────────────────────────────────────────────────┐
│                                                           [6/6] [X]  │
│                                                                       │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────────┐ │
│  │                                                                  │ │
│  │  Customize Your Dashboard                                        │ │
│  │  ────────────────────────                                        │ │
│  │                                                                  │ │
│  │  Your dashboard is fully customizable!                           │ │
│  │                                                                  │ │
│  │  • Drag widgets to rearrange them                                │ │
│  │  • Click the gear icon to edit widget settings                   │ │
│  │  • Use the customize button to add/remove widgets                │ │
│  │                                                                  │ │
│  │  Your layout is saved automatically.                             │ │
│  │                                                                  │ │
│  │                                                                  │ │
│  │              [← Back]            [Finish Tour]                   │ │
│  │                                                                  │ │
│  └─────────────────────────────────────────────────────────────────┘ │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

---

## Tour Completion

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                    You're all set!                              │
│                                                                 │
│          Start exploring your energy management system          │
│                                                                 │
│                                                                 │
│                     [Go to Dashboard]                           │
│                                                                 │
│                     [Watch Video Tutorial]                      │
│                                                                 │
│   [ ] Don't show this again                                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Contextual Help

### Tooltip Help

Her widget'ta `?` ikonu ile yardım:

```
┌─────────────────────────────────────────┐
│  BESS STATUS                       [?]  │
│                                    ↓    │
│  ┌─────────────────────────────────────┐│
│  │ Battery Energy Storage System       ││
│  │                                     ││
│  │ Shows current charge level and      ││
│  │ power flow direction.               ││
│  │                                     ││
│  │ SOC = State of Charge (0-100%)      ││
│  │ + = Charging, - = Discharging       ││
│  │                                     ││
│  │ [Learn More]                        ││
│  └─────────────────────────────────────┘│
└─────────────────────────────────────────┘
```

### First-time Feature Highlights

Yeni özellikler için spotlight:

```
┌─────────────────────────────────────────────────────────────┐
│                                                     [×]     │
│                                                             │
│     ★ NEW FEATURE                                           │
│     ─────────────                                           │
│                                                             │
│     Weather forecast now includes solar irradiance          │
│     predictions for better production estimates.            │
│                                                             │
│     [Got it]                                                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Preferences Storage

```typescript
interface OnboardingPreferences {
  tourCompleted: boolean;
  tourCompletedAt: Date | null;
  showTourOnLogin: boolean;
  seenFeatures: string[];
  tooltipsEnabled: boolean;
}

// Default values
const defaultPreferences: OnboardingPreferences = {
  tourCompleted: false,
  tourCompletedAt: null,
  showTourOnLogin: true,
  seenFeatures: [],
  tooltipsEnabled: true,
};
```
