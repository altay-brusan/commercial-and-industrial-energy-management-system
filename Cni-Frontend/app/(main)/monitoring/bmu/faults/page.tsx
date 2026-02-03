'use client';
import React, { useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Messages } from 'primereact/messages';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';
import { Divider } from 'primereact/divider';

interface ContainerBatteryFaultState {
    kind: string;
    version: number;
    timestampUtc: string;
    // Pack Faults
    faultPackWarning: boolean;
    faultPackDanger: boolean;
    faultPackProtection: boolean;
    // Cell Faults
    faultCellOverVoltage: boolean;
    faultCellUnderVoltage: boolean;
    faultCellOverTemp: boolean;
    faultCellUnderTemp: boolean;
    // Contactor Faults
    faultDischargeContactor: boolean;
    faultChargeContactor: boolean;
    faultNegContactor: boolean;
    faultPrechargeContactor: boolean;
    // Board Faults
    faultBmuBoard: boolean;
    faultStackBoard: boolean;
    faultMeasurementBoard: boolean;
    faultCommunicationBoard: boolean;
}

const staticData: ContainerBatteryFaultState = {
    kind: "ContainerBatteryFaultState",
    version: 1,
    timestampUtc: "2026-01-08T17:00:00.000Z",
    faultPackWarning: false,
    faultPackDanger: false,
    faultPackProtection: false,
    faultCellOverVoltage: false,
    faultCellUnderVoltage: true,
    faultCellOverTemp: false,
    faultCellUnderTemp: false,
    faultDischargeContactor: false,
    faultChargeContactor: false,
    faultNegContactor: false,
    faultPrechargeContactor: false,
    faultBmuBoard: false,
    faultStackBoard: false,
    faultMeasurementBoard: false,
    faultCommunicationBoard: false
};

export default function BMUFaultsPage() {
    const t = useTranslations('BMUFaults');
    const msgs = useRef<Messages>(null);

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    const countActiveFaults = () => {
        return [
            staticData.faultPackDanger,
            staticData.faultPackProtection,
            staticData.faultCellOverVoltage,
            staticData.faultCellUnderVoltage,
            staticData.faultCellOverTemp,
            staticData.faultCellUnderTemp,
            staticData.faultDischargeContactor,
            staticData.faultChargeContactor,
            staticData.faultNegContactor,
            staticData.faultPrechargeContactor,
            staticData.faultBmuBoard,
            staticData.faultStackBoard,
            staticData.faultMeasurementBoard,
            staticData.faultCommunicationBoard
        ].filter(Boolean).length;
    };

    const countActiveWarnings = () => {
        return [staticData.faultPackWarning].filter(Boolean).length;
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString();
    };

    const FaultItem = ({ label, active, severity = 'danger' }: { label: string; active: boolean; severity?: 'danger' | 'warning' | 'success' }) => (
        <div className="flex justify-content-between align-items-center p-2 surface-50 border-round mb-2">
            <span>{label}</span>
            <Tag
                value={active ? (severity === 'warning' ? 'WARNING' : 'FAULT') : 'OK'}
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
                            <div className={`flex align-items-center justify-content-center border-round w-4rem h-4rem ${countActiveFaults() > 0 ? 'bg-red-100' : 'bg-green-100'}`}>
                                <i className={`pi pi-exclamation-triangle text-3xl ${countActiveFaults() > 0 ? 'text-red-500' : 'text-green-500'}`}></i>
                            </div>
                            <div>
                                <div className="text-500 text-sm">{t('activeFaults')}</div>
                                <div className={`text-3xl font-bold ${countActiveFaults() > 0 ? 'text-red-500' : 'text-green-500'}`}>
                                    {countActiveFaults()}
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
                <div className="col-12 md:col-6">
                    <Card className="h-full">
                        <div className="flex align-items-center gap-3">
                            <div className={`flex align-items-center justify-content-center border-round w-4rem h-4rem ${countActiveWarnings() > 0 ? 'bg-yellow-100' : 'bg-green-100'}`}>
                                <i className={`pi pi-exclamation-circle text-3xl ${countActiveWarnings() > 0 ? 'text-yellow-500' : 'text-green-500'}`}></i>
                            </div>
                            <div>
                                <div className="text-500 text-sm">{t('activeWarnings')}</div>
                                <div className={`text-3xl font-bold ${countActiveWarnings() > 0 ? 'text-yellow-500' : 'text-green-500'}`}>
                                    {countActiveWarnings()}
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
            </div>

            <div className="grid">
                {/* Pack Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-box mr-2"></i>
                            {t('packFaults')}
                        </h3>
                        <FaultItem label={t('faultPackWarning')} active={staticData.faultPackWarning} severity="warning" />
                        <FaultItem label={t('faultPackDanger')} active={staticData.faultPackDanger} />
                        <FaultItem label={t('faultPackProtection')} active={staticData.faultPackProtection} />
                    </Card>
                </div>

                {/* Cell Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-th-large mr-2"></i>
                            {t('cellFaults')}
                        </h3>
                        <FaultItem label={t('overVoltage')} active={staticData.faultCellOverVoltage} />
                        <FaultItem label={t('underVoltage')} active={staticData.faultCellUnderVoltage} />
                        <FaultItem label={t('overTemp')} active={staticData.faultCellOverTemp} />
                        <FaultItem label={t('underTemp')} active={staticData.faultCellUnderTemp} />
                    </Card>
                </div>

                {/* Contactor Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-bolt mr-2"></i>
                            {t('contactorFaults')}
                        </h3>
                        <FaultItem label={t('dischargeContactor')} active={staticData.faultDischargeContactor} />
                        <FaultItem label={t('chargeContactor')} active={staticData.faultChargeContactor} />
                        <FaultItem label={t('negContactor')} active={staticData.faultNegContactor} />
                        <FaultItem label={t('prechargeContactor')} active={staticData.faultPrechargeContactor} />
                    </Card>
                </div>

                {/* Board Faults */}
                <div className="col-12 lg:col-6">
                    <Card className="h-full">
                        <h3 className="m-0 text-lg font-semibold mb-4">
                            <i className="pi pi-microchip mr-2"></i>
                            {t('boardFaults')}
                        </h3>
                        <FaultItem label={t('bmuBoard')} active={staticData.faultBmuBoard} />
                        <FaultItem label={t('stackBoard')} active={staticData.faultStackBoard} />
                        <FaultItem label={t('measurementBoard')} active={staticData.faultMeasurementBoard} />
                        <FaultItem label={t('communicationBoard')} active={staticData.faultCommunicationBoard} />
                    </Card>
                </div>
            </div>

            <div className="text-right text-500 text-sm mt-3">
                Last updated: {formatDate(staticData.timestampUtc)}
            </div>
        </div>
    );
}
