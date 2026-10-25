"use client";

interface SolarDetailPanelProps {
  onClose: () => void;
}

interface SolarPanelInfo {
  label: string;
  value: string;
}

const generalInfo: SolarPanelInfo[] = [
  { label: "System Name", value: "Main Solar Array" },
  { label: "Location", value: "Building A - Rooftop" },
  { label: "Installation Date", value: "March 15, 2023" },
  { label: "Total Capacity", value: "100 kW" },
  { label: "Panel Count", value: "280 panels" },
  { label: "Panel Model", value: "SunPower SPR-X22-360" },
  { label: "Inverter Model", value: "SMA Sunny Tripower 25000TL" },
  { label: "Inverter Count", value: "4 units" },
];

const currentStatus: SolarPanelInfo[] = [
  { label: "Current Output", value: "68.5 kW" },
  { label: "Capacity Utilization", value: "68.5%" },
  { label: "DC Voltage", value: "580 V" },
  { label: "AC Voltage", value: "400 V" },
  { label: "Current (DC)", value: "118 A" },
  { label: "Current (AC)", value: "99 A" },
  { label: "Frequency", value: "50.01 Hz" },
  { label: "Power Factor", value: "0.99" },
];

const productionData: SolarPanelInfo[] = [
  { label: "Today's Production", value: "245.8 kWh" },
  { label: "Yesterday's Production", value: "312.4 kWh" },
  { label: "This Week", value: "1,842 kWh" },
  { label: "This Month", value: "7,256 kWh" },
  { label: "This Year", value: "42,580 kWh" },
  { label: "Lifetime Production", value: "156,420 kWh" },
  { label: "Remaining Today (Est.)", value: "89.2 kWh" },
  { label: "Sunset Time", value: "18:42" },
];

const performanceMetrics: SolarPanelInfo[] = [
  { label: "Performance Ratio", value: "94.2%" },
  { label: "Specific Yield", value: "4.26 kWh/kWp" },
  { label: "Availability", value: "99.8%" },
  { label: "Capacity Factor", value: "18.2%" },
  { label: "System Efficiency", value: "96.5%" },
  { label: "Degradation Rate", value: "0.3%/year" },
];

const environmentalData: SolarPanelInfo[] = [
  { label: "Irradiance", value: "842 W/m²" },
  { label: "Module Temperature", value: "45.2°C" },
  { label: "Ambient Temperature", value: "28°C" },
  { label: "Wind Speed", value: "12 km/h" },
  { label: "Humidity", value: "45%" },
  { label: "Cloud Cover", value: "15%" },
];

const maintenanceInfo: SolarPanelInfo[] = [
  { label: "Last Maintenance", value: "January 8, 2025" },
  { label: "Next Scheduled", value: "April 8, 2025" },
  { label: "Last Cleaning", value: "December 15, 2024" },
  { label: "Warranty Expiry", value: "March 15, 2048" },
  { label: "Total Downtime (Year)", value: "4.2 hours" },
  { label: "Fault Count (Year)", value: "2" },
];

function InfoSection({ title, items }: { title: string; items: SolarPanelInfo[] }) {
  return (
    <div className="mb-6">
      <h3 className="text-sm font-semibold text-foreground mb-3 pb-2 border-b border-border">
        {title}
      </h3>
      <div className="space-y-2">
        {items.map((item) => (
          <div key={item.label} className="flex justify-between items-baseline">
            <span className="text-sm text-muted-foreground">{item.label}</span>
            <span className="text-sm font-medium text-foreground">{item.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

export function SolarDetailPanel({ onClose }: SolarDetailPanelProps) {
  return (
    <div className="w-[420px] shrink-0 border-l border-border bg-background overflow-y-auto">
      <div className="sticky top-0 bg-background border-b border-border px-6 py-4 flex items-center justify-between z-10">
        <h2 className="text-lg font-semibold">Solar Panels Details</h2>
        <button
          onClick={onClose}
          className="text-muted-foreground hover:text-foreground transition-colors text-xl leading-none"
        >
          ×
        </button>
      </div>

      <div className="p-6">
        <InfoSection title="General Information" items={generalInfo} />
        <InfoSection title="Current Status" items={currentStatus} />
        <InfoSection title="Production Data" items={productionData} />
        <InfoSection title="Performance Metrics" items={performanceMetrics} />
        <InfoSection title="Environmental Conditions" items={environmentalData} />
        <InfoSection title="Maintenance" items={maintenanceInfo} />
      </div>
    </div>
  );
}
