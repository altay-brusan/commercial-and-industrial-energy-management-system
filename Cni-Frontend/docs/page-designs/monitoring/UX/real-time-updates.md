# Real-time Updates - Monitoring Pages

Monitoring sayfalarında gerçek zamanlı veri güncelleme stratejileri ve uygulamaları.

---

## Güncelleme Stratejisi

### Polling vs WebSocket

| Yöntem | Kullanım | Avantajlar | Dezavantajlar |
|--------|----------|------------|---------------|
| Polling | Standard monitoring | Basit, güvenilir | Gecikme, bandwidth |
| WebSocket | Critical alerts | Anlık, düşük latency | Kompleks, connection mgmt |
| SSE | Live data streams | One-way, efficient | Browser desteği |

### Önerilen Hibrit Yaklaşım

```typescript
const updateStrategy = {
  // Kritik alarmlar için WebSocket
  criticalAlerts: {
    method: 'websocket',
    reconnectInterval: 1000,
    maxRetries: 10,
  },

  // Standard data için polling
  liveData: {
    method: 'polling',
    interval: 5000,
    pauseOnHidden: true,
    pauseOnInteraction: true,
  },

  // Historical data için lazy loading
  historicalData: {
    method: 'fetch-on-demand',
    cacheTime: 60000,
  },
};
```

---

## Polling Implementation

### Basic Polling Hook

```typescript
function useLiveData(interval: number = 5000) {
  const [data, setData] = useState<LiveData | null>(null);
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const response = await api.getLiveData();
      setData(response);
      setLastUpdate(new Date());
      setError(null);
    } catch (err) {
      setError(err as Error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
    const timer = setInterval(fetchData, interval);
    return () => clearInterval(timer);
  }, [fetchData, interval]);

  return { data, lastUpdate, isLoading, error, refresh: fetchData };
}
```

### Smart Polling (Pause on Interaction)

```typescript
function useSmartPolling(interval: number = 5000) {
  const [isPaused, setIsPaused] = useState(false);
  const pauseTimeoutRef = useRef<NodeJS.Timeout>();

  const pausePolling = useCallback((duration: number = 10000) => {
    setIsPaused(true);

    if (pauseTimeoutRef.current) {
      clearTimeout(pauseTimeoutRef.current);
    }

    pauseTimeoutRef.current = setTimeout(() => {
      setIsPaused(false);
    }, duration);
  }, []);

  // Pause on user interaction
  useEffect(() => {
    const handleInteraction = () => pausePolling(10000);

    document.addEventListener('mousemove', handleInteraction);
    document.addEventListener('keydown', handleInteraction);
    document.addEventListener('scroll', handleInteraction);

    return () => {
      document.removeEventListener('mousemove', handleInteraction);
      document.removeEventListener('keydown', handleInteraction);
      document.removeEventListener('scroll', handleInteraction);
    };
  }, [pausePolling]);

  // Pause when tab is hidden
  useEffect(() => {
    const handleVisibilityChange = () => {
      if (document.hidden) {
        setIsPaused(true);
      } else {
        setIsPaused(false);
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => document.removeEventListener('visibilitychange', handleVisibilityChange);
  }, []);

  return { isPaused, pausePolling };
}
```

---

## Data Diffing & Change Detection

### Efficient Updates

```typescript
interface DataPoint {
  id: string;
  value: number;
  timestamp: Date;
}

function detectChanges(oldData: DataPoint[], newData: DataPoint[]): {
  added: DataPoint[];
  updated: DataPoint[];
  removed: DataPoint[];
} {
  const oldMap = new Map(oldData.map(d => [d.id, d]));
  const newMap = new Map(newData.map(d => [d.id, d]));

  const added: DataPoint[] = [];
  const updated: DataPoint[] = [];
  const removed: DataPoint[] = [];

  // Check for new and updated
  for (const [id, newItem] of newMap) {
    const oldItem = oldMap.get(id);
    if (!oldItem) {
      added.push(newItem);
    } else if (oldItem.value !== newItem.value) {
      updated.push(newItem);
    }
  }

  // Check for removed
  for (const [id, oldItem] of oldMap) {
    if (!newMap.has(id)) {
      removed.push(oldItem);
    }
  }

  return { added, updated, removed };
}
```

