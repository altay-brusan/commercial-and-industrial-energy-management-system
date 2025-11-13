'use client';
import React, { useEffect, useRef, useState } from 'react'
import { Card } from 'primereact/card';
import { useTranslations } from 'next-intl';
import { SelectButton } from 'primereact/selectbutton';
import InverterHeatChart from '@/src/components/inverters/InverterHeatChart';
import InverterLinesChart, { ChartData } from '@/src/components/inverters/InverterLinesChart';
import { useFetch } from '@/src/hooks/use-fetch';
import { graphsService } from '@/src/services/graphs';
import webSocketService from '@/src/services/WebSocketService';
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData';
import { useTenant } from '@/src/context/TenantContext';
import { subDays, setHours, setMinutes } from 'date-fns';
import { formatDateISO } from '@/src/utils/dateFormatTR';
import { tryCatch } from '@/src/utils/tryCatch';
import { scadaService } from '@/src/services/scada';
import { useLoading } from '@/src/context/LoadingContext';

export default function InvertersPage() {
    const t = useTranslations();
    const [data, setData] = useState<DashboardRealTimeData>();
    const [value, setValue] = useState<number[]>([1, 2, 3]);
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<(() => void) | null>(null);
    const { startDate, tenant } = useTenant();
    const { showLoading, hideLoading, setIsLoading } = useLoading();
    const previousDay = startDate === null ? formatDateISO(setMinutes(setHours(new Date(), 0), 0)) : setMinutes(setHours(subDays(startDate, 1), 3), 0);

    const pcsId = data?.pcsDevices[0]?.deviceId;
    const [inverterData] = useFetch<any>(() => pcsId ? graphsService.getInverterGraphData(pcsId, previousDay, startDate ? formatDateISO(startDate) as any : '') : Promise.resolve([]), { deps: [pcsId, startDate], reloadDeps: [pcsId, startDate], setLoading: setIsLoading });

    const items = data?.pcsDevices.map((device, index) => ({
        name: `Inverter ${index + 1}`,
        value: device.deviceId
    }));

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
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [startDate]);

    const formatChartData = (data: any[], field: string): ChartData | undefined => {
        if (!data || data.length === 0) return undefined;

        return {
            series: [{
                name: t(`inverter.${field}`),
                data: data.map(item => item[field])
            }],
            categories: data.map(item => new Date(item.time).toLocaleTimeString('tr-TR', {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false
            }))
        };
    };

    return (
        <div className='grid'>
            <div className='col-12'>
                <Card>
                    <div className='flex justify-content-between align-items-center'>
                        <h2 className='text-3xl font-bold mb-0'>{t("inverters.title")}</h2>
                        <div className='flex flex-column align-items-start gap-2'>
                            <span className='text-sm text-gray-500'>{t("inverters.selectInverter")}</span>
                            <SelectButton value={value} onChange={(e) => setValue(e.value)} optionLabel="name" options={items} multiple />
                        </div>
                    </div>
                </Card>
                {/* <Card className='my-4'>
                    <InverterHeatChart
                        data={[]}
                        className="mt-4"
                    />
                </Card> */}
            </div>
            <div className="col-12">
                <div className="grid">
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="Active Power"
                                chartId="activePower"
                                data={formatChartData(inverterData, 'activePower')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="Reactive Power"
                                chartId="reactivePower"
                                data={formatChartData(inverterData, 'reactivePower')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="Power Factor"
                                chartId="powerFactor"
                                data={formatChartData(inverterData, 'powerFactor')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="AC Voltage"
                                chartId="acVoltage"
                                data={formatChartData(inverterData, 'acVoltage')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="AC Current"
                                chartId="acCurrent"
                                data={formatChartData(inverterData, 'acCurrent')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="DC Voltage"
                                chartId="dcVoltage"
                                data={formatChartData(inverterData, 'dcVoltage')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="DC Current"
                                chartId="dcCurrent"
                                data={formatChartData(inverterData, 'dcCurrent')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="DC Power"
                                chartId="dcPower"
                                data={formatChartData(inverterData, 'dcPower')}
                            />
                        </Card>
                    </div>
                    <div className="col-12 md:col-6">
                        <Card>
                            <InverterLinesChart
                                title="Temperature"
                                chartId="temperature"
                                data={formatChartData(inverterData, 'temperature')}
                            />
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    )
}
