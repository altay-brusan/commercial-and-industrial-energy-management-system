'use client';
import React, { useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Messages } from 'primereact/messages';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';
import { Knob } from 'primereact/knob';
import { ProgressBar } from 'primereact/progressbar';

interface ChillerState {
    kind: string;
    version: number;
    timestampUtc: string;
    tmsMode: number;
    inletTempC: number;
    outletTempC: number;
    ambientTempC: number;
    inletPressureKpa: number;
    outletPressureKpa: number;
    compressorRunning: boolean;
    compressorRpm: number;
    pumpRunning: boolean;
    pumpRpm: number;
    fanRunning: boolean;
    electricHeaterOn: boolean;
    crankcaseHeaterOn: boolean;
    faultCode: number;
    faultLevel: number;
}

const staticData: ChillerState = {
    kind: "ChillerState",
    version: 1,
    timestampUtc: "2026-01-08T17:00:00.000Z",
    tmsMode: 1,
    inletTempC: 22.5,
    outletTempC: 18.3,
    ambientTempC: 28.0,
    inletPressureKpa: 350,
    outletPressureKpa: 320,
    compressorRunning: true,
    compressorRpm: 2400,
    pumpRunning: true,
    pumpRpm: 1800,
    fanRunning: true,
    electricHeaterOn: false,
    crankcaseHeaterOn: false,
    faultCode: 0,
    faultLevel: 0
};

export default function BMUThermalPage() {
    const t = useTranslations('BMUThermal');
    const msgs = useRef<Messages>(null);

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    const getModeLabel = (mode: number) => {
        const modeKey = `mode${mode}` as const;
        return t(modeKey);
    };

    const getModeColor = (mode: number) => {
        switch (mode) {
            case 0: return 'secondary';
            case 1: return 'info';
            case 2: return 'warning';
            case 3: return 'success';
            case 4: return 'secondary';
            case 5: return 'info';
            case 6: return 'danger';
            case 7: return 'warning';
            default: return 'secondary';
        }
    };

    const getTempColor = (temp: number) => {
        if (temp > 35) return '#EF4444';
        if (temp > 25) return '#F59E0B';
        return '#22C55E';
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString();
    };

    const ComponentStatus = ({ label, running, rpm }: { label: string; running: boolean; rpm?: number }) => (
        <div className="flex justify-content-between align-items-center p-3 surface-50 border-round mb-2">
            <div className="flex align-items-center gap-2">
                <i className={`pi ${running ? 'pi-spin pi-cog text-green-500' : 'pi-cog text-gray-400'}`}></i>
                <span className="font-medium">{label}</span>
            </div>
            <div className="flex align-items-center gap-3">
                {rpm !== undefined && running && (
                    <span className="text-sm text-500">{rpm} {t('rpm')}</span>
                )}
                <Tag
                    value={running ? t('running') : t('stopped')}
                    severity={running ? 'success' : 'secondary'}
                />
            </div>
        </div>
    );

    return (
        <div>
            <div className="flex justify-content-between align-items-center mb-3">
                <h1 className="text-2xl font-bold m-0">{t('title')}</h1>
                <Tag value={getModeLabel(staticData.tmsMode)} severity={getModeColor(staticData.tmsMode)} className="text-lg px-3 py-2" />
            </div>

            <Messages ref={msgs} />

            <div className="grid">
                {/* Operating Mode */}
                <div className="col-12 lg:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-sliders-h mr-2"></i>
                            {t('operatingMode')}
                        </h3>
                        <div className="flex flex-column align-items-center">
                            <div className="text-6xl font-bold text-primary mb-3">{staticData.tmsMode}</div>
                            <Tag value={getModeLabel(staticData.tmsMode)} severity={getModeColor(staticData.tmsMode)} className="text-lg px-4 py-2" />
                        </div>
                        {staticData.faultCode !== 0 && (
                            <div className="mt-4 p-3 surface-100 border-round">
                                <div className="flex justify-content-between mb-2">
                                    <span>{t('faultCode')}</span>
                                    <span className="font-bold text-red-500">{staticData.faultCode}</span>
                                </div>
                                <div className="flex justify-content-between">
                                    <span>{t('faultLevel')}</span>
                                    <span className="font-bold text-red-500">{staticData.faultLevel}</span>
                                </div>
                            </div>
                        )}
                    </Card>
                </div>

                {/* Temperatures */}
                <div className="col-12 lg:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-sun mr-2"></i>
                            {t('temperatures')}
                        </h3>
                        <div className="flex flex-column gap-4">
                            <div className="text-center">
                                <div className="text-sm text-500 mb-2">{t('inletTemp')}</div>
                                <Knob value={staticData.inletTempC} min={0} max={50} readOnly valueColor={getTempColor(staticData.inletTempC)} size={100} />
                                <div className="text-lg font-bold mt-1">{staticData.inletTempC}°C</div>
                            </div>
                            <div className="text-center">
                                <div className="text-sm text-500 mb-2">{t('outletTemp')}</div>
                                <Knob value={staticData.outletTempC} min={0} max={50} readOnly valueColor={getTempColor(staticData.outletTempC)} size={100} />
                                <div className="text-lg font-bold mt-1">{staticData.outletTempC}°C</div>
                            </div>
                            <div className="text-center">
                                <div className="text-sm text-500 mb-2">{t('ambientTemp')}</div>
                                <Knob value={staticData.ambientTempC} min={0} max={50} readOnly valueColor={getTempColor(staticData.ambientTempC)} size={100} />
                                <div className="text-lg font-bold mt-1">{staticData.ambientTempC}°C</div>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Pressures */}
                <div className="col-12 lg:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-gauge mr-2"></i>
                            {t('pressures')}
                        </h3>
                        <div className="flex flex-column gap-4">
                            <div>
                                <div className="flex justify-content-between mb-2">
                                    <span className="text-500">{t('inletPressure')}</span>
                                    <span className="font-bold">{staticData.inletPressureKpa} kPa</span>
                                </div>
                                <ProgressBar value={(staticData.inletPressureKpa / 500) * 100} showValue={false} style={{ height: '8px' }} />
                            </div>
                            <div>
                                <div className="flex justify-content-between mb-2">
                                    <span className="text-500">{t('outletPressure')}</span>
                                    <span className="font-bold">{staticData.outletPressureKpa} kPa</span>
                                </div>
                                <ProgressBar value={(staticData.outletPressureKpa / 500) * 100} showValue={false} style={{ height: '8px' }} />
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Component Status */}
                <div className="col-12">
                    <Card>
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-cog mr-2"></i>
                            {t('componentStatus')}
                        </h3>
                        <div className="grid">
                            <div className="col-12 lg:col-6">
                                <ComponentStatus label={t('compressor')} running={staticData.compressorRunning} rpm={staticData.compressorRpm} />
                                <ComponentStatus label={t('pump')} running={staticData.pumpRunning} rpm={staticData.pumpRpm} />
                                <ComponentStatus label={t('fan')} running={staticData.fanRunning} />
                            </div>
                            <div className="col-12 lg:col-6">
                                <ComponentStatus label={t('electricHeater')} running={staticData.electricHeaterOn} />
                                <ComponentStatus label={t('crankcaseHeater')} running={staticData.crankcaseHeaterOn} />
                            </div>
                        </div>
                    </Card>
                </div>
            </div>

            <div className="text-right text-500 text-sm mt-3">
                Last updated: {formatDate(staticData.timestampUtc)}
            </div>
        </div>
    );
}
