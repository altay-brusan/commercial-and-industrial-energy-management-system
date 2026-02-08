# Accessibility - EMS Dashboard

Erişilebilirlik standartları ve uygulamaları.

---

## Temel Gereksinimler

Dashboard WCAG 2.1 AA standartlarına uygun olmalıdır.

---

## 1. Color Blindness Support

### Renklerin Yanında İkonlar

Sadece renge güvenilmemeli, her zaman ikon/text desteği olmalı:

| Durum | Renk | İkon | Text |
|-------|------|------|------|
| Operational | Yeşil | [OK] | "Operational" |
| Warning | Turuncu | [!] | "Warning" |
| Critical | Kırmızı | [X] | "Critical" |
| Offline | Gri | [-] | "Offline" |

### Alternatif Renk Paletleri

```css
/* Protanopia/Deuteranopia friendly */
.color-blind-mode {
  --status-operational: oklch(0.7 0.15 250); /* Blue */
  --status-warning: oklch(0.8 0.15 90);      /* Yellow */
  --status-critical: oklch(0.5 0.2 30);      /* Orange-Red */
}
```

### Pattern Support

Grafiklerde renklerin yanında desenler:

```
Solar:    ████ (solid)
Grid:     ░░░░ (dotted)
Battery:  ▓▓▓▓ (striped)
Load:     ▒▒▒▒ (crosshatch)
```

---

## 2. Screen Reader Support

### ARIA Labels

```html
<div
  role="status"
  aria-label="System Health: All systems operational"
  aria-live="polite"
>
  <span class="sr-only">System Health Status</span>
  <div aria-hidden="true">████████████</div>
  <span>Operational</span>
</div>
```

### Live Regions

Dinamik içerik için aria-live:

```html
<!-- Alarm count updates -->
<div aria-live="assertive" aria-atomic="true">
  2 critical alarms require attention
</div>

<!-- Data updates -->
<div aria-live="polite" aria-atomic="false">
  Current power: 125 kW
</div>
```

### Semantic Structure

```html
<main role="main" aria-label="EMS Dashboard">
  <section aria-labelledby="system-health-title">
    <h2 id="system-health-title">System Health</h2>
    <!-- content -->
  </section>

  <section aria-labelledby="alarms-title">
    <h2 id="alarms-title">Active Alarms</h2>
    <!-- content -->
  </section>
</main>
```

---

## 3. Keyboard Navigation

### Tab Order

Mantıklı tab sırası:

```
1. Header navigation
2. System Health widget
3. Active Alarms widget
4. Power Flow diagram
5. BESS Status → Solar → Grid → Weather
6. Energy Today → Financial → Quick Actions
7. KPI Strip
8. Footer
```

### Focus Indicators

Belirgin focus göstergesi:

```css
:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
  border-radius: var(--radius);
}
```

### Keyboard Shortcuts

| Shortcut | Action | ARIA Announcement |
|----------|--------|-------------------|
| `R` | Refresh | "Dashboard refreshed" |
| `A` | Alarms | "Navigating to alarms" |
| `Esc` | Close | "Dialog closed" |

### Skip Links

```html
<a href="#main-content" class="skip-link">
  Skip to main content
</a>
<a href="#alarms" class="skip-link">
  Skip to alarms
</a>
```

---

## 4. High Contrast Mode

### System High Contrast

```css
@media (prefers-contrast: high) {
  :root {
    --background: white;
    --foreground: black;
    --border: black;
    --energy-solar: green;
    --energy-grid: red;
  }
}
```

### Manual Toggle

```
[Normal] [High Contrast] [Dark High Contrast]
```

---

## 5. Font Scaling

### Relative Units

```css
/* Use rem for font sizes */
.widget-title { font-size: 1rem; }
.widget-value { font-size: 1.875rem; }
.widget-label { font-size: 0.875rem; }

/* Minimum touch target */
.button { min-height: 44px; min-width: 44px; }
```

### User Preference

```
Font Size: [A-] [A] [A+]
           80%  100% 120%
```

### Layout Adaptation

Font büyütüldüğünde layout bozulmamalı:

```css
.widget-content {
  /* Allow content to wrap */
  overflow-wrap: break-word;
  /* Maintain readability */
  line-height: 1.5;
}
```

---

## 6. Motion Preferences

### Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### Animation Toggle

```
Animations: [ON] [Reduced] [OFF]
```

---

## 7. Error Identification

### Form Errors

```html
<div class="input-group" aria-invalid="true">
  <label for="power-limit">Power Limit (kW)</label>
  <input
    id="power-limit"
    type="number"
    aria-describedby="power-limit-error"
  />
  <span id="power-limit-error" role="alert">
    Value must be between 0 and 100 kW
  </span>
</div>
```

### Error Announcements

```javascript
// Announce errors to screen readers
function announceError(message) {
  const announcement = document.createElement('div');
  announcement.setAttribute('role', 'alert');
  announcement.setAttribute('aria-live', 'assertive');
  announcement.textContent = message;
  document.body.appendChild(announcement);
}
```

---

## 8. Testing Checklist

### Manual Testing

- [ ] Navigate entire dashboard with keyboard only
- [ ] Test with screen reader (VoiceOver/NVDA)
- [ ] Test with 200% zoom
- [ ] Test in high contrast mode
- [ ] Test with color blindness simulator
- [ ] Test with reduced motion enabled

### Automated Testing

```bash
# axe-core testing
npm run test:a11y

# Lighthouse accessibility audit
npm run lighthouse -- --only-categories=accessibility
```

### Acceptance Criteria

- [ ] All images have alt text
- [ ] All form inputs have labels
- [ ] Color contrast ratio ≥ 4.5:1
- [ ] Focus indicators visible
- [ ] No keyboard traps
- [ ] ARIA labels meaningful
- [ ] Error messages clear and helpful
