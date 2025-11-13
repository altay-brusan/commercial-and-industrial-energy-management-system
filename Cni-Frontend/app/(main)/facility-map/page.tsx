/* eslint-disable react-hooks/exhaustive-deps */
'use client';

import { useState, useCallback, useRef, useEffect, useMemo, useContext } from 'react';
import { GoogleMap, LoadScript, InfoWindow } from '@react-google-maps/api';
import { Tooltip } from 'primereact/tooltip';
import { ProgressSpinner } from 'primereact/progressspinner';
import webSocketService from '@/src/services/WebSocketService';
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData';

import { Facility } from '../../../src/types/facility';
import FacilityMarker from '../../../src/components/FacilityMarker';
import { MeterGroup } from 'primereact/metergroup';
import { COLORS } from '@/src/utils/color';
import { useTranslations } from 'next-intl';
import { Chart } from 'primereact/chart';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { Card } from 'primereact/card';
import { FaThermometerFull } from 'react-icons/fa';
import { capitalizeFirstLetter } from '@/src/utils/helper';
import { LayoutContext } from '@/layout/context/layoutcontext';
import { IoIosNotifications } from "react-icons/io";
import Link from 'next/link';
import { PaginatedList } from '@/src/types/common';
import { useFetch } from '@/src/hooks/use-fetch';
import { getFacilities } from '@/src/services/facility';
import { useTenant } from '@/src/context/TenantContext';
import { scadaService } from '@/src/services/scada';
import { tryCatch } from '@/src/utils/tryCatch';
import { useLoading } from '@/src/context/LoadingContext';
import { formatCurrency } from '@/src/utils/formatters';


const containerStyle = {
    width: '100%',
    height: '100%'
};

const defaultCenter = {
    lat: 40.7545,
    lng: 29.8206
};