### Change Highlighting

```typescript
function useChangeHighlight(data: DataPoint[]) {
  const [highlightedIds, setHighlightedIds] = useState<Set<string>>(new Set());
  const prevDataRef = useRef<DataPoint[]>([]);

  useEffect(() => {
    const changes = detectChanges(prevDataRef.current, data);
    const changedIds = new Set([
      ...changes.updated.map(d => d.id),
      ...changes.added.map(d => d.id),
    ]);

    setHighlightedIds(changedIds);

    // Clear highlights after animation
    const timer = setTimeout(() => {
      setHighlightedIds(new Set());
    }, 500);

    prevDataRef.current = data;

    return () => clearTimeout(timer);
  }, [data]);

  return highlightedIds;
}
```

---

## Connection Management

### Connection Status

```typescript
type ConnectionStatus = 'connected' | 'connecting' | 'disconnected' | 'error';

interface ConnectionState {
  status: ConnectionStatus;
  lastConnected: Date | null;
  retryCount: number;
  error: Error | null;
}

function useConnectionStatus() {
  const [state, setState] = useState<ConnectionState>({
    status: 'connecting',
    lastConnected: null,
    retryCount: 0,
    error: null,
  });

  const connect = useCallback(async () => {
    setState(prev => ({ ...prev, status: 'connecting' }));

    try {
      await api.connect();
      setState({
        status: 'connected',
        lastConnected: new Date(),
        retryCount: 0,
        error: null,
      });
    } catch (error) {
      setState(prev => ({
        ...prev,
        status: 'error',
        error: error as Error,
        retryCount: prev.retryCount + 1,
      }));
    }
  }, []);

  return { ...state, connect };
}
```

### Reconnection Strategy

```typescript
const reconnectionConfig = {
  maxRetries: 10,
  initialDelay: 1000,
  maxDelay: 30000,
  backoffMultiplier: 2,
};

function getReconnectionDelay(retryCount: number): number {
  const delay = reconnectionConfig.initialDelay *
    Math.pow(reconnectionConfig.backoffMultiplier, retryCount);
  return Math.min(delay, reconnectionConfig.maxDelay);
}

// Exponential backoff: 1s, 2s, 4s, 8s, 16s, 30s, 30s...
```

---

## UI Update Patterns

### Refresh Indicator

```
┌─────────────────────────────────────────────────────────────┐
│ Auto-refresh: [5s ▼]  Next update in: ████░░░░░░ 3s        │
│                       [||] Pause  [⟳] Refresh Now          │
└─────────────────────────────────────────────────────────────┘
```

```typescript
function RefreshIndicator({ interval, onRefresh }: Props) {
  const [countdown, setCountdown] = useState(interval / 1000);
  const [isPaused, setIsPaused] = useState(false);

  useEffect(() => {
    if (isPaused) return;

    const timer = setInterval(() => {
      setCountdown(prev => {
        if (prev <= 1) {
          onRefresh();
          return interval / 1000;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [interval, isPaused, onRefresh]);

  const progress = ((interval / 1000 - countdown) / (interval / 1000)) * 100;

  return (
    <div className="flex items-center gap-4">
      <select value={interval} onChange={/* ... */}>
        <option value={5000}>5s</option>
        <option value={15000}>15s</option>
        <option value={30000}>30s</option>
      </select>
      <div className="flex-1 h-2 bg-muted rounded">
        <div
          className="h-full bg-energy-solar rounded transition-all"
          style={{ width: `${progress}%` }}
        />
      </div>
      <span className="text-sm text-muted-foreground">{countdown}s</span>
      <Button onClick={() => setIsPaused(!isPaused)}>
        {isPaused ? '▶' : '||'}
      </Button>
    </div>
  );
}
```

