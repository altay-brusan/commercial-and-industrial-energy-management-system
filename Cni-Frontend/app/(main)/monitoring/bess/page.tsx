/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import React, { useEffect, useRef, useState } from 'react';
import BESSCard from '../bess/components/BESSCard';
import { useTranslations } from 'next-intl';
import { Messages } from 'primereact/messages';
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData';
import webSocketService from '@/src/services/WebSocketService';
import { useTenant } from '@/src/context/TenantContext';
import { tryCatch } from '@/src/utils/tryCatch';
import { scadaService } from '@/src/services/scada';
import { useLoading } from '@/src/context/LoadingContext';

export default function BessControlPage() {
    const t = useTranslations('BESS');
    const msgs = useRef<Messages>(null);
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const [data, setData] = useState<DashboardRealTimeData>();
    const { startDate, tenant } = useTenant();
    const connectionRef = useRef<(() => void) | null>(null);
    const { showLoading, hideLoading } = useLoading();

    const getScadaData = async () => {
        showLoading();
        const [error, result] = await tryCatch(scadaService.getScadaData(tenant, startDate));
        if (error) {
            hideLoading();
            throw error;
        }
        hideLoading();
        return result;
    }

    useEffect(() => {
        if (msgs.current) {
            msgs.current.clear();
            msgs.current.show([
                { sticky: true, severity: 'info', detail: t('description'), closable: false },
            ]);
        }
    }, [t]);

    useEffect(() => {
        const SIGNALR_HUB_URL = 'https://derms-api.n2nenergy.com/liveDataHub';
        let isSubscribed = true;

        const connectToSignalR = async () => {
            try {
                if (!webSocketService.isConnected()) {
                    await webSocketService.connect(SIGNALR_HUB_URL);
                }

                if (isSubscribed) {
                    setIsConnected(webSocketService.isConnected());

                    const unsubscribe = await webSocketService.subscribe<DashboardRealTimeData>('FacilityUpdate', (data) => {
                        if (isSubscribed) {
                            setData(data);
                        }
                    });

                    connectionRef.current = unsubscribe;

                    await webSocketService.invoke('SubscribeToFacility', 6);
                }
            } catch (error) {
                console.error('SignalR bağlantı hatası:', error);
                if (isSubscribed) {
                    setIsConnected(false);
                }
            }
        };

        const fetchData = async () => {
            if (!startDate) {
                await connectToSignalR();
            } else {
                try {
                    const scadaData = await getScadaData();
                    setData(scadaData);
                } catch (error) {
                    console.error('Scada veri çekme hatası:', error);
                }
            }
        };

        fetchData();

        const connectionInterval = setInterval(() => {
            if (isSubscribed && !startDate) {
                const connected = webSocketService.isConnected();
                setIsConnected(connected);

                if (!connected) {
                    connectToSignalR();
                }
            }
        }, 5000);

        return () => {
            isSubscribed = false;
            if (connectionRef.current) {
                connectionRef.current();
                connectionRef.current = null;
            }
            clearInterval(connectionInterval);
        };
    }, [startDate]);

    return (
        <div >
            <h1 className="text-2xl font-bold m-0">{t('title')}</h1>
            <Messages ref={msgs} />
            <div className="grid">
                {data?.bessDevices?.map((bess, index) => (
                    <div className="col-12 lg:col-6 xl:col-4" key={index}>
                        <BESSCard data={bess} index={index} />
                    </div>
                ))}
            </div>
        </div>
    );
}
