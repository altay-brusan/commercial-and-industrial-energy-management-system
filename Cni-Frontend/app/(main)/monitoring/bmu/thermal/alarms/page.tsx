'use client';
import React, { useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Messages } from 'primereact/messages';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';

interface ChillerAlarmState {
    kind: string;
    version: number;
    timestampUtc: string;
    // Sensor Faults
    coolantPressSensorFault: boolean;
    coolantTempSensorFault: boolean;
    ambientTempSensorFault: boolean;
    dischargeTempSensorFault: boolean;
    // System Alarms
    lowPressureAlarm: boolean;
    highPressureAlarm: boolean;
    dischargeTempAlarm: boolean;
    compressorOverloadAlarm: boolean;
    // VFD Alarms
    vfdOverVoltageAlarm: boolean;
    vfdUnderVoltageAlarm: boolean;
    vfdOverCurrentAlarm: boolean;
    vfdOverTempAlarm: boolean;
    vfdPhaseLossAlarm: boolean;
    // Locked Faults
    lowPressureLocked: boolean;
    highPressureLocked: boolean;
    dischargeTempLocked: boolean;
}

const staticData: ChillerAlarmState = {
    kind: "ChillerAlarmState",
    version: 1,
    timestampUtc: "2026-01-08T17:00:00.000Z",
    coolantPressSensorFault: false,
    coolantTempSensorFault: false,
    ambientTempSensorFault: false,
    dischargeTempSensorFault: false,
    lowPressureAlarm: false,
    highPressureAlarm: true,
    dischargeTempAlarm: false,
    compressorOverloadAlarm: false,
    vfdOverVoltageAlarm: false,
    vfdUnderVoltageAlarm: false,
    vfdOverCurrentAlarm: false,
    vfdOverTempAlarm: false,
    vfdPhaseLossAlarm: false,
    lowPressureLocked: false,
    highPressureLocked: false,
    dischargeTempLocked: false
};

