# Performance Requirements - EMS Dashboard

Dashboard performans hedefleri ve optimizasyon stratejileri.

---

## Performance Metrics

| Metrik | Hedef | Ölçüm Yöntemi |
|--------|-------|---------------|
| Initial Load | <2s | First Contentful Paint (FCP) |
| Time to Interactive | <3s | TTI |
| Widget Load | <500ms | Per widget render |
| Data Refresh | <100ms | WebSocket update to UI |
| Interaction Response | <50ms | Click to visual feedback |
| Animation | 60fps | Smooth transitions |
| Memory Usage | <100MB | JS Heap size |

---

## Loading Strategy

### Critical Path

```
1. HTML Shell (instant)
   └─> 2. Critical CSS (inline)
       └─> 3. Header + Navigation
           └─> 4. Tier 1 Widgets (System Health, Alarms)
               └─> 5. Tier 2 Widgets (BESS, Solar, Grid)
                   └─> 6. Tier 3 Widgets (Charts, KPIs)
                       └─> 7. Non-critical features
```

### Priority Loading

```typescript
// Widget loading priority
const widgetPriority = {
  'system-health': 1,  // Critical - load first
  'active-alarms': 1,
  'power-flow': 2,
  'bess-status': 2,
  'solar-status': 2,
  'grid-status': 2,
  'weather': 3,
  'energy-today': 3,
  'financial': 3,
  'kpi-strip': 4,
  'quick-actions': 4,
};
```

---

## Data Flow Architecture

### Real-time Updates

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│                 │     │                 │     │                 │
│   WebSocket     │────→│   State Store   │────→│   UI Update     │
│   Connection    │     │   (Zustand)     │     │   (React)       │
│                 │     │                 │     │                 │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│  Reconnection   │     │   Persistence   │
│  Logic          │     │   (IndexedDB)   │
└─────────────────┘     └─────────────────┘
```

### State Management

```typescript
interface DashboardState {
  // Real-time data
  systemHealth: SystemHealthData;
  alarms: Alarm[];
  powerFlow: PowerFlowData;
  bess: BESSData;
  solar: SolarData;
  grid: GridData;

  // Cached data
  weather: WeatherData;
  financial: FinancialData;
  kpis: KPIData;

  // UI state
  userPreferences: DashboardPreferences;
  isLoading: boolean;
  lastUpdate: Date;
  connectionStatus: 'connected' | 'disconnected' | 'reconnecting';
}
```

---

## Optimization Techniques

### 1. Code Splitting

```typescript
// Lazy load non-critical widgets
const WeatherWidget = lazy(() => import('./widgets/WeatherWidget'));
const FinancialSummary = lazy(() => import('./widgets/FinancialSummary'));
const KPIStrip = lazy(() => import('./widgets/KPIStrip'));

// Usage with Suspense
<Suspense fallback={<WidgetSkeleton />}>
  <WeatherWidget />
</Suspense>
```

### 2. Virtual Scrolling

Alarm listesi için virtual scrolling:

```typescript
// Only render visible items
<VirtualList
  itemCount={alarms.length}
  itemSize={64}
  overscanCount={5}
>
  {({ index, style }) => (
    <AlarmRow alarm={alarms[index]} style={style} />
  )}
</VirtualList>
```

### 3. Memoization

```typescript
// Memoize expensive calculations
const filteredAlarms = useMemo(() =>
  alarms.filter(a => a.priority === selectedPriority),
  [alarms, selectedPriority]
);

// Memoize components
const MemoizedPowerFlow = memo(PowerFlowDiagram);
```

### 4. Debouncing & Throttling

```typescript
// Debounce user input
const debouncedSearch = useDebouncedCallback(
  (query) => searchAlarms(query),
  300
);

// Throttle rapid updates
const throttledUpdate = useThrottledCallback(
  (data) => updateChart(data),
  100
);
```

### 5. Image Optimization

```typescript
// Use next/image for automatic optimization
<Image
  src="/icons/solar-panel.svg"
  width={24}
  height={24}
  priority={false}
  loading="lazy"
/>
```

---

## Caching Strategy

### Data Caching

```typescript
// Cache configuration
const cacheConfig = {
  weather: { ttl: 5 * 60 * 1000 },      // 5 minutes
  financial: { ttl: 60 * 1000 },         // 1 minute
  historical: { ttl: 24 * 60 * 60 * 1000 }, // 24 hours
  realtime: { ttl: 0 },                   // No cache
};
```

### Service Worker

```typescript
// Cache static assets
workbox.routing.registerRoute(
  /\.(?:js|css|woff2?)$/,
  new workbox.strategies.CacheFirst({
    cacheName: 'static-assets',
    plugins: [
      new workbox.expiration.ExpirationPlugin({
        maxEntries: 100,
        maxAgeSeconds: 7 * 24 * 60 * 60, // 1 week
      }),
    ],
  })
);
```

---

## Offline Support

### Offline Capabilities

| Feature | Offline Support |
|---------|-----------------|
| View cached data | Yes |
| View historical charts | Yes |
| Quick actions | No (requires connection) |
| Real-time updates | No |
| Export reports | Yes (from cache) |

### Offline Indicator

```
┌─────────────────────────────────────────────────────────────┐
│  [!] You are offline. Showing cached data from 5 min ago.   │
│                                      [Dismiss] [Try Again]  │
└─────────────────────────────────────────────────────────────┘
```

---

## Monitoring

### Performance Metrics Collection

```typescript
// Report Web Vitals
export function reportWebVitals(metric: Metric) {
  switch (metric.name) {
    case 'FCP':
    case 'LCP':
    case 'CLS':
    case 'FID':
    case 'TTFB':
      analytics.track('web_vital', {
        name: metric.name,
        value: metric.value,
        rating: metric.rating,
      });
      break;
  }
}
```

### Error Tracking

```typescript
// Track render errors
class ErrorBoundary extends Component {
  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    errorReporter.report({
      error,
      componentStack: errorInfo.componentStack,
      url: window.location.href,
    });
  }
}
```

---

## Performance Budget

### Bundle Size Limits

| Asset Type | Budget |
|------------|--------|
| Main JS bundle | <200KB (gzipped) |
| CSS | <50KB (gzipped) |
| Fonts | <100KB |
| Images | <500KB total |
| Total initial load | <500KB |

### Lighthouse Scores

| Category | Target Score |
|----------|--------------|
| Performance | >90 |
| Accessibility | >95 |
| Best Practices | >95 |
| SEO | >90 |

---

## Testing Performance

### Automated Tests

```bash
# Lighthouse CI
npm run lighthouse:ci

# Bundle analysis
npm run analyze

# Load testing
npm run loadtest
```

### Manual Testing

1. Test on slow 3G network
2. Test on low-end device
3. Test with 1000+ alarms
4. Test continuous updates for 1 hour
5. Test memory usage over time
