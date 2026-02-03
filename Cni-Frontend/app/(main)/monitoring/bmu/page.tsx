'use client';
import React, { useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Messages } from 'primereact/messages';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';
import { ProgressBar } from 'primereact/progressbar';
import { Divider } from 'primereact/divider';
import { Button } from 'primereact/button';

interface ContainerBatteryState {
    kind: string;
    version: number;
    timestampUtc: string;
    socPercent: number;
    sohPercent: number;
    cycleCount: number;
    remainingEnergyKwh: number;
    availableEnergyKwh: number;
    totalVoltageV: number;
    currentA: number;
    cellTempC: number;
    cellVoltageMv: number;
    selectedBmuLoc: number;
    selectedCellLoc: number;
    minCellVoltageMv: number;
    maxCellVoltageMv: number;
    avgCellVoltageMv: number;
    minCellVoltageBmuLoc: number;
    minCellVoltageCellLoc: number;
    maxCellVoltageBmuLoc: number;
    maxCellVoltageCellLoc: number;
    minCellTempC: number;
    maxCellTempC: number;
    minCellTempBmuLoc: number;
    minCellTempCellLoc: number;
    maxCellTempBmuLoc: number;
    maxCellTempCellLoc: number;
    ambientTempC: number;
    outVoltPosV: number;
    outVoltNegV: number;
    stateCode: number;
    state: string;
    dmccA: number;
    dmdcA: number;
    chargeComplete: boolean;
    dischargeComplete: boolean;
    isolationStatus: number;
    contactStatusPos: number;
    contactStatusNeg: number;
}

const staticData: ContainerBatteryState = {
    kind: "ContainerBatteryState",
    version: 1,
    timestampUtc: "2026-01-08T17:00:00.000Z",
    socPercent: 100,
    sohPercent: 100,
    cycleCount: 312,
    remainingEnergyKwh: 234.56,
    availableEnergyKwh: 300.00,
    totalVoltageV: 748.5,
    currentA: -52.3,
    cellTempC: 26,
    cellVoltageMv: 3320,
    selectedBmuLoc: 1,
    selectedCellLoc: 5,
    minCellVoltageMv: 3285,
    maxCellVoltageMv: 3342,
    avgCellVoltageMv: 3315,
    minCellVoltageBmuLoc: 3,
    minCellVoltageCellLoc: 7,
    maxCellVoltageBmuLoc: 1,
    maxCellVoltageCellLoc: 2,
    minCellTempC: 22,
    maxCellTempC: 28,
    minCellTempBmuLoc: 1,
    minCellTempCellLoc: 3,
    maxCellTempBmuLoc: 2,
    maxCellTempCellLoc: 5,
    ambientTempC: 24,
    outVoltPosV: 750.2,
    outVoltNegV: 0.0,
    stateCode: 104,
    state: "Discharge",
    dmccA: 100.0,
    dmdcA: 100.0,
    chargeComplete: false,
    dischargeComplete: false,
    isolationStatus: 0,
    contactStatusPos: 1,
    contactStatusNeg: 1
};