### Stale Data Warning

```typescript
function StaleDataWarning({ lastUpdate }: { lastUpdate: Date }) {
  const ageMs = Date.now() - lastUpdate.getTime();
  const ageMinutes = Math.floor(ageMs / 60000);

  if (ageMinutes < 1) return null;

  const severity = ageMinutes < 5 ? 'warning' : 'critical';

  return (
    <Alert variant={severity}>
      <AlertTitle>Data may be outdated</AlertTitle>
      <AlertDescription>
        Last update was {ageMinutes} minute{ageMinutes > 1 ? 's' : ''} ago.
        {severity === 'critical' && (
          <Button onClick={/* retry */}>Retry Connection</Button>
        )}
      </AlertDescription>
    </Alert>
  );
}
```

---

## Performance Optimization

### Batch Updates

```typescript
// Batch multiple state updates
function useBatchedUpdates() {
  const pendingUpdatesRef = useRef<Update[]>([]);
  const flushTimeoutRef = useRef<NodeJS.Timeout>();

  const queueUpdate = useCallback((update: Update) => {
    pendingUpdatesRef.current.push(update);

    if (!flushTimeoutRef.current) {
      flushTimeoutRef.current = setTimeout(() => {
        const updates = pendingUpdatesRef.current;
        pendingUpdatesRef.current = [];
        flushTimeoutRef.current = undefined;

        // Apply all updates at once
        applyBatchedUpdates(updates);
      }, 100); // 100ms batch window
    }
  }, []);

  return queueUpdate;
}
```

### Virtual Scrolling for Large Data Sets

```typescript
import { useVirtualizer } from '@tanstack/react-virtual';

function LiveDataTable({ data }: { data: DataPoint[] }) {
  const parentRef = useRef<HTMLDivElement>(null);

  const virtualizer = useVirtualizer({
    count: data.length,
    getScrollElement: () => parentRef.current,
    estimateSize: () => 48, // row height
    overscan: 10,
  });

  return (
    <div ref={parentRef} className="h-[600px] overflow-auto">
      <div style={{ height: virtualizer.getTotalSize() }}>
        {virtualizer.getVirtualItems().map(virtualRow => (
          <DataRow
            key={data[virtualRow.index].id}
            data={data[virtualRow.index]}
            style={{
              position: 'absolute',
              top: virtualRow.start,
              height: virtualRow.size,
            }}
          />
        ))}
      </div>
    </div>
  );
}
```

---

## Offline Support

### Caching Strategy

```typescript
const cacheConfig = {
  // Fresh data period
  freshFor: 30 * 1000, // 30 seconds

  // Stale-while-revalidate period
  staleWhileRevalidate: 5 * 60 * 1000, // 5 minutes

  // Maximum cache age
  maxAge: 15 * 60 * 1000, // 15 minutes
};

async function fetchWithCache(key: string) {
  const cached = await cache.get(key);

  if (cached) {
    const age = Date.now() - cached.timestamp;

    if (age < cacheConfig.freshFor) {
      return { data: cached.data, source: 'cache-fresh' };
    }

    if (age < cacheConfig.staleWhileRevalidate) {
      // Return stale data immediately, revalidate in background
      fetchFresh(key).then(data => cache.set(key, data));
      return { data: cached.data, source: 'cache-stale' };
    }
  }

  // No cache or expired, fetch fresh
  const data = await fetchFresh(key);
  await cache.set(key, data);
  return { data, source: 'network' };
}
```

### Offline Indicator

```typescript
function useOnlineStatus() {
  const [isOnline, setIsOnline] = useState(navigator.onLine);

  useEffect(() => {
    const handleOnline = () => setIsOnline(true);
    const handleOffline = () => setIsOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  return isOnline;
}
```

```
┌─────────────────────────────────────────────────────────────┐
│ [!] You are offline                                         │
│     Showing cached data from 5 minutes ago.                 │
│     Some features may be unavailable.                       │
└─────────────────────────────────────────────────────────────┘
```
