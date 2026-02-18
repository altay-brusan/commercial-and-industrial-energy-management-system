"use client";

import { useState, useEffect } from "react";
import { usePathname } from "next/navigation";
import { TopBar } from "@/components/layout/top-bar";
import { MonitoringSidebar } from "@/components/monitoring/monitoring-sidebar";
import { ConsumptionSidebar } from "@/components/consumption/consumption-sidebar";
import { ConsumerTable } from "@/components/consumption/consumer-table";
import { ConsumerDetail } from "@/components/consumption/consumer-detail";
import { AlarmDetailPanel } from "@/components/alarms/alarm-detail-panel";
import { EnergyAnalyticsView } from "@/components/consumption/energy-analytics-view";
import { DemandResponseView } from "@/components/consumption/demand-response-view";
import { consumers } from "@/lib/consumption/mock-data";
import { getAlarmById } from "@/lib/alarms/mock-data";
import type { Consumer } from "@/lib/consumption/types";
import type { Alarm } from "@/lib/alarms/types";
import type { ConsumptionView } from "@/components/consumption/consumption-sidebar";

const validViews: ConsumptionView[] = ["energy-analytics", "demand-response"];

export default function ConsumptionPage() {
  const pathname = usePathname();
  const segment = pathname.replace("/monitoring/consumption", "").replace(/^\//, "");
  const activeView: ConsumptionView = validViews.includes(segment as ConsumptionView)
    ? (segment as ConsumptionView)
    : "consumer-list";

  const [selectedConsumer, setSelectedConsumer] = useState<Consumer | null>(null);
  const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

  useEffect(() => {
    setSelectedConsumer(null);
    setSelectedAlarm(null);
  }, [activeView]);

  const handleAlarmOpen = (alarmId: string) => {
    const alarm = getAlarmById(alarmId);
    if (alarm) setSelectedAlarm(alarm);
  };

  const renderContent = () => {
    switch (activeView) {
      case "energy-analytics":
        return <EnergyAnalyticsView />;
      case "demand-response":
        return <DemandResponseView />;
      default:
        return selectedConsumer ? (
          <ConsumerDetail
            consumer={selectedConsumer}
            onBack={() => setSelectedConsumer(null)}
            onAlarmOpen={handleAlarmOpen}
          />
        ) : (
          <>
            <div className="shrink-0 bg-background px-6 py-5 border-b">
              <h1 className="text-xl font-semibold">Consumption Monitoring</h1>
            </div>
            <ConsumerTable
              consumers={consumers}
              onConsumerSelect={setSelectedConsumer}
            />
          </>
        );
    }
  };

  return (
    <div className="h-screen flex flex-col bg-background overflow-hidden">
      <TopBar activeTab="monitoring" />
      <div className="flex flex-1 overflow-hidden">
        <MonitoringSidebar />
        <ConsumptionSidebar />
        <main className="flex-1 flex flex-col overflow-hidden">
          {renderContent()}
        </main>
        {selectedAlarm && (
          <AlarmDetailPanel
            alarm={selectedAlarm}
            onClose={() => setSelectedAlarm(null)}
          />
        )}
      </div>
    </div>
  );
}