export default function BMUPage() {
    const t = useTranslations('BMU');
    const tMenu = useTranslations('menu');
    const tCommon = useTranslations('common');
    const msgs = useRef<Messages>(null);
    const router = useRouter();

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    const getStateColor = (state: string) => {
        switch (state.toLowerCase()) {
            case 'charge':
                return 'success';
            case 'discharge':
                return 'warning';
            case 'standby':
                return 'info';
            case 'error':
                return 'danger';
            default:
                return 'info';
        }
    };

    const getSOCColor = (soc: number) => {
        if (soc > 70) return '#22C55E';
        if (soc > 30) return '#F59E0B';
        return '#EF4444';
    };

    const getSOHColor = (soh: number) => {
        if (soh > 85) return '#22C55E';
        if (soh >= 75) return '#FB923C';
        return '#EF4444';
    };

    const getTempColor = (temp: number) => {
        if (temp > 35) return 'text-red-500';
        if (temp > 30) return 'text-yellow-500';
        return 'text-green-500';
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString();
    };

    return (
        <div>
            <div className="flex justify-content-between align-items-center mb-3">
                <h1 className="text-2xl font-bold m-0">{t('title')}</h1>
                <Tag value={staticData.state} severity={getStateColor(staticData.state)} className="text-lg px-3 py-2" />
            </div>

            <Messages ref={msgs} />

            <div className="grid">
                {/* Battery Status Card */}
                <div className="col-12 lg:col-6 xl:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('batteryStatus')}</h3>

                        <div className="mb-4">
                            <div className="flex justify-content-between mb-2">
                                <span>{t('stateOfCharge')}</span>
                                <span className="font-bold">{staticData.socPercent}%</span>
                            </div>
                            <ProgressBar value={staticData.socPercent} showValue={false} style={{ height: '10px' }} color={getSOCColor(staticData.socPercent)} />
                        </div>

                        <div className="mb-4">
                            <div className="flex justify-content-between mb-2">
                                <span>{t('stateOfHealth')}</span>
                                <span className="font-bold">{staticData.sohPercent}%</span>
                            </div>
                            <ProgressBar value={staticData.sohPercent} showValue={false} style={{ height: '10px' }} color={getSOHColor(staticData.sohPercent)} />
                        </div>

                        <Divider />

                        <div className="flex flex-column gap-3">
                            <div className="flex justify-content-between">
                                <span>{t('cycleCount')}</span>
                                <span className="font-semibold">{staticData.cycleCount}</span>
                            </div>
                            <div className="flex justify-content-between">
                                <span>{t('remainingEnergy')}</span>
                                <span className="font-semibold">{staticData.remainingEnergyKwh} kWh</span>
                            </div>
                            <div className="flex justify-content-between">
                                <span>{t('availableEnergy')}</span>
                                <span className="font-semibold">{staticData.availableEnergyKwh} kWh</span>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Electrical Parameters Card */}
                <div className="col-12 lg:col-6 xl:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('totalVoltage')} & {t('current')}</h3>

                        <div className="flex flex-column gap-3">
                            <div className="flex justify-content-between align-items-center p-3 surface-100 border-round">
                                <span>{t('totalVoltage')}</span>
                                <span className="text-xl font-bold text-primary">{staticData.totalVoltageV} V</span>
                            </div>
                            <div className="flex justify-content-between align-items-center p-3 surface-100 border-round">
                                <span>{t('current')}</span>
                                <span className={`text-xl font-bold ${staticData.currentA < 0 ? 'text-orange-500' : 'text-green-500'}`}>
                                    {staticData.currentA} A
                                </span>
                            </div>
                        </div>

                        <Divider />

                        <h4 className="mt-0 mb-3">{t('outputVoltage')}</h4>
                        <div className="flex flex-column gap-3">
                            <div className="flex justify-content-between">
                                <span>{t('positiveVoltage')}</span>
                                <span className="font-semibold text-green-600">{staticData.outVoltPosV} V</span>
                            </div>
                            <div className="flex justify-content-between">
                                <span>{t('negativeVoltage')}</span>
                                <span className="font-semibold">{staticData.outVoltNegV} V</span>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Cell Information Card */}
                <div className="col-12 lg:col-6 xl:col-4">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('cellInfo')}</h3>

                        <div className="flex flex-column gap-3">
                            <div className="flex justify-content-between align-items-center">
                                <span>{t('cellTemperature')}</span>
                                <span className={`font-bold ${getTempColor(staticData.cellTempC)}`}>
                                    {staticData.cellTempC}°C
                                </span>
                            </div>
                            <div className="flex justify-content-between align-items-center">
                                <span>{t('cellVoltage')}</span>
                                <span className="font-bold">{staticData.cellVoltageMv} mV</span>
                            </div>
                            <div className="flex justify-content-between align-items-center">
                                <span>{t('selectedBmu')}</span>
                                <Tag value={`BMU ${staticData.selectedBmuLoc}`} severity="info" />
                            </div>
                            <div className="flex justify-content-between align-items-center">
                                <span>{t('selectedCell')}</span>
                                <Tag value={`Cell ${staticData.selectedCellLoc}`} severity="info" />
                            </div>
                            <div className="flex justify-content-between align-items-center">
                                <span>{t('ambientTemp')}</span>
                                <span className={`font-bold ${getTempColor(staticData.ambientTempC)}`}>
                                    {staticData.ambientTempC}°C
                                </span>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Voltage Statistics Card */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('voltageStatistics')}</h3>

                        <div className="grid">
                            <div className="col-12 md:col-4">
                                <div className="surface-100 p-3 border-round text-center h-full flex flex-column justify-content-between">
                                    <div className="text-sm text-500 mb-2">{t('minCellVoltage')}</div>
                                    <div className="text-2xl font-bold text-blue-500">{staticData.minCellVoltageMv} mV</div>
                                    <div className="text-xs text-500 mt-2">
                                        {t('location')}: BMU {staticData.minCellVoltageBmuLoc}, Cell {staticData.minCellVoltageCellLoc}
                                    </div>
                                </div>
                            </div>
                            <div className="col-12 md:col-4">
                                <div className="surface-100 p-3 border-round text-center h-full flex flex-column justify-content-between">
                                    <div className="text-sm text-500 mb-2">{t('avgCellVoltage')}</div>
                                    <div className="text-2xl font-bold text-primary">{staticData.avgCellVoltageMv} mV</div>
                                    <div className="text-xs text-500 mt-2">&nbsp;</div>
                                </div>
                            </div>
                            <div className="col-12 md:col-4">
                                <div className="surface-100 p-3 border-round text-center h-full flex flex-column justify-content-between">
                                    <div className="text-sm text-500 mb-2">{t('maxCellVoltage')}</div>
                                    <div className="text-2xl font-bold text-red-500">{staticData.maxCellVoltageMv} mV</div>
                                    <div className="text-xs text-500 mt-2">
                                        {t('location')}: BMU {staticData.maxCellVoltageBmuLoc}, Cell {staticData.maxCellVoltageCellLoc}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Temperature Statistics Card */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('temperatureStatistics')}</h3>

                        <div className="grid">
                            <div className="col-12 md:col-6">
                                <div className="surface-100 p-3 border-round text-center">
                                    <div className="text-sm text-500 mb-2">{t('minCellTemp')}</div>
                                    <div className="text-2xl font-bold text-blue-500">{staticData.minCellTempC}°C</div>
                                    <div className="text-xs text-500 mt-2">
                                        {t('location')}: BMU {staticData.minCellTempBmuLoc}, Cell {staticData.minCellTempCellLoc}
                                    </div>
                                </div>
                            </div>
                            <div className="col-12 md:col-6">
                                <div className="surface-100 p-3 border-round text-center">
                                    <div className="text-sm text-500 mb-2">{t('maxCellTemp')}</div>
                                    <div className="text-2xl font-bold text-red-500">{staticData.maxCellTempC}°C</div>
                                    <div className="text-xs text-500 mt-2">
                                        {t('location')}: BMU {staticData.maxCellTempBmuLoc}, Cell {staticData.maxCellTempCellLoc}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* Current Limits Card */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('currentLimits')}</h3>

                        <div className="grid">
                            <div className="col-12 md:col-6">
                                <div className="surface-100 p-3 border-round text-center">
                                    <div className="text-sm text-500 mb-2">{t('maxChargeCurrent')}</div>
                                    <div className="text-2xl font-bold text-green-500">{staticData.dmccA} A</div>
                                </div>
                            </div>
                            <div className="col-12 md:col-6">
                                <div className="surface-100 p-3 border-round text-center">
                                    <div className="text-sm text-500 mb-2">{t('maxDischargeCurrent')}</div>
                                    <div className="text-2xl font-bold text-orange-500">{staticData.dmdcA} A</div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>

                {/* System Status Card */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">{t('systemStatus')}</h3>

                        <div className="grid">
                            <div className="col-6">
                                <div className="flex flex-column gap-3">
                                    <div className="flex justify-content-between align-items-center">
                                        <span>{t('chargeComplete')}</span>
                                        <Tag
                                            value={staticData.chargeComplete ? tCommon('yes') : tCommon('no')}
                                            severity={staticData.chargeComplete ? 'success' : 'secondary'}
                                        />
                                    </div>
                                    <div className="flex justify-content-between align-items-center">
                                        <span>{t('dischargeComplete')}</span>
                                        <Tag
                                            value={staticData.dischargeComplete ? tCommon('yes') : tCommon('no')}
                                            severity={staticData.dischargeComplete ? 'success' : 'secondary'}
                                        />
                                    </div>
                                    <div className="flex justify-content-between align-items-center">
                                        <span>{t('isolationStatus')}</span>
                                        <Tag
                                            value={staticData.isolationStatus === 0 ? t('normal') : t('fault')}
                                            severity={staticData.isolationStatus === 0 ? 'success' : 'danger'}
                                        />
                                    </div>
                                </div>
                            </div>
                            <div className="col-6">
                                <div className="flex flex-column gap-3">
                                    <div className="text-sm font-semibold mb-1">{t('contactorStatus')}</div>
                                    <div className="flex justify-content-between align-items-center">
                                        <span>{t('positive')}</span>
                                        <Tag
                                            value={staticData.contactStatusPos === 1 ? t('closed') : t('open')}
                                            severity={staticData.contactStatusPos === 1 ? 'success' : 'warning'}
                                        />
                                    </div>
                                    <div className="flex justify-content-between align-items-center">
                                        <span>{t('negative')}</span>
                                        <Tag
                                            value={staticData.contactStatusNeg === 1 ? t('closed') : t('open')}
                                            severity={staticData.contactStatusNeg === 1 ? 'success' : 'warning'}
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
            </div>

            {/* Navigation Cards */}
            <Divider />
            <h3 className="text-lg font-semibold mb-3">
                <i className="pi pi-compass mr-2"></i>
                {t('navigationCards')}
            </h3>
            <div className="grid">
                <div className="col-12 md:col-6 lg:col-3">
                    <Card className="h-full cursor-pointer hover:shadow-4 transition-all transition-duration-200" onClick={() => router.push('/monitoring/bmu/faults')}>
                        <div className="flex flex-column align-items-center text-center">
                            <div className="flex align-items-center justify-content-center border-round w-4rem h-4rem bg-red-100 mb-3">
                                <i className="pi pi-exclamation-triangle text-3xl text-red-500"></i>
                            </div>
                            <h4 className="m-0 font-semibold mb-2">{tMenu('bmuFaults')}</h4>
                            <Button label={t('viewDetails')} icon="pi pi-arrow-right" text size="small" />
                        </div>
                    </Card>
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <Card className="h-full cursor-pointer hover:shadow-4 transition-all transition-duration-200" onClick={() => router.push('/monitoring/bmu/thermal')}>
                        <div className="flex flex-column align-items-center text-center">
                            <div className="flex align-items-center justify-content-center border-round w-4rem h-4rem bg-blue-100 mb-3">
                                <i className="pi pi-sun text-3xl text-blue-500"></i>
                            </div>
                            <h4 className="m-0 font-semibold mb-2">{tMenu('bmuThermal')}</h4>
                            <Button label={t('viewDetails')} icon="pi pi-arrow-right" text size="small" />
                        </div>
                    </Card>
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <Card className="h-full cursor-pointer hover:shadow-4 transition-all transition-duration-200" onClick={() => router.push('/monitoring/bmu/thermal/alarms')}>
                        <div className="flex flex-column align-items-center text-center">
                            <div className="flex align-items-center justify-content-center border-round w-4rem h-4rem bg-yellow-100 mb-3">
                                <i className="pi pi-bell text-3xl text-yellow-500"></i>
                            </div>
                            <h4 className="m-0 font-semibold mb-2">{tMenu('bmuChillerAlarms')}</h4>
                            <Button label={t('viewDetails')} icon="pi pi-arrow-right" text size="small" />
                        </div>
                    </Card>
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <Card className="h-full cursor-pointer hover:shadow-4 transition-all transition-duration-200" onClick={() => router.push('/monitoring/bmu/hvcu')}>
                        <div className="flex flex-column align-items-center text-center">
                            <div className="flex align-items-center justify-content-center border-round w-4rem h-4rem bg-purple-100 mb-3">
                                <i className="pi pi-sliders-h text-3xl text-purple-500"></i>
                            </div>
                            <h4 className="m-0 font-semibold mb-2">{tMenu('bmuHvcu')}</h4>
                            <Button label={t('viewDetails')} icon="pi pi-arrow-right" text size="small" />
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