export default function ChillerAlarmsPage() {
    const t = useTranslations('BMUChillerAlarms');
    const msgs = useRef<Messages>(null);

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    const countActiveAlarms = () => {
        return [
            staticData.coolantPressSensorFault,
            staticData.coolantTempSensorFault,
            staticData.ambientTempSensorFault,
            staticData.dischargeTempSensorFault,
            staticData.lowPressureAlarm,
            staticData.highPressureAlarm,
            staticData.dischargeTempAlarm,
            staticData.compressorOverloadAlarm,
            staticData.vfdOverVoltageAlarm,
            staticData.vfdUnderVoltageAlarm,
            staticData.vfdOverCurrentAlarm,
            staticData.vfdOverTempAlarm,
            staticData.vfdPhaseLossAlarm
        ].filter(Boolean).length;
    };

    const countLockedAlarms = () => {
        return [
            staticData.lowPressureLocked,
            staticData.highPressureLocked,
            staticData.dischargeTempLocked
        ].filter(Boolean).length;
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString();
    };

    const AlarmItem = ({ label, active, severity = 'danger' }: { label: string; active: boolean; severity?: 'danger' | 'warning' | 'success' }) => (
        <div className="flex justify-content-between align-items-center p-2 surface-50 border-round mb-2">
            <span>{label}</span>
            <Tag
                value={active ? 'ALARM' : 'OK'}
                severity={active ? severity : 'success'}
            />
        </div>
    );

    return (
        <div>
            <div className="flex justify-content-between align-items-center mb-3">
                <h1 className="text-2xl font-bold m-0">{t('title')}</h1>
            </div>

            <Messages ref={msgs} />

            {/* Summary Cards */}
            <div className="grid mb-4">
                <div className="col-12 md:col-6">
                    <Card className="h-full">
                        <div className="flex align-items-center gap-3">
                            <div className={`flex align-items-center justify-content-center border-round w-4rem h-4rem ${countActiveAlarms() > 0 ? 'bg-red-100' : 'bg-green-100'}`}>
                                <i className={`pi pi-bell text-3xl ${countActiveAlarms() > 0 ? 'text-red-500' : 'text-green-500'}`}></i>
                            </div>
                            <div>
                                <div className="text-500 text-sm">{t('activeAlarms')}</div>
                                <div className={`text-3xl font-bold ${countActiveAlarms() > 0 ? 'text-red-500' : 'text-green-500'}`}>
                                    {countActiveAlarms()}
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
                <div className="col-12 md:col-6">
                    <Card className="h-full">
                        <div className="flex align-items-center gap-3">
                            <div className={`flex align-items-center justify-content-center border-round w-4rem h-4rem ${countLockedAlarms() > 0 ? 'bg-orange-100' : 'bg-green-100'}`}>
                                <i className={`pi pi-lock text-3xl ${countLockedAlarms() > 0 ? 'text-orange-500' : 'text-green-500'}`}></i>
                            </div>
                            <div>
                                <div className="text-500 text-sm">{t('lockedAlarms')}</div>
                                <div className={`text-3xl font-bold ${countLockedAlarms() > 0 ? 'text-orange-500' : 'text-green-500'}`}>
                                    {countLockedAlarms()}
                                </div>
                            </div>
                        </div>
                        {countLockedAlarms() > 0 && (
                            <div className="mt-3 text-sm text-orange-600">
                                <i className="pi pi-info-circle mr-1"></i>
                                {t('manualResetRequired')}
                            </div>
                        )}
                    </Card>
                </div>
            </div>

            <div className="grid">
                {/* Sensor Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-wifi mr-2"></i>
                            {t('sensorFaults')}
                        </h3>
                        <AlarmItem label={t('coolantPressSensor')} active={staticData.coolantPressSensorFault} />
                        <AlarmItem label={t('coolantTempSensor')} active={staticData.coolantTempSensorFault} />
                        <AlarmItem label={t('ambientTempSensor')} active={staticData.ambientTempSensorFault} />
                        <AlarmItem label={t('dischargeTempSensor')} active={staticData.dischargeTempSensorFault} />
                    </Card>
                </div>

                {/* System Alarms */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-cog mr-2"></i>
                            {t('systemAlarms')}
                        </h3>
                        <AlarmItem label={t('lowPressure')} active={staticData.lowPressureAlarm} severity="warning" />
                        <AlarmItem label={t('highPressure')} active={staticData.highPressureAlarm} />
                        <AlarmItem label={t('dischargeTemp')} active={staticData.dischargeTempAlarm} />
                        <AlarmItem label={t('compressorOverload')} active={staticData.compressorOverloadAlarm} />
                    </Card>
                </div>

                {/* VFD Alarms */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-bolt mr-2"></i>
                            {t('vfdAlarms')}
                        </h3>
                        <AlarmItem label={t('vfdOverVoltage')} active={staticData.vfdOverVoltageAlarm} />
                        <AlarmItem label={t('vfdUnderVoltage')} active={staticData.vfdUnderVoltageAlarm} />
                        <AlarmItem label={t('vfdOverCurrent')} active={staticData.vfdOverCurrentAlarm} />
                        <AlarmItem label={t('vfdOverTemp')} active={staticData.vfdOverTempAlarm} />
                        <AlarmItem label={t('vfdPhaseLoss')} active={staticData.vfdPhaseLossAlarm} />
                    </Card>
                </div>

                {/* Locked Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-lock mr-2"></i>
                            {t('lockedFaults')}
                        </h3>
                        <div className="mb-3 text-sm text-500">
                            <i className="pi pi-info-circle mr-1"></i>
                            {t('manualResetRequired')}
                        </div>
                        <AlarmItem label={t('lowPressure')} active={staticData.lowPressureLocked} severity="warning" />
                        <AlarmItem label={t('highPressure')} active={staticData.highPressureLocked} />
                        <AlarmItem label={t('dischargeTemp')} active={staticData.dischargeTempLocked} />
                    </Card>
                </div>
            </div>

            <div className="text-right text-500 text-sm mt-3">
                Last updated: {formatDate(staticData.timestampUtc)}
            </div>
        </div>
    );
}
