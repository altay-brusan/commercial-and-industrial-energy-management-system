/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import React, { useState, useEffect, useRef, useMemo } from 'react'
import { Card } from 'primereact/card';
import { useTranslations } from 'next-intl';
import { SelectButton } from 'primereact/selectbutton';
import TotalRealTime from '@/src/components/solar/TotalRealTime';
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData';
import webSocketService from '@/src/services/WebSocketService';
import { useFetch } from '@/src/hooks/use-fetch';
import { graphsService } from '@/src/services/graphs';
import { useTenant } from '@/src/context/TenantContext';
import { subDays, setHours, setMinutes } from 'date-fns';
import { formatDateISO } from '@/src/utils/dateFormatTR';
import PowerGeneration from '@/src/components/inverters/PowerGeneration';
import { tryCatch } from '@/src/utils/tryCatch';
import { scadaService } from '@/src/services/scada';
import { useLoading } from '@/src/context/LoadingContext';

export default function SolarPage() {
    const t = useTranslations();
    const [value, setValue] = useState([1, 2, 3]);
    const [data, setData] = useState<DashboardRealTimeData>();
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<(() => void) | null>(null);
    const { startDate, tenant } = useTenant();
    const { setIsLoading } = useLoading();
    const previousDay = startDate === null ? formatDateISO(setMinutes(setHours(new Date(), 0), 0)) : setMinutes(setHours(subDays(startDate, 1), 3), 0);

    const solarId = data?.solarDevices[0]?.deviceId;

    const [energyProduction] = useFetch<any>(() => solarId ? graphsService.getEnergyProduction(solarId, previousDay, startDate ? formatDateISO(startDate) as any : '') : Promise.resolve([]), { deps: [solarId, startDate], reloadDeps: [solarId, startDate], setLoading: setIsLoading });
    const [powerProduction] = useFetch<any>(() => solarId ? graphsService.getPowerProduction(solarId, previousDay, startDate ? formatDateISO(startDate) as any : '') : Promise.resolve([]), { deps: [solarId, startDate], reloadDeps: [solarId, startDate], setLoading: setIsLoading });

    const formattedEnergyProduction = useMemo(() =>
        energyProduction?.dataPoints?.map((point: any) => ({
            time: new Date(point.time).toLocaleTimeString('tr-TR', {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false
            }),
            prediction: point.prediction || 0,
            production: point.production || 0
        }))?.sort((a: { time: string }, b: { time: string }) => {
            const timeA = new Date(a.time).getTime();
            const timeB = new Date(b.time).getTime();
            return timeA - timeB;
        }) || [],
        [energyProduction]
    );

    const formattedPowerProduction = useMemo(() =>
        powerProduction?.dataPoints?.map((point: any) => ({
            time: new Date(point.time).toLocaleTimeString('tr-TR', {
                hour: '2-digit',
                minute: '2-digit',
                hour12: false
            }),
            prediction: point.prediction || 0,
            production: point.production || 0
        }))?.sort((a: { time: string }, b: { time: string }) => {
            const timeA = new Date(a.time).getTime();
            const timeB = new Date(b.time).getTime();
            return timeA - timeB;
        }) || [],
        [powerProduction]
    );

    const getScadaData = async () => {
        const [error, result] = await tryCatch(scadaService.getScadaData(tenant, startDate));
        if (error) throw error;
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
    }, [startDate]);

    return (
        <div className='grid'>
            <div className='col-12'>
                <Card>
                    <h2 className='text-3xl font-bold mb-0'>{t("solar.title")}</h2>
                    <div className='flex flex-column align-items-start gap-2 mt-4'>
                        <span className='text-sm text-gray-500'>{t("solar.selectSolar")}</span>
                        <SelectButton value={value} onChange={(e) => setValue(e.value)} optionLabel="name" options={data?.solarDevices?.map((device, index) => ({ name: `Solar ${index + 1}`, value: device.deviceId })) || []} multiple />
                    </div>
                </Card>
                <div className="grid my-4">
                    <div className="col-12 md:col-4">
                        <Card>
                            <div className="flex flex-column justify-content-between align-items-center">
                                <h3 className="text-2xl font-bold mb-0">{t("solar.totalPerformance")}</h3>
                                <span className='text-2xl text-gray-500'>%{data?.solarDevices?.length ? (data.solarDevices.map(device => device.performanceRatio).reduce((a, b) => a + b, 0) / data.solarDevices.length)?.toFixed(2) : '0.00'}</span>
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-4">
                        <Card>
                            <div className="flex flex-column justify-content-between align-items-center">
                                <h3 className="text-2xl font-bold mb-0">{t("solar.energyProducedToday")}</h3>
                                <span className='text-2xl text-gray-500'>{(data?.solarDevices?.length ? data.solarDevices.map(device => device.dcOutput.totalEnergyDaily).reduce((a, b) => a + b, 0) : 0)?.toFixed(2)} kWh</span>
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-4">
                        <Card>
                            <div className="flex flex-column justify-content-between align-items-center">
                                <h3 className="text-2xl font-bold mb-0">{t("solar.weather")}</h3>
                                <span className='text-2xl text-gray-500'>{data?.solarDevices[0]?.weather?.toFixed(2)} °C</span>
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-4">
                        <Card>
                            <div className="flex flex-column justify-content-between align-items-center">
                                <h3 className="text-2xl font-bold mb-0">{t("solar.irradiance")}</h3>
                                <span className='text-2xl text-gray-500'>{data?.solarDevices[0]?.irradiance?.toFixed(2)}
                                    <span className='text-sm text-gray-500'>Wh/m²</span>
                                </span>
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 md:col-4">
                        <Card>
                            <div className="flex flex-column justify-content-between align-items-center">
                                <h3 className="text-2xl font-bold mb-0">{t("solar.alarms")}</h3>
                                <span className='text-2xl text-gray-500'>
                                    {data?.solarDevices?.length && data.solarDevices.some(device => device.status?.faultMessages?.length > 0 || device.status?.warningMessages?.length > 0)
                                        ? t("solar.hasAlarms")
                                        : t("solar.noAlarms")}
                                </span>
                            </div>
                        </Card>
                    </div>
                </div>
            </div>
            <div className="col-12">
                <Card>
                    <h3 className="text-xl font-bold mb-0">Energy Production</h3>
                    <TotalRealTime data={formattedEnergyProduction} />
                </Card>
            </div>
            <div className="col-12">
                <div className="grid">
                    <div className="col-12">
                        <Card>
                            <PowerGeneration
                                title="Power Generation"
                                chartId="powerGeneration"
                                height="480px"
                                data={formattedPowerProduction}
                            />
                        </Card>
                    </div>
                    {/* <div className="col-6">
                        <Card>
                            <h3 className="text-xl font-bold mb-0">{t("solar.weatherBasedTotalSolarGenerationPrediction")}</h3>
                            <TotalRealTime data={weatherBasedData} />
                        </Card>
                    </div> */}
                </div>
            </div>
        </div>
    )
}
