'use client';
import React, { useRef, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Messages } from 'primereact/messages';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';
import { InputSwitch } from 'primereact/inputswitch';

interface HVCUControlState {
    kind: string;
    version: number;
    timestampUtc: string;
    pack1IsolationControl: boolean;
    bcu01StateCode: number;
    bcu02StateCode: number;
    bcu03StateCode: number;
    bcu04StateCode: number;
}

const staticData: HVCUControlState = {
    kind: "HVCUControlState",
    version: 1,
    timestampUtc: "2026-01-08T17:00:00.000Z",
    pack1IsolationControl: true,
    bcu01StateCode: 104,
    bcu02StateCode: 101,
    bcu03StateCode: 105,
    bcu04StateCode: 101
};

const BCU_STATES: { [key: number]: { label: string; color: 'secondary' | 'info' | 'warning' | 'success' | 'danger' } } = {
    101: { label: 'standby', color: 'info' },
    102: { label: 'startup', color: 'warning' },
    103: { label: 'precharge', color: 'warning' },
    104: { label: 'discharge', color: 'danger' },
    105: { label: 'charge', color: 'success' },
    106: { label: 'shutdown', color: 'secondary' }
};

export default function HVCUControlPage() {
    const t = useTranslations('BMUHVCU');
    const msgs = useRef<Messages>(null);

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleString();
    };

    const getBCUState = (stateCode: number) => {
        return BCU_STATES[stateCode] || { label: 'unknown', color: 'secondary' as const };
    };

    const BCUCard = ({ label, stateCode }: { label: string; stateCode: number }) => {
        const state = getBCUState(stateCode);
        return (
            <Card className="h-full">
                <div className="flex flex-column align-items-center text-center">
                    <h4 className="m-0 mb-3 font-semibold">{label}</h4>
                    <div className={`flex align-items-center justify-content-center border-round w-5rem h-5rem mb-3 ${
                        state.color === 'success' ? 'bg-green-100' :
                        state.color === 'danger' ? 'bg-orange-100' :
                        state.color === 'warning' ? 'bg-yellow-100' :
                        state.color === 'info' ? 'bg-blue-100' :
                        'bg-gray-100'
                    }`}>
                        <i className={`pi pi-server text-4xl ${
                            state.color === 'success' ? 'text-green-500' :
                            state.color === 'danger' ? 'text-orange-500' :
                            state.color === 'warning' ? 'text-yellow-500' :
                            state.color === 'info' ? 'text-blue-500' :
                            'text-gray-500'
                        }`}></i>
                    </div>
                    <Tag value={t(state.label as keyof typeof t)} severity={state.color} className="text-sm px-3 py-1 mb-2" />
                    <div className="text-sm text-500 mt-2">
                        {t('stateCode')}: {stateCode}
                    </div>
                </div>
            </Card>
        );
    };

    return (
        <div>
            <div className="flex justify-content-between align-items-center mb-3">
                <h1 className="text-2xl font-bold m-0">{t('title')}</h1>
            </div>

            <Messages ref={msgs} />

            {/* Isolation Control */}
            <div className="grid mb-4">
                <div className="col-12">
                    <Card>
                        <div className="flex justify-content-between align-items-center">
                            <div className="flex align-items-center gap-3">
                                <div className={`flex align-items-center justify-content-center border-round w-4rem h-4rem ${staticData.pack1IsolationControl ? 'bg-green-100' : 'bg-gray-100'}`}>
                                    <i className={`pi pi-shield text-3xl ${staticData.pack1IsolationControl ? 'text-green-500' : 'text-gray-400'}`}></i>
                                </div>
                                <div>
                                    <h3 className="m-0 font-semibold">{t('isolationControl')}</h3>
                                    <span className="text-500 text-sm">{t('controlState')}</span>
                                </div>
                            </div>
                            <div className="flex align-items-center gap-3">
                                <Tag
                                    value={staticData.pack1IsolationControl ? t('enabled') : t('disabled')}
                                    severity={staticData.pack1IsolationControl ? 'success' : 'secondary'}
                                    className="text-sm px-3 py-1"
                                />
                                <InputSwitch checked={staticData.pack1IsolationControl} disabled />
                            </div>
                        </div>
                    </Card>
                </div>
            </div>

            {/* BCU States */}
            <h3 className="text-lg font-semibold mb-3">
                <i className="pi pi-server mr-2"></i>
                {t('bcuState')}
            </h3>
            <div className="grid">
                <div className="col-12 md:col-6 lg:col-3">
                    <BCUCard label={t('bcu01')} stateCode={staticData.bcu01StateCode} />
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <BCUCard label={t('bcu02')} stateCode={staticData.bcu02StateCode} />
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <BCUCard label={t('bcu03')} stateCode={staticData.bcu03StateCode} />
                </div>
                <div className="col-12 md:col-6 lg:col-3">
                    <BCUCard label={t('bcu04')} stateCode={staticData.bcu04StateCode} />
                </div>
            </div>

            {/* State Legend */}
            <Card className="mt-4">
                <h3 className="m-0 text-lg font-semibold mb-4">
                    <i className="pi pi-info-circle mr-2"></i>
                    {t('stateLegend')}
                </h3>
                <div className="grid">
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="101" severity="info" />
                            <span className="text-sm">{t('standby')}</span>
                        </div>
                    </div>
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="102" severity="warning" />
                            <span className="text-sm">{t('startup')}</span>
                        </div>
                    </div>
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="103" severity="warning" />
                            <span className="text-sm">{t('precharge')}</span>
                        </div>
                    </div>
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="104" severity="danger" />
                            <span className="text-sm">{t('discharge')}</span>
                        </div>
                    </div>
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="105" severity="success" />
                            <span className="text-sm">{t('charge')}</span>
                        </div>
                    </div>
                    <div className="col-6 md:col-4 lg:col-2">
                        <div className="flex align-items-center gap-2">
                            <Tag value="106" severity="secondary" />
                            <span className="text-sm">{t('shutdown')}</span>
                        </div>
                    </div>
                </div>
            </Card>

            <div className="text-right text-500 text-sm mt-3">
                Last updated: {formatDate(staticData.timestampUtc)}
            </div>
        </div>
    );
}
