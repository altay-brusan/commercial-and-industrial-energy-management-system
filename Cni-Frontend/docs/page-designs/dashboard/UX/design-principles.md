# Design Principles - EMS Dashboard

Dashboard tasarımının temel prensipleri.

---

## 1. Glanceable Design

```
┌─────────────────────────────────────────────────────────────────┐
│  "Glanceable" Tasarım                                           │
│  ─────────────────────                                          │
│  Kullanıcı 3 saniyede sistemin genel durumunu anlayabilmeli     │
└─────────────────────────────────────────────────────────────────┘
```

### Uygulama Kuralları

- En kritik bilgi en üstte ve en büyük
- Renk kodları tutarlı ve anlamlı
- Sayılar büyük ve okunabilir
- Status göstergeleri belirgin
- Gereksiz detaylar gizli

### Örnekler

**İyi:**
- "System: Operational" yeşil badge
- "SOC: 75%" büyük font ile
- Kritik alarm sayısı kırmızı badge'de

**Kötü:**
- Uzun açıklama metinleri
- Küçük fontlar
- Çok fazla detay aynı anda

---

## 2. Progressive Disclosure

```
┌─────────────────────────────────────────────────────────────────┐
│  "Progressive Disclosure"                                        │
│  ────────────────────────                                        │
│  Özet → Hover ile detay → Click ile sayfa                       │
└─────────────────────────────────────────────────────────────────┘
```

### 3 Seviyeli Bilgi Akışı

| Seviye | Tetikleyici | Bilgi Derinliği |
|--------|-------------|-----------------|
| Level 1 | Görünür | Ana metrik/durum |
| Level 2 | Hover | Detay tooltip |
| Level 3 | Click | Tam sayfa detay |

### Örnek: BESS Widget

1. **Görünür**: SOC: 75%, Charging
2. **Hover**: Temperature, Voltage, Current, Estimated time
3. **Click**: Full BESS monitoring page

---

## 3. Action-Oriented

```
┌─────────────────────────────────────────────────────────────────┐
│  "Action-Oriented"                                               │
│  ─────────────────                                               │
│  Her widget bir aksiyon önerebilmeli                            │
└─────────────────────────────────────────────────────────────────┘
```

### Aksiyon Tipleri

| Widget | Suggested Action | Trigger |
|--------|------------------|---------|
| BESS (Low SOC) | "Charge Now" | SOC < 20% |
| Solar (Curtailed) | "Remove Curtailment" | Manual curtail active |
| Alarms | "Acknowledge All" | Multiple unack alarms |
| Grid (Peak) | "Discharge BESS" | Peak tariff active |

### Contextual Actions

Widget durumuna göre dinamik aksiyon önerileri:

```typescript
function getSuggestedAction(widget: WidgetState): Action | null {
  if (widget.type === 'bess' && widget.soc < 20) {
    return { label: 'Charge BESS', action: 'startCharge' };
  }
  if (widget.type === 'alarms' && widget.unacknowledgedCount > 3) {
    return { label: 'Acknowledge All', action: 'ackAll' };
  }
  return null;
}
```

---

## 4. Consistency

### Renk Tutarlılığı

Aynı renk her yerde aynı anlama gelmeli:
- Yeşil = Solar / Pozitif / Operational
- Kırmızı = Grid / Negatif / Critical
- Mor = Battery
- Turuncu = Warning / Consumption

### Layout Tutarlılığı

Her widget aynı yapıyı takip etmeli:
1. Header (title + status badge)
2. Main content (primary metric)
3. Secondary metrics
4. Footer (actions / details link)

### Interaction Tutarlılığı

- Hover: Her yerde tooltip
- Click: Her yerde detay sayfası
- Right-click: Context menu (opsiyonel)

---

## 5. Feedback

### Immediate Feedback

Her kullanıcı aksiyonuna anında feedback:

| Aksiyon | Feedback |
|---------|----------|
| Button click | Loading spinner |
| Data refresh | Flash animation |
| Error | Toast notification |
| Success | Checkmark + message |

### System Status Feedback

Sistem durumu her zaman görünür:
- Last update timestamp
- Connection status indicator
- Auto-refresh countdown

---

## 6. Error Prevention

### Confirmation Dialogs

Kritik aksiyonlar için onay:
- "Disconnect from Grid?" - Yes/Cancel
- "Discharge BESS to 0%?" - Yes/Cancel
- "Clear all alarms?" - Yes/Cancel

### Smart Defaults

Güvenli varsayılan değerler:
- BESS discharge: Stop at 20% SOC
- Solar curtailment: 80% of capacity
- Grid disconnect: After 5-second countdown

---

## 7. Recoverability

### Undo Actions

Mümkün olan her yerde geri alma:
- "BESS charge started. [Undo]"
- "Report generated. [Delete]"
- "Alarm acknowledged. [Undo]"

### Auto-save

Kullanıcı tercihleri otomatik kayıt:
- Dashboard layout
- Widget visibility
- Refresh intervals