const FacilityMapPage = () => {
    const [loading, setLoading] = useState<boolean>(true);
    const [selectedFacility, setSelectedFacility] = useState<Facility | null>(null);
    const [mapCenter, setMapCenter] = useState(defaultCenter);
    const [mapZoom, setMapZoom] = useState(10);
    const [data, setData] = useState<DashboardRealTimeData>();
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const connectionRef = useRef<(() => void) | null>(null);
    const mapRef = useRef<google.maps.Map | null>(null);
    const t = useTranslations();
    const { onMenuToggle } = useContext(LayoutContext);
    const { startDate, tenant } = useTenant();
    const { showLoading, hideLoading } = useLoading();

    const [facilityList] = useFetch<PaginatedList<Facility>>(() => getFacilities(1, 100000), { setLoading });

    const getScadaData = async () => {
        showLoading();
        const [error, result] = await tryCatch(scadaService.getScadaData(tenant, startDate));
        if (error) {
            hideLoading()
            throw error;
        }
        hideLoading();
        return result;
    }

    useEffect(() => {
        onMenuToggle();

        return () => {
            onMenuToggle();
        }
    }, []);

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
                            console.log("🚀 ~ unsubscribe ~ data:", data)
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

    useEffect(() => {
        if (facilityList?.items?.length) {
            calculateMapBounds();
        }
    }, [facilityList]);

    const handleMapLoad = useCallback((map: google.maps.Map) => {
        mapRef.current = map;
    }, []);

    const handleMarkerClick = useCallback((facility: Facility) => {
        setSelectedFacility(facility);
    }, []);

    const handleInfoWindowClose = useCallback(() => {
        setSelectedFacility(null);
    }, []);

    const calculateMapBounds = useCallback(() => {
        if (!facilityList?.items || !facilityList.items.length || !window.google) return;

        if (facilityList.items.length === 1) {
            const facility = facilityList.items[0];
            setMapCenter({ lat: facility.latitude, lng: facility.longitude });
            setMapZoom(15);
            return;
        }

        const bounds = new window.google.maps.LatLngBounds();

        facilityList.items.forEach(facility => {
            bounds.extend({ lat: facility.latitude, lng: facility.longitude });
        });

        const center = bounds.getCenter();
        setMapCenter({ lat: center.lat(), lng: center.lng() });
        setMapZoom(8);

        if (mapRef.current) {
            mapRef.current.fitBounds(bounds);
        }
    }, [facilityList]);

    const values = [
        { label: t('dashboard.grid'), color: COLORS.GRID, value: Number(data?.systemData?.loadGridContribution) },
        { label: t('dashboard.bess'), color: COLORS.BESS, value: Number(data?.systemData?.loadPcsContribution) },
        { label: t('dashboard.solar'), color: COLORS.SOLAR, value: Number(data?.systemData?.loadSolarContribution) },
        { label: t('dashboard.generator'), color: COLORS.GENERATOR, value: Number(data?.systemData?.loadGeneratorContribution) }
    ];

    const energySourceData = {
        labels: ['BESS', 'SOLAR', 'GENERATOR', 'GRID'],
        datasets: [{
            data: [
                Math.abs(Number(data?.systemData?.pcsPower)),
                Number(data?.systemData?.solarPower),
                Number(data?.systemData?.generatorPower),
                Number(data?.systemData?.gridPower)
            ].map(value => {
                const total = Math.abs(Number(data?.systemData?.pcsPower)) +
                    Number(data?.systemData?.solarPower) +
                    Number(data?.systemData?.generatorPower) +
                    Number(data?.systemData?.gridPower);
                return total > 0 ? (value / total) * 100 : 0;
            }),
            backgroundColor: [COLORS.BESS, COLORS.SOLAR, COLORS.GENERATOR, COLORS.GRID],
            borderWidth: 0
        }]
    };


    const centerTextPlugin = {
        id: 'centerText',
        beforeDraw: function (chart: any) {
            const width = chart.width;
            const height = chart.height;
            const ctx = chart.ctx;

            ctx.restore();
            ctx.font = 'bold 20px Arial';
            ctx.fillStyle = '#495057';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            const text = [
                Math.abs(Number(data?.systemData?.pcsPower)),
                Number(data?.systemData?.solarPower),
                Number(data?.systemData?.generatorPower),
                Number(data?.systemData?.gridPower)
            ].reduce((acc, curr) => acc + curr, 0).toFixed(2) + " kW";
            const textX = window.innerWidth < 768 ? width / 2 : width / 3.5;
            const textY = window.innerWidth < 768 ? height / 2.2 : height / 2;
            ctx.fillText(text, textX, textY);
            ctx.save();
        }
    };

    const energySourceOptions = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        cutout: '60%',
        plugins: {
            legend: {
                display: true,
                position: window.innerWidth < 768 ? 'bottom' : 'right',
                align: 'center',
                labels: {
                    padding: window.innerWidth < 768 ? 10 : 20,
                    usePointStyle: true,
                    pointStyle: 'circle',
                    font: {
                        size: window.innerWidth < 768 ? 12 : 14,
                        weight: 'bold'
                    },
                    boxWidth: window.innerWidth < 768 ? 8 : 10
                }
            },
            tooltip: {
                enabled: true,
                callbacks: {
                    label: function (context: any) {
                        return `${context.label}: ${Number(context.raw).toFixed(2)}`;
                    }
                }
            },
            datalabels: {
                color: '#000',
                font: {
                    weight: 'bold',
                    size: window.innerWidth < 768 ? 12 : 14
                },
                anchor: 'center',
                align: 'center',
                offset: window.innerWidth < 768 ? 5 : 10,
                padding: window.innerWidth < 768 ? 4 : 6,
                display: true,
                formatter: function (value: number) {
                    return `${Number(value).toFixed(2)}`;
                }
            }
        }
    }), []);


    return (
        <div className="w-full">
            {loading ? (
                <div className="flex justify-content-center align-items-center" style={{ height: '85vh' }}>
                    <ProgressSpinner style={{ width: '50px', height: '50px' }} strokeWidth="4" animationDuration=".5s" />
                </div>
            ) : (
                <div className='grid'>
                    <div className='col-12 lg:col-3 flex flex-column'>
                        <Card className='flex-1 mb-3'>
                            <div className="flex justify-content-between mb-3 pt-3 px-3">
                                <div>
                                    <span className="block text-500 font-medium mb-3">{t('dashboard.totalEnergyUsageToday')}</span>
                                    <div className="text-900 font-medium text-xl">{Number(data?.facilityData?.acPower?.totalEnergyDaily).toFixed(2)} kWh</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-blue-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-bolt text-blue-500 text-xl" />
                                </div>
                            </div>
                            <div className='px-3'>
                                <MeterGroup values={values} />
                                <div className="flex mt-4 mb-2 w-full font-bold text-xl">
                                    <span className='px-2 py-2 w-6'>{t("dashboard.activePower")}: {Number(data?.facilityData?.acPower?.totalActivePower).toFixed(2)} kW</span>
                                    <span className='px-2 py-2 w-6'>{t("dashboard.reactivePower")}: {Number(data?.facilityData?.acPower?.totalReactivePower).toFixed(2)} kW</span>
                                </div>
                            </div>
                        </Card>
                        <Card className='flex-1 mb-3' title={t('dashboard.currentEnergySource')}>
                            <div className="flex justify-content-center align-items-center h-16rem">
                                <Chart
                                    height={window.innerWidth < 768 ? '200px' : '260px'}
                                    type="doughnut"
                                    data={energySourceData}
                                    options={energySourceOptions}
                                    plugins={[ChartDataLabels, centerTextPlugin]}
                                />
                            </div>
                        </Card>
                        <Card className='flex-1' title={t('dashboard.pcsState')}>
                            <div className="flex justify-content-between mb-3">
                                <div className='flex flex-column xl:flex-row gap-2 xl:gap-0 justify-content-between w-full'>
                                    <div>
                                        <div className="text-900 font-medium text-3xl flex gap-4">
                                            {t('dashboard.onGrid')}
                                            <i className="pi pi-wifi text-purple-500 text-3xl" />
                                        </div>
                                        <p className='text-red-500'>{t("BESS.temperature")}:<span className='font-bold'> <FaThermometerFull /> {Number(data?.pcsDevices?.[0]?.temperaturePcs).toFixed(2)} °C</span></p>
                                    </div>
                                    <div>
                                        <p className='text-xl font-bold'>{t("dashboard.load")} {Number(data?.pcsDevices?.[0]?.acPower?.totalActivePower).toFixed(2)} kW</p>
                                        <p className='text-xl font-bold'>{t("dashboard.efficiency")} {Number(data?.pcsDevices?.[0]?.efficiency).toFixed(2)}%</p>
                                    </div>
                                </div>
                            </div>
                        </Card>
                    </div>
                    <div className="col-12 lg:col-6 lg:h-auto" style={{ height: '50rem' }}>
                        <Card className="border-none shadow-1 padding-0 md:h-full map-card h-full">
                            <LoadScript googleMapsApiKey={"***REMOVED***"}>
                                <GoogleMap
                                    mapContainerStyle={containerStyle}
                                    center={mapCenter}
                                    zoom={mapZoom}
                                    onLoad={(map) => {
                                        handleMapLoad(map);
                                        if (facilityList?.items && facilityList.items.length > 1 && map && window.google) {
                                            const bounds = new window.google.maps.LatLngBounds();
                                            facilityList.items.forEach(facility => {
                                                bounds.extend({ lat: facility.latitude, lng: facility.longitude });
                                            });
                                            map.fitBounds(bounds);
                                        }
                                    }}
                                    options={{
                                        mapTypeControl: false,
                                        streetViewControl: false,
                                        styles: [
                                            {
                                                featureType: 'administrative',
                                                elementType: 'geometry',
                                                stylers: [{ visibility: 'simplified' }]
                                            },
                                            {
                                                featureType: 'water',
                                                elementType: 'geometry',
                                                stylers: [{ color: '#b7d5e8' }]
                                            },
                                            {
                                                featureType: 'landscape',
                                                elementType: 'geometry',
                                                stylers: [{ color: '#e8f4dc' }]
                                            }
                                        ]
                                    }}
                                >
                                    {facilityList?.items?.map((facility) => (
                                        <FacilityMarker
                                            key={facility.id}
                                            facility={facility}
                                            onClick={handleMarkerClick}
                                        />
                                    ))}

                                    {selectedFacility && (
                                        <InfoWindow
                                            position={{ lat: selectedFacility.latitude, lng: selectedFacility.longitude }}
                                            onCloseClick={handleInfoWindowClose}
                                        >
                                            <div className="w-full">
                                                <div className="border-none shadow-none w-full p-0">
                                                    <div className="text-lg font-bold mb-2 flex align-items-center justify-content-between">{selectedFacility.name} {selectedFacility.status === 'inactive' && (
                                                        <span
                                                            className="cursor-pointer"
                                                            data-pr-tooltip="Bu tesis şu anda aktif değil"
                                                            data-pr-position="right"
                                                        >
                                                            <IoIosNotifications size={20} color='red' />
                                                            <Tooltip target=".cursor-pointer" />
                                                        </span>
                                                    )}</div>
                                                    <hr />
                                                    <div className="flex justify-content-between align-items-center mb-2 font-bold">
                                                        <div className="flex align-items-center">
                                                            <div className="mr-2">On Grid</div>
                                                            <i className="pi pi-wifi text-primary" style={{ fontSize: '1rem' }}></i>
                                                        </div>
                                                        <div>Efficiency %{selectedFacility.efficiency || Number(data?.pcsDevices?.[0]?.efficiency).toFixed(2)}</div>
                                                    </div>

                                                    <div className="mb-3 text-sm">{selectedFacility.address || 'İstanbul Kartal, Mustafa Kemal cad. Hukukçular Towers A Blok'}</div>
                                                    <hr />
                                                    <div className="flex justify-content-between gap-3">
                                                        <Link href={`/facility/${selectedFacility.id}`} className='text-primary font-bold' >Historical Data</Link>
                                                        <Link href={`/facility/${selectedFacility.id}`} className='text-primary font-bold' >Control</Link>
                                                        <Link href={`/facility/${selectedFacility.id}`} className='text-primary font-bold' >Detail</Link>
                                                    </div>
                                                </div>
                                            </div>
                                        </InfoWindow>
                                    )}
                                </GoogleMap>
                            </LoadScript>
                        </Card>
                    </div>
                    <div className='col-12 lg:col-3 flex flex-column'>
                        <Card className='flex-1 mb-3'>
                            <div className="flex justify-content-between mb-3">
                                <div>
                                    <span className="block text-500 font-medium mb-3">{t('dashboard.solarCapacity')}</span>
                                    <div className="text-900 font-medium text-xl">{Number(data?.solarDevices?.[0]?.panelPower?.power).toFixed(2)} kW</div>
                                </div>
                                <div className="flex align-items-center justify-content-center bg-cyan-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className="pi pi-sun text-cyan-500 text-xl" />
                                </div>
                            </div>
                            <div className='text-500'>
                                <p>{t("dashboard.maintenanceStatus")}: <span className='font-bold ml-2'>{capitalizeFirstLetter(t("common.required"))}</span></p>
                                <p className='text-red-500'>{t("dashboard.panelTemperature")}<span className='font-bold'> <FaThermometerFull /> {Number(data?.solarDevices?.[0]?.pvPanelTemperature).toFixed(2)} °C</span></p>
                            </div>
                        </Card>
                        <Card className='flex-1 mb-3' title={t('dashboard.bessStateOfCharge')}>
                            <div className='flex flex-column gap-2'>
                                <Tooltip target=".tooltip" mouseTrack mouseTrackLeft={10} />
                                {data?.bessDevices?.map((bess, index) => (
                                    <div
                                        key={bess.deviceId}
                                        data-pr-tooltip={bess.chargeStatus === 1 ? "Charging" : "Discharging"}
                                        className="tooltip text-900 font-medium text-xl border-1 p-3 border-round border-gray-300 mb-2 w-full flex justify-content-between"
                                    >
                                        <span className='font-bold text-500'>
                                            BESS-{index + 1} :
                                            <i className={`pi pi-circle-fill ${bess.chargeStatus === 1 ? 'text-green-500' : 'text-red-500'}`}></i>
                                        </span>
                                        {Number(bess.stateOfCharge).toFixed(2)}%
                                    </div>
                                ))}
                            </div>
                        </Card>
                        <Card className='flex-1 mb-3' title={t('dashboard.dailyEnergyCost')}>
                            <div className="flex flex-column gap-2">
                                <div className="surface-card p-4 border-round shadow-1">
                                    <div className="flex justify-content-between align-items-center mb-3">
                                        <span className="text-600 text-xl">{t("dashboard.totalCost")}</span>
                                        <i className="pi pi-bolt text-primary text-2xl"></i>
                                    </div>
                                    <div className="text-900 text-4xl font-bold">{formatCurrency(data?.systemData?.grid_Consumption_TL_Daily ?? 0)}</div>
                                </div>

                                <div className="surface-card p-4 border-round shadow-1">
                                    <div className="flex justify-content-between align-items-center mb-3">
                                        <span className="text-600 text-xl">{t("dashboard.youSaved")}</span>
                                        <i className="pi pi-wallet text-green-500 text-2xl"></i>
                                    </div>
                                    <div className="text-green-500 text-4xl font-bold">{formatCurrency(data?.systemData?.cost_Saving_TL_Daily ?? 0)}</div>
                                    <div className="text-600 text-sm mt-2">
                                        <span>{t("dashboard.today")}</span>
                                    </div>
                                </div>
                            </div>
                        </Card>
                        <Card className='flex-1' title={t('dashboard.dailyAvoidedCO2Emission')}>
                            <div className="w-full text-left text-6xl font-bold mt-4">
                                19.12 <span className='text-sm'>tons this month</span>
                            </div>
                        </Card>
                    </div>
                </div>
            )
            }
        </div >
    );
};

export default FacilityMapPage;
