/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable @next/next/no-img-element */
'use client';
import { Chart } from 'primereact/chart';
import React, { useContext, useEffect, useRef, useState, useMemo } from 'react';
import webSocketService from '@/src/services/WebSocketService';
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { LayoutContext } from '../../layout/context/layoutcontext';
import Card from '@/src/components/Card';
import { useTranslations } from 'next-intl';
import { FaThermometerFull } from 'react-icons/fa';
import { MeterGroup } from 'primereact/metergroup';
import { Tooltip } from 'primereact/tooltip';
import { COLORS } from '@/src/utils/color';
import { capitalizeFirstLetter } from '@/src/utils/helper';
import { LayoutConfig } from '@/src/types';
import { formatCurrency } from '@/src/utils/formatters';
import { useTenant } from '@/src/context/TenantContext';
import { scadaService } from '@/src/services/scada';
import { tryCatch } from '@/src/utils/tryCatch';
import { useLoading } from '@/src/context/LoadingContext';

const Dashboard = () => {
    const { layoutConfig, setLayoutConfig } = useContext(LayoutContext);
    const t = useTranslations();
    const [data, setData] = useState<DashboardRealTimeData>();
    const [isConnected, setIsConnected] = useState<boolean>(false);
    const connectionRef = useRef<(() => void) | null>(null);
    const { startDate, tenant } = useTenant();
    const { showLoading, hideLoading } = useLoading();

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
        setLayoutConfig((prevState: LayoutConfig) => ({ ...prevState, isFullScreen: true }));
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
                            setData(data);
                            console.log("🚀 ~ connectToSignalR ~ data:", data)
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

        // Bağlantı durumunu kontrol et (sadece SignalR için)
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
    }, [startDate]); // startDate değiştiğinde useEffect'i tekrar çalıştır

    const values = [
        { label: t('dashboard.grid'), color: COLORS.GRID, value: Number(data?.systemData?.loadGridContribution) },
        { label: t('dashboard.bess'), color: COLORS.BESS, value: Number(data?.systemData?.loadPcsContribution) },
        { label: t('dashboard.solar'), color: COLORS.SOLAR, value: Number(data?.systemData?.loadSolarContribution) },
        { label: t('dashboard.generator'), color: COLORS.GENERATOR, value: Number(data?.systemData?.loadGeneratorContribution) }
    ];

    const getEfficiencyColor = (value: number) => {
        if (value >= 95) return '#22C55E';
        if (value >= 85) return '#F97316';
        return '#EF4444';
    };

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
            ctx.fillStyle = layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef';
            ctx.textBaseline = 'middle';
            ctx.textAlign = 'center';
            const text = [
                Math.abs(Number(data?.systemData?.pcsPower)),
                Number(data?.systemData?.solarPower),
                Number(data?.systemData?.generatorPower),
                Number(data?.systemData?.gridPower)
            ].reduce((acc, curr) => acc + curr, 0).toFixed(1) + " kW";
            const textX = window.innerWidth < 768 ? width / 2 : width / 2;
            const textY = window.innerWidth < 768 ? height / 2.2 : height / 2.2;
            ctx.fillText(text, textX, textY);
            ctx.save();
        }
    };

    const energySourceOptions = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        cutout: '55%',
        plugins: {
            legend: {
                display: true,
                position: 'bottom',
                align: 'center',
                labels: {
                    usePointStyle: true,
                    pointStyle: 'circle',
                    font: {
                        size: 12,
                    },
                }
            },
            tooltip: {
                enabled: true,
                callbacks: {
                    label: function (context: any) {
                        return `${context.label}: ${Number(context.raw).toFixed(1)}`;
                    }
                }
            },
            datalabels: {
                color: '#fff',
                font: {
                    weight: 'bold',
                    size: window.innerWidth < 768 ? 12 : 12
                },
                anchor: 'center',
                align: 'center',
                offset: window.innerWidth < 768 ? 5 : 10,
                display: true,
                formatter: function (value: number) {
                    return `${Number(value).toFixed(1)}%`;
                },
                backgroundColor: 'rgba(0, 0, 0, 0.7)',
                borderRadius: 2,
                padding: { top: 2, bottom: 2, left: 2, right: 2 }
            }
        }
    }), []);

    const dailyUsageData = {
        labels: ['Thursday', 'Friday', 'Saturday', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Today'],
        datasets: [{
            label: 'Daily Usage (*100 kWh)',
            data: [6.5, 11, 3, 6, 10, 10.5, 6.5, 5],
            backgroundColor: COLORS.GRID,
            borderColor: COLORS.SOLAR,
            borderWidth: 0
        }]
    };

    const dailyUsageOptions = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: {
            legend: {
                labels: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 12 : 14
                    },
                    boxWidth: window.innerWidth < 768 ? 8 : 10
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    color: layoutConfig.colorScheme === 'light' ? '#ebedef' : 'rgba(160, 167, 181, .3)'
                }
            },
            x: {
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    display: false
                }
            }
        }
    }), [layoutConfig.colorScheme]);

    const costSavingsData = {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [{
            label: 'Monthly Savings (TL)',
            data: [10000, 22000, 12000, 25000, 35000, 30000, 28000, 26000, 38000, 35000, 32000, 30000],
            backgroundColor: COLORS.GRID,
            borderWidth: 0
        }]
    };

    const co2EmissionData = useMemo(() => ({
        labels: ['Thursday', 'Friday', 'Saturday', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Today'],
        datasets: [{
            label: 'Daily Avoided CO\u2082 Emission (%)',
            data: [15, 22, 40, 55, 12, 18, 26, 15],
            backgroundColor: COLORS.GRID,
            borderWidth: 0
        }]
    }), []);

    const co2EmissionOptions = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: {
            legend: {
                labels: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 12 : 14
                    },
                    boxWidth: window.innerWidth < 768 ? 8 : 10
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                max: 60,
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    color: layoutConfig.colorScheme === 'light' ? '#ebedef' : 'rgba(160, 167, 181, .3)'
                }
            },
            x: {
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    display: false
                }
            }
        }
    }), [layoutConfig.colorScheme]);

    const dailyUsagePerSourceData = useMemo(() => ({
        labels: ['Thursday', 'Friday', 'Saturday', 'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Today'],
        datasets: [
            {
                label: 'BESS',
                data: [1000, 2000, 1500, 3000, 5000, 4000, 3500, 2000],
                backgroundColor: COLORS.BESS,
                borderWidth: 0,
                stack: 'stack1'
            },
            {
                label: 'PV',
                data: [2000, 15000, 8000, 9000, 15000, 12000, 7000, 19000],
                backgroundColor: COLORS.SOLAR,
                borderWidth: 0,
                stack: 'stack1'
            },
            {
                label: 'GRID',
                data: [3000, 5000, 3000, 14000, 17000, 18000, 19000, 7000],
                backgroundColor: COLORS.GRID,
                borderWidth: 0,
                stack: 'stack1'
            }
        ]
    }), []);

    const dailyUsagePerSourceOptions = useMemo(() => ({
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: {
            legend: {
                position: window.innerWidth < 768 ? 'bottom' : 'top',
                labels: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 12 : 14
                    },
                    boxWidth: window.innerWidth < 768 ? 8 : 10,
                    padding: window.innerWidth < 768 ? 10 : 20
                }
            },
            tooltip: {
                mode: 'index'
            }
        },
        scales: {
            y: {
                stacked: true,
                beginAtZero: true,
                max: 40000,
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    color: layoutConfig.colorScheme === 'light' ? '#ebedef' : 'rgba(160, 167, 181, .3)'
                }
            },
            x: {
                stacked: true,
                ticks: {
                    color: layoutConfig.colorScheme === 'light' ? '#495057' : '#ebedef',
                    font: {
                        size: window.innerWidth < 768 ? 10 : 12
                    }
                },
                grid: {
                    display: false
                }
            }
        }
    }), [layoutConfig.colorScheme]);

    return (
        <div className="grid">
            <div className="col-12 lg:col-6 xl:col-6">
                <Card title={t('dashboard.totalEnergyUsageToday')} contentClassName=''>
                    <div className="flex justify-content-between mb-3 pt-3 md:pt-6 px-3 md:px-4">
                        <div className="text-900 font-medium text-2xl md:text-3xl">{Number(data?.facilityData?.acPower?.totalEnergyDaily).toFixed(1)} kWh</div>
                        <div className="flex align-items-center justify-content-center bg-blue-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                            <i className="pi pi-bolt text-blue-500 text-xl" />
                        </div>
                    </div>
                    <div className='px-2 md:px-4'>
                        <MeterGroup values={values} />
                        <div className="flex mt-4 mb-2 md:mb-0 md:mt-6 w-full font-medium text-base md:text-lg font-semibold">
                            <span className='px-2 py-2 md:w-6'>{t("dashboard.activePower")}: {Number(data?.facilityData?.acPower?.totalActivePower).toFixed(1)} kW</span>
                            <span className='px-2 py-2 md:w-6'>{t("dashboard.reactivePower")}: {Number(data?.facilityData?.acPower?.totalReactivePower).toFixed(1)} kW</span>
                        </div>
                    </div>
                </Card>
            </div>
            <div className="col-12 lg:col-6 xl:col-3">
                <Card contentClassName='py-2' title={t('dashboard.currentEnergySource')}>
                    <div className="flex justify-content-center align-items-center p-2 md:p-0">
                        <Chart
                            height={window.innerWidth < 768 ? '200px' : '260px'}
                            type="doughnut"
                            data={energySourceData}
                            options={energySourceOptions}
                            plugins={[ChartDataLabels, centerTextPlugin]}
                        />
                    </div>
                </Card>
            </div>
            <div className="col-12 lg:col-6  xl:col-3">
                <Card contentClassName='p-2' title={t('dashboard.dailyEnergyCost')}>
                    <div className="flex flex-column gap-2">
                        <div className="surface-card p-4 border-round shadow-1">
                            <div className="flex justify-content-between align-items-center mb-3">
                                <span className="text-600 text-lg md:text-xl">{t("dashboard.totalCost")}</span>
                                <i className="pi pi-bolt text-primary text-2xl"></i>
                            </div>
                            <div className="text-900 text-2xl md:text-4xl font-semibold">{formatCurrency(data?.systemData?.grid_Consumption_TL_Daily ?? 0)}</div>
                        </div>

                        <div className="surface-card p-4 border-round shadow-1">
                            <div className="flex justify-content-between align-items-center mb-3">
                                <span className="text-600 text-lg md:text-xl">{t("dashboard.youSaved")}</span>
                                <i className="pi pi-wallet text-green-500 text-2xl"></i>
                            </div>
                            <div className="text-green-500 text-2xl md:text-4xl font-semibold">{formatCurrency(data?.systemData?.cost_Saving_TL_Daily ?? 0)}</div>
                            <div className="text-600 text-base md:text-lg mt-2">
                                <span>{t("dashboard.today")}</span>
                            </div>
                        </div>
                    </div>
                </Card>
            </div>

            <div className="col-12 lg:col-6 xl:col-3">
                <Card title={t('dashboard.bessStateOfCharge')} contentClassName='p-4'>
                    <div className='flex flex-column gap-2'>
                        <Tooltip target=".tooltip" mouseTrack mouseTrackLeft={10} />
                        {data?.bessDevices?.map((bess, index) => (
                            <div
                                key={bess.deviceId}
                                data-pr-tooltip={bess.chargeStatus === 1 ? "Charging" : "Discharging"}
                                className="tooltip text-900 font-medium text-lg md:text-xl border-1 p-3 border-round border-gray-300 mb-2 w-full flex justify-content-between"
                            >
                                <span className='font-semibold text-500'>
                                    BESS-{index + 1} :
                                    <i className={`pi pi-circle-fill ${bess.chargeStatus === 1 ? 'text-green-500' : 'text-red-500'}`}></i>
                                </span>
                                {Number(bess.stateOfCharge).toFixed(1)}%
                            </div>
                        ))}
                    </div>
                </Card>
            </div>
            <div className="col-12 lg:col-6 xl:col-3">
                <Card title={t('dashboard.pcsState')} contentClassName='p-4'>
                    <div className="flex justify-content-between mb-3 h-full">
                        <div className='flex flex-column justify-content-between h-full'>
                            <div className="text-900 font-medium text-lg md:text-xl flex gap-4">
                                {t('dashboard.onGrid')}
                                <i className="pi pi-wifi text-purple-500 text-xl" />
                            </div>
                            <p className='text-red-500 mb-1 text-base md:text-lg'>{t("BESS.temperature")}:<span className='font-semibold'> <FaThermometerFull /> {Number(data?.pcsDevices?.[0]?.temperaturePcs).toFixed(1)} °C</span></p>
                            <p className='text-lg md:text-xl font-semibold mb-1'>{t("dashboard.load")} {Number(data?.pcsDevices?.[0]?.acPower?.totalActivePower).toFixed(1)} kW</p>

                            <div className="flex flex-column gap-2">
                                <MeterGroup
                                    className='w-full'
                                    labelPosition='start'
                                    values={[{
                                        label: t('dashboard.efficiency'),
                                        value: Number(data?.pcsDevices?.[0]?.efficiency ?? 0),
                                        color: getEfficiencyColor(data?.pcsDevices?.[0]?.efficiency ?? 0)
                                    }]}
                                />
                            </div>

                            <div className='flex gap-2 mt-2'>
                                <p className='font-medium text-base md:text-lg mb-1 surface-card p-2 border-round shadow-2'>
                                    <span className='font-normal'>AC:</span> {Number(data?.pcsDevices?.[0]?.acPower?.totalActivePower).toFixed(1)} kW
                                </p>
                                <p className='font-medium text-base md:text-lg mb-1 surface-card p-2 border-round shadow-2'>
                                    <span className='font-normal'>DC:</span> {Math.abs(data?.pcsDevices?.[0]?.dcPower?.power ?? 0).toFixed(1)} kW
                                </p>
                            </div>
                        </div>
                    </div>
                </Card>
            </div>
            <div className="col-12 lg:col-6 xl:col-6">
                <Card title={t('dashboard.solarCapacity')} contentClassName='p-4'>
                    <div className="flex flex-column gap-3">
                        <div className="flex justify-content-between align-items-center">
                            <div className="text-900 font-semibold text-lg md:text-xl">
                                <span>{t("dashboard.currentPower")}:</span> {Number(data?.solarDevices?.[0]?.panelPower?.power).toFixed(1)} kW
                            </div>
                            <div className="flex align-items-center justify-content-center bg-cyan-100 border-round" style={{ width: '2.5rem', height: '2.5rem' }}>
                                <i className="pi pi-sun text-cyan-500 text-xl" />
                            </div>
                        </div>

                        <div className='flex flex-column md:flex-row gap-2 justify-content-between'>
                            <div className="flex flex-column gap-3">
                                <div className="flex align-items-center text-600 text-base md:text-lg">
                                    <span>{t("dashboard.maintenanceStatus")}: </span>
                                    <span className="font-semibold ml-2 text-900">{capitalizeFirstLetter(t("common.required"))}</span>
                                </div>

                                <div className="flex align-items-center text-red-500 text-base md:text-lg">
                                    <span>{t("dashboard.panelTemperature")}</span>
                                    <span className="font-semibold ml-2">
                                        <FaThermometerFull className="mr-2" /> {Number(data?.solarDevices?.[0]?.pvPanelTemperature).toFixed(1)} °C
                                    </span>
                                </div>
                            </div>
                            <div className="flex flex-column">
                                <div className="text-500 font-medium mb-2 text-base md:text-lg">{t("dashboard.energyProducedToday")}</div>
                                <div className="text-900 font-semibold text-lg md:text-xl">{Number(data?.solarDevices?.[0]?.dcOutput?.totalEnergyDaily).toFixed(1)} kWh</div>
                            </div>
                        </div>

                        <div className="flex flex-column md:flex-row gap-3 md:gap-6 mt-2 md:mt-4">
                            <div className="flex flex-column">
                                <div className="text-500 font-medium mb-2 text-base md:text-lg">{t("dashboard.dcOutput")}</div>
                                <div className="text-900 font-semibold text-lg md:text-xl">{Number(data?.solarDevices?.[0]?.dcOutput?.power).toFixed(1)} kW</div>
                            </div>
                            <div className="flex flex-column">
                                <MeterGroup
                                    className='w-20rem'
                                    labelPosition='start'
                                    values={[{
                                        label: t('dashboard.efficiency'),
                                        value: Number(data?.solarDevices?.[0]?.performanceRatio ?? 0),
                                        color: getEfficiencyColor(data?.solarDevices?.[0]?.performanceRatio ?? 0)
                                    }]}
                                />
                            </div>
                        </div>
                    </div>
                </Card>
            </div>
            <div className="col-12 lg:col-6 xl:col-6">
                <Card contentClassName='p-4 md:p-6' title={t('dashboard.dailyAvoidedCO2Emission')}>
                    <Chart
                        height={window.innerWidth < 768 ? '200px' : '300px'}
                        type="bar"
                        data={co2EmissionData}
                        options={co2EmissionOptions}
                    />
                </Card>
            </div>
            <div className="col-12 lg:col-6 xl:col-6">
                <Card contentClassName='p-4 md:p-6' title={t('dashboard.dailyUsage')}>
                    <Chart
                        height={window.innerWidth < 768 ? '200px' : '300px'}
                        type="bar"
                        data={dailyUsageData}
                        options={dailyUsageOptions}
                    />
                </Card>
            </div>
            <div className="col-12 lg:col-6  xl:col-6">
                <Card contentClassName='p-4 md:p-6' title={t('dashboard.costSavingMonthly')}>
                    <Chart
                        height={window.innerWidth < 768 ? '200px' : '300px'}
                        type="bar"
                        data={costSavingsData}
                        options={dailyUsageOptions}
                    />
                </Card>
            </div>

            <div className="col-12 lg:col-6  xl:col-6">
                <Card contentClassName='p-4 md:p-6' title={t('dashboard.dailyUsagePerSource')}>
                    <Chart
                        height={window.innerWidth < 768 ? '200px' : '300px'}
                        type="bar"
                        data={dailyUsagePerSourceData}
                        options={dailyUsagePerSourceOptions}
                    />
                </Card>
            </div>
        </div>
    );
};

export default Dashboard;
