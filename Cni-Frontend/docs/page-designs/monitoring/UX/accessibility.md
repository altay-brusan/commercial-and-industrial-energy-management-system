# Accessibility - Monitoring Pages

Monitoring sayfalarının erişilebilirlik gereksinimleri ve uygulamaları.

---

## Genel Standartlar

Monitoring sayfaları WCAG 2.1 AA standardına uygun olmalıdır:

- **Perceivable**: Tüm içerik algılanabilir olmalı
- **Operable**: Tüm fonksiyonlar klavye ile kullanılabilmeli
- **Understandable**: Arayüz anlaşılır olmalı
- **Robust**: Yardımcı teknolojilerle uyumlu olmalı

---

## Keyboard Navigation

### Focus Management

```
Tab Order (soldan sağa, yukarıdan aşağıya):
1. Header navigation
2. Refresh controls
3. Filter controls
4. Data table headers
5. Data table rows
6. Pagination
7. Footer actions
```

### Focus Indicators

```css
/* Visible focus indicator */
:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
  border-radius: 4px;
}

/* Remove default outline when using mouse */
:focus:not(:focus-visible) {
  outline: none;
}
```

### Skip Links

```html
<a href="#main-content" class="skip-link">
  Skip to main content
</a>
<a href="#data-table" class="skip-link">
  Skip to data table
</a>
```

```css
.skip-link {
  position: absolute;
  left: -9999px;
  z-index: 999;
}

.skip-link:focus {
  left: 10px;
  top: 10px;
  background: var(--background);
  padding: 8px 16px;
}
```

---

## Screen Reader Support

### ARIA Labels

```html
<!-- Status indicator -->
<span
  class="status-dot"
  role="status"
  aria-label="Device PV-01 is online and generating 125.4 kilowatts"
>
  ●
</span>

<!-- Connection status -->
<div
  role="status"
  aria-live="polite"
  aria-label="Connected to server, last update 5 seconds ago"
>
  ● Connected | Last: 14:32:05
</div>

<!-- Data table -->
<table
  role="grid"
  aria-label="Live monitoring data for all devices"
  aria-describedby="table-description"
>
```

### Live Regions

```html
<!-- Data updates (polite) -->
<div aria-live="polite" aria-atomic="true" class="sr-only">
  Data refreshed. PV-01 power updated to 127.5 kilowatts.
</div>

<!-- Critical alerts (assertive) -->
<div aria-live="assertive" aria-atomic="true" class="sr-only">
  Warning: Device BESS-01 has gone offline.
</div>

<!-- Connection status changes -->
<div role="alert" aria-live="assertive">
  Connection lost. Displaying cached data.
</div>
```

### Table Accessibility

```html
<table role="grid" aria-label="Live device data">
  <thead>
    <tr>
      <th scope="col" aria-sort="ascending">
        Device
        <span class="sr-only">, sorted ascending</span>
      </th>
      <th scope="col">Power</th>
      <th scope="col">Status</th>
      <th scope="col">Last Update</th>
    </tr>
  </thead>
  <tbody>
    <tr
      role="row"
      tabindex="0"
      aria-selected="false"
      aria-label="PV-01, 125.4 kilowatts, online, updated 14:32:05"
    >
      <td>PV-01</td>
      <td>125.4 kW</td>
      <td aria-label="online">●</td>
      <td>14:32:05</td>
    </tr>
  </tbody>
</table>
```

---

## Color Contrast

### Minimum Contrast Ratios

| Element | Ratio Required | Our Implementation |
|---------|----------------|-------------------|
| Normal text | 4.5:1 | 7:1 |
| Large text (18px+) | 3:1 | 4.5:1 |
| UI components | 3:1 | 4:1 |
| Focus indicators | 3:1 | 4.5:1 |

### Color Testing

```typescript
// Kontrast oranı hesaplama
function getContrastRatio(foreground: string, background: string): number {
  const l1 = getLuminance(foreground);
  const l2 = getLuminance(background);
  const lighter = Math.max(l1, l2);
  const darker = Math.min(l1, l2);
  return (lighter + 0.05) / (darker + 0.05);
}

// Test örneği
const ratio = getContrastRatio('--energy-solar', '--background');
// Should be >= 4.5 for normal text
```

### Color Independence

Renkler asla tek başına anlam taşımamalı:

```
GOOD:
● Online (yeşil + dolu nokta + "Online" text)
○ Offline (kırmızı + boş nokta + "Offline" text)
↑ +2.1 (yeşil + yukarı ok + pozitif değer)
↓ -1.5 (kırmızı + aşağı ok + negatif değer)

BAD:
● (sadece yeşil nokta)
● (sadece kırmızı nokta)
```

---

