# Weather Widget

Hava durumu ve solar üretim tahmini ilişkisini gösteren widget.

---

## Görsel Tasarım

```
┌─────────────────────────────────────────────────────────────┐
│  WEATHER & FORECAST                               [Details] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │      NOW              │        FORECAST               │ │
│  │  ────────────────────│─────────────────────────────── │ │
│  │                      │                                │ │
│  │   [Sun] 28°C         │   12:00  [Sun]  30°C  High     │ │
│  │   Clear Sky          │   15:00  [Cloud] 28°C Medium   │ │
│  │                      │   18:00  [Part]  24°C Low      │ │
│  │   Humidity: 45%      │                                │ │
│  │   Wind: 12 km/h NW   │   Tomorrow: [Sun] 32°C Sunny   │ │
│  │   UV Index: 8 High   │   Expected: 850 kWh            │ │
│  │                      │                                │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  Solar Irradiance: 850 W/m²    Cloud Cover: 10%            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Hava Durumu Koşulları

| Koşul | İkon | Renk | Solar Etkisi |
|-------|------|------|--------------|
| Clear/Sunny | [Sun] | Sarı | High |
| Partly Cloudy | [Part] | Açık Gri | Medium |
| Cloudy | [Cloud] | Gri | Low |
| Overcast | [Over] | Koyu Gri | Very Low |
| Rain | [Rain] | Mavi | Low |
| Storm | [Storm] | Koyu Mavi | Very Low |

**Not**: Emoji kullanılmaz, sadece metin veya ikon.

---

## Solar Üretim Tahmini Korelasyonu

| Hava | Irradiance | Tahmini Üretim |
|------|------------|----------------|
| Clear | >800 W/m² | 90-100% kapasite |
| Partly Cloudy | 500-800 W/m² | 60-85% kapasite |
| Cloudy | 200-500 W/m² | 30-55% kapasite |
| Overcast | <200 W/m² | <25% kapasite |

---

## Metrikler

### Current Conditions
- Temperature: °C
- Humidity: %
- Wind: km/h + yön
- UV Index: 0-11+ scale

### Solar-Related
- Irradiance: W/m²
- Cloud Cover: %
- Sunrise/Sunset times

### Forecast
- Hourly: Next 6-12 hours
- Daily: Tomorrow summary
- Production estimate: kWh

---

## Data Structure

```typescript
interface WeatherData {
  current: {
    temperature: number; // °C
    humidity: number; // %
    windSpeed: number; // km/h
    windDirection: string; // "NW", "SE", etc.
    condition: WeatherCondition;
    uvIndex: number;
    irradiance: number; // W/m²
    cloudCover: number; // %
  };
  forecast: WeatherForecast[];
  sunrise: string; // "06:42"
  sunset: string; // "19:35"
}

type WeatherCondition =
  | 'clear'
  | 'partly-cloudy'
  | 'cloudy'
  | 'overcast'
  | 'rain'
  | 'storm';

interface WeatherForecast {
  time: string; // "12:00" or "Tomorrow"
  temperature: number;
  condition: WeatherCondition;
  solarImpact: 'high' | 'medium' | 'low' | 'very-low';
  expectedProduction?: number; // kWh (for daily)
}
```

---

## Props

```typescript
interface WeatherWidgetProps {
  data: WeatherData;
  onDetailsClick?: () => void;
  showForecast?: boolean;
  showSolarImpact?: boolean;
  compact?: boolean;
}
```

---

## Solar Impact Indicator

Her forecast satırında solar etkisi:

```
High   → [+++] Yeşil  → "Optimal for solar"
Medium → [++]  Sarı   → "Good for solar"
Low    → [+]   Turuncu → "Reduced production"
V.Low  → [-]   Gri    → "Minimal production"
```

---

## İnteraktivite

### Hover
- Condition: Detailed description
- Forecast row: Extended forecast
- Irradiance: Production impact estimate

### Click
- Details → Full weather page
- Forecast → Extended forecast

---

## Responsive Behavior

| Breakpoint | Görünüm |
|------------|---------|
| Desktop | Full layout |
| Tablet | Current + 4 forecast |
| Mobile | Current only, tap for forecast |

---

## Data Source

Weather data API integration:
- Primary: OpenWeatherMap
- Secondary: Local weather station
- Update frequency: 15 minutes

---

## Night Mode

Gece modunda farklı görünüm:

```
┌─────────────────────────────────────────────────────────────┐
│  WEATHER                                        [Night]     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [Moon] 18°C Clear                                          │
│  Humidity: 55%  |  Wind: 8 km/h                             │
│                                                             │
│  Sunrise: 06:42 (in 7h 23m)                                │
│  Tomorrow: [Sun] 28°C - High solar expected                │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Accessibility

- Weather icons: aria-label with description
- Temperature: Include "degrees Celsius"
- Forecast: Time-based context
- Solar impact: Explain meaning
