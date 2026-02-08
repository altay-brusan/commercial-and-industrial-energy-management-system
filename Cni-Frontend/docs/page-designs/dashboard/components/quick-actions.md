# Quick Actions Widget

Sık kullanılan aksiyonlara hızlı erişim sağlayan widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  QUICK ACTIONS                                              [Edit Shortcuts]│
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │              │  │              │  │              │  │              │    │
│  │   Charge     │  │   Curtail    │  │   Report     │  │   Silence    │    │
│  │    BESS      │  │    Solar     │  │   Generate   │  │    Alarms    │    │
│  │              │  │              │  │              │  │              │    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘    │
│                                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │              │  │              │  │              │  │              │    │
│  │   Grid       │  │   Refresh    │  │   Mobile     │  │   System     │    │
│  │  Disconnect  │  │    Data      │  │    Alert     │  │   Settings   │    │
│  │              │  │              │  │              │  │              │    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Varsayılan Aksiyonlar

### Primary Actions (Üst sıra)

| Aksiyon | İkon | Açıklama |
|---------|------|----------|
| Charge BESS | Battery+ | Manuel şarj başlat |
| Curtail Solar | Sun- | Solar üretimi kısıtla |
| Generate Report | Document | Rapor oluştur/indir |
| Silence Alarms | Bell-off | Alarm sesini kapat |

### Secondary Actions (Alt sıra)

| Aksiyon | İkon | Açıklama |
|---------|------|----------|
| Grid Disconnect | Plug-off | Şebekeden ayır |
| Refresh Data | Refresh | Verileri yenile |
| Send Alert | Phone | Mobil bildirim gönder |
| System Settings | Gear | Ayarlara git |

---

## Aksiyon Kategorileri

| Kategori | Aksiyonlar |
|----------|------------|
| Control | Charge BESS, Discharge BESS, Curtail Solar, Grid Disconnect |
| Monitoring | Refresh Data, Full Screen Mode, Export View |
| Communication | Send Alert, Generate Report, Share Dashboard |
| Settings | System Settings, User Preferences, Notifications |

---

## Button States

### Default
```css
background: var(--secondary);
color: var(--secondary-foreground);
```

### Hover
```css
background: var(--secondary/80);
transform: translateY(-2px);
```

### Active/Pressed
```css
background: var(--primary);
color: var(--primary-foreground);
```

### Disabled
```css
opacity: 0.5;
cursor: not-allowed;
```

### Loading
```css
/* Spinner overlay */
pointer-events: none;
```

---

## Confirmation Dialogs

Kritik aksiyonlar için onay:

```
┌─────────────────────────────────────────────────────────────┐
│  Disconnect from Grid?                                 [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  This will isolate the system from the utility grid.        │
│  The system will operate in island mode.                    │
│                                                             │
│  Are you sure you want to proceed?                          │
│                                                             │
│  [Cancel]                              [Yes, Disconnect]    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Structure

```typescript
interface QuickAction {
  id: string;
  label: string;
  description: string;
  icon?: string;
  variant: 'default' | 'primary' | 'destructive';
  requiresConfirmation: boolean;
  confirmationMessage?: string;
  isEnabled: boolean;
  disabledReason?: string;
}

interface QuickActionsData {
  actions: QuickAction[];
  customizable: boolean;
}
```

---

## Props

```typescript
interface QuickActionsProps {
  data: QuickActionsData;
  onAction: (actionId: string) => void;
  onEditShortcuts?: () => void;
  columns?: 2 | 4;
  showDescriptions?: boolean;
}
```

---

## Edit Shortcuts Modal

```
┌─────────────────────────────────────────────────────────────┐
│  EDIT QUICK ACTIONS                                    [×]  │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  YOUR SHORTCUTS (max 8)           AVAILABLE ACTIONS        │
│  ─────────────────────           ─────────────────         │
│  [≡] Charge BESS       [×]       [ ] Start Discharge       │
│  [≡] Curtail Solar     [×]       [ ] Remove Curtailment    │
│  [≡] Generate Report   [×]       [ ] Export Data           │
│  [≡] Silence Alarms    [×]       [ ] Enable Peak Shaving   │
│  [≡] Grid Disconnect   [×]       [ ] Schedule Charge       │
│  [≡] Refresh Data      [×]       [ ] View Logs             │
│  [≡] Send Alert        [×]       [ ] Emergency Stop        │
│  [≡] System Settings   [×]       [ ] Run Diagnostics       │
│                                                             │
│  Drag to reorder                 Click to add              │
│                                                             │
│  [Cancel]                              [Save Changes]       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## İnteraktivite

### Button Click
1. Check if enabled
2. Show confirmation if required
3. Execute action
4. Show feedback (toast/spinner)

### Edit Mode
- Drag to reorder
- Click × to remove
- Click available to add

---

## Feedback States

### Success
```
[OK] BESS charging started
```

### Error
```
[X] Failed to start charging: BESS offline
```

### Loading
```
[...] Starting charge...
```

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | 4×2 grid |
| Tablet | 4×2 grid, smaller |
| Mobile | 2×4 grid |

### Mobile Layout

```
┌─────────────────────────────────┐
│  QUICK ACTIONS           [Edit] │
├─────────────────────────────────┤
│                                 │
│  ┌───────────┐ ┌───────────┐   │
│  │  Charge   │ │  Curtail  │   │
│  │   BESS    │ │   Solar   │   │
│  └───────────┘ └───────────┘   │
│                                 │
│  ┌───────────┐ ┌───────────┐   │
│  │  Report   │ │  Silence  │   │
│  │ Generate  │ │  Alarms   │   │
│  └───────────┘ └───────────┘   │
│                                 │
│  [Show More]                    │
│                                 │
└─────────────────────────────────┘
```

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `1-8` | Trigger action 1-8 |
| `E` | Edit shortcuts |
| `R` | Refresh data |

---

## Accessibility

- Buttons: Clear labels with descriptions
- Confirmation: Focus trapped in modal
- Loading: Announce state changes
- Disabled: Explain why via tooltip