## Motion and Animation

### Reduced Motion Support

```css
@media (prefers-reduced-motion: reduce) {
  /* Disable all animations */
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
    scroll-behavior: auto !important;
  }

  /* Keep essential status indicators */
  .status-indicator.critical {
    /* Use border flash instead of pulse */
    border: 2px solid var(--energy-grid);
  }
}
```

### Essential vs Decorative Animation

| Animation | Type | Reduced Motion Behavior |
|-----------|------|------------------------|
| Loading spinner | Essential | Show static indicator |
| Value highlight | Essential | Use border instead |
| Flow diagram lines | Decorative | Static lines |
| Hover transitions | Decorative | Instant change |

---

## Form Controls

### Labels and Instructions

```html
<!-- Filter control -->
<div class="filter-group">
  <label for="source-filter" id="source-label">
    Filter by source
  </label>
  <select
    id="source-filter"
    aria-labelledby="source-label"
    aria-describedby="source-help"
  >
    <option value="all">All sources</option>
    <option value="pv">PV Systems</option>
    <option value="bess">BESS</option>
  </select>
  <span id="source-help" class="helper-text">
    Select one or more energy sources to display
  </span>
</div>

<!-- Search input -->
<div class="search-group">
  <label for="search-input" class="sr-only">
    Search parameters
  </label>
  <input
    type="search"
    id="search-input"
    placeholder="Search parameters..."
    aria-describedby="search-help"
  />
  <span id="search-help" class="sr-only">
    Type to filter the data table. Press Escape to clear.
  </span>
</div>
```

### Error Messages

```html
<div class="error-message" role="alert" aria-live="polite">
  <span class="error-icon" aria-hidden="true">!</span>
  <span>Unable to connect to server.
    <button aria-label="Retry connection">Retry</button>
  </span>
</div>
```

---

## Data Visualization Accessibility

### Chart Accessibility

```html
<!-- Chart container -->
<figure
  role="img"
  aria-label="24-hour voltage trend chart showing stable values between 398 and 401 volts"
>
  <canvas id="voltage-chart"></canvas>
  <figcaption class="sr-only">
    Voltage trend over the last 24 hours.
    Minimum: 398.2V at 03:00.
    Maximum: 401.1V at 14:30.
    Current: 399.5V.
  </figcaption>
</figure>

<!-- Alternative data table -->
<details>
  <summary>View chart data as table</summary>
  <table>
    <!-- Accessible data table version of chart -->
  </table>
</details>
```

### Power Flow Diagram

```html
<div
  role="img"
  aria-label="Power flow diagram showing energy distribution"
  aria-describedby="power-flow-description"
>
  <!-- SVG diagram -->
</div>

<div id="power-flow-description" class="sr-only">
  Current power flow:
  Solar panels generating 373 kilowatts flowing to the main bus.
  Battery charging at 50 kilowatts.
  Grid importing 27 kilowatts.
  Total load consumption is 450 kilowatts.
</div>
```

---

## Testing Checklist

### Manual Testing

- [ ] Navigate entire page using only keyboard
- [ ] Test with screen reader (VoiceOver, NVDA)
- [ ] Verify focus indicators are visible
- [ ] Check color contrast with tools
- [ ] Test with 200% zoom
- [ ] Test with reduced motion enabled
- [ ] Verify all images have alt text
- [ ] Check form labels and error messages

### Automated Testing

```typescript
// axe-core integration
import { axe } from 'jest-axe';

test('monitoring page has no accessibility violations', async () => {
  const { container } = render(<MonitoringPage />);
  const results = await axe(container);
  expect(results).toHaveNoViolations();
});
```

### Screen Reader Testing Script

```
1. Navigate to Monitoring > Live Data
2. Verify page title is announced
3. Tab through controls, verify each is announced
4. Navigate to data table
5. Verify row content is announced correctly
6. Trigger a data update, verify live region announces change
7. Open a filter dropdown, verify options are announced
8. Test with high contrast mode enabled
```

---

## Assistive Technology Compatibility

### Tested Screen Readers

| Screen Reader | Browser | Status |
|---------------|---------|--------|
| VoiceOver | Safari | ✅ Fully supported |
| VoiceOver | Chrome | ✅ Fully supported |
| NVDA | Firefox | ✅ Fully supported |
| NVDA | Chrome | ✅ Fully supported |
| JAWS | Chrome | ✅ Fully supported |
| TalkBack | Chrome Mobile | ✅ Fully supported |

### Known Issues & Workarounds

| Issue | Workaround |
|-------|------------|
| Live regions too frequent | Debounce updates to 5s minimum |
| Complex table navigation | Provide simplified view option |
| SVG diagram not accessible | Text description + data table |
