'use client'
import React, { useLayoutEffect, useState, useEffect, useCallback, useRef } from 'react'
import CustomDropdown from '@/src/components/ui/Dropdown'
import { useTranslations } from 'next-intl'
import { Message } from 'primereact/message'
import { Button } from 'primereact/button'
import { Tooltip } from 'primereact/tooltip'
import { COLORS } from '@/src/utils/color'
import dynamic from 'next/dynamic'
import { ApexOptions } from 'apexcharts'
import { DashboardRealTimeData } from '@/src/models/DashboardRealTimeData'
import webSocketService from '@/src/services/WebSocketService'
import { useFetch } from '@/src/hooks/use-fetch'
import { formatDateISO } from '@/src/utils/dateFormatTR'
import { useTenant } from '@/src/context/TenantContext'
import { graphsService } from '@/src/services/graphs'
import { subDays } from 'date-fns'
import { setHours, setMinutes } from 'date-fns'
import { scadaService } from '@/src/services/scada'
import { tryCatch } from '@/src/utils/tryCatch'
import { useLoading } from '@/src/context/LoadingContext'
const ReactApexChart = dynamic(() => import('react-apexcharts'), { ssr: false })

interface ChartState {
    series: {
        name: string;
        data: number[];
    }[];
    options: {
        chart: {
            height: number;
            type: 'area';
            toolbar: {
                show: boolean;
            }
        };
        dataLabels: {
            enabled: boolean;
        };
        stroke: {
            curve: 'smooth' | 'straight' | 'stepline';
        };
        colors: string[];
        xaxis: {
            categories: string[];
        };
        tooltip: {
            x: {
                format: string;
            };
            y: {
                formatter: (value: number) => string;
            };
        };
        legend: {
            position: 'top' | 'right' | 'bottom' | 'left';
            horizontalAlign: 'left' | 'center' | 'right';
        };
    };
}

const formatTime = (dateString: string): string => {
    const date = new Date(dateString);
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
};

const PowerGeneralPage = () => {
    const t = useTranslations();
    const [isConnected, setIsConnected] = useState(false);
    const connectionRef = useRef<(() => void) | null>(null);

    const [period, setPeriod] = useState<string>("now");
    const { showLoading, hideLoading, setIsLoading } = useLoading();
    const [data, setData] = useState<DashboardRealTimeData>();
    const { startDate, tenant } = useTenant();
    const previousDay = startDate === null ? formatDateISO(setMinutes(setHours(new Date(), 0), 0)) : setMinutes(setHours(subDays(startDate, 1), 3), 0);
    const [powerGeneralData] = useFetch<any>(() => tenant ? graphsService.getPowerGeneralGraphData(tenant, previousDay, startDate ? formatDateISO(startDate) as any : '') : Promise.resolve([]), { deps: [tenant, startDate], reloadDeps: [tenant, startDate], setLoading: setIsLoading });
    console.log("🚀 ~ PowerGeneralPage ~ powerGeneralData:", powerGeneralData)

    const [lineChartState, setLineChartState] = useState<ChartState>({
        series: [
            {
                name: "BESS",
                data: []
            },
            {
                name: "Solar",
                data: []
            },
            {
                name: "Grid",
                data: []
            },
            {
                name: "Generator",
                data: []
            }
        ],
        options: {
            chart: {
                height: 500,
                type: 'area',
                toolbar: {
                    show: false
                }
            },
            dataLabels: {
                enabled: false
            },
            stroke: {
                curve: 'smooth'
            },
            colors: [COLORS.BESS, COLORS.SOLAR, COLORS.GRID, COLORS.GENERATOR],
            tooltip: {
                x: {
                    format: 'dd/MM/yy HH:mm'
                },
                y: {
                    formatter: function (value: number) {
                        return value?.toFixed(2) + 'kW';
                    }
                }
            },
            xaxis: {
                categories: [],
            },
            legend: {
                position: 'top',
                horizontalAlign: 'left',
            }
        }
    });

    const [donutChartState, setDonutChartState] = useState({
        series: [0, 0, 0, 0],
        options: {
            chart: {
                type: 'donut' as const,
                height: 350,
                animations: {
                    enabled: false
                },
                dropShadow: {
                    enabled: false
                },
                toolbar: {
                    show: false
                }
            },
            colors: [COLORS.GRID, COLORS.BESS, COLORS.GENERATOR, COLORS.SOLAR],
            labels: ['GRID', 'BESS', 'GENERATOR', 'SOLAR'],
            dataLabels: {
                enabled: true,
                formatter: function (val: number) {
                    return val?.toFixed(2) + '%'
                },
                style: {
                    fontSize: window.innerWidth < 768 ? '12px' : '14px',
                    fontWeight: 'bold',
                    textShadow: 'none'
                },
                background: {
                    enabled: false
                },
                dropShadow: {
                    enabled: false
                }
            },
            plotOptions: {
                pie: {
                    startAngle: -90,
                    endAngle: 90,
                    offsetY: 10,
                    donut: {
                        size: '60%',
                        labels: {
                            show: true,
                            name: {
                                show: true,
                                fontSize: window.innerWidth < 768 ? '12px' : '14px',
                                fontWeight: 600,
                                offsetY: 0,
                                shadow: {
                                    enabled: false
                                }
                            },
                            value: {
                                show: true,
                                fontSize: window.innerWidth < 768 ? '14px' : '16px',
                                fontWeight: 400,
                                shadow: {
                                    enabled: false
                                },
                                formatter: function (val: string) {
                                    return val + '%'
                                }
                            },

                        }
                    }
                }
            },
            grid: {
                padding: {
                    bottom: -150
                }
            },
            legend: {
                position: 'bottom' as const,
                formatter: function (seriesName: string, opts: any) {
                    return seriesName + ":  " + opts.w.globals.series[opts.seriesIndex]?.toFixed(2) + '%'
                }
            },
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        height: 250
                    },
                    legend: {
                        position: 'bottom' as const
                    }
                }
            }]
        }
    });

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
    }, [startDate]);

    useEffect(() => {
        if (data?.systemData) {
            const gridContribution = data.systemData.gridPower || 0;
            const pcsContribution = data.systemData.pcsPower || 0;
            const generatorContribution = data.systemData.generatorPower || 0;
            const solarContribution = data.systemData.solarPower || 0;

            const total = gridContribution + pcsContribution + generatorContribution + solarContribution;

            if (total > 0) {
                const gridPercentage = Number((gridContribution / total * 100)?.toFixed(2));
                const pcsPercentage = Number((pcsContribution / total * 100)?.toFixed(2));
                const generatorPercentage = Number((generatorContribution / total * 100)?.toFixed(2));
                const solarPercentage = Number((solarContribution / total * 100)?.toFixed(2));

                setDonutChartState(prevState => ({
                    ...prevState,
                    series: [gridPercentage, pcsPercentage, generatorPercentage, solarPercentage]
                }));
            }
        }
    }, [data]);

    useEffect(() => {
        if (powerGeneralData && Array.isArray(powerGeneralData)) {
            const bessData: number[] = [];
            const solarData: number[] = [];
            const gridData: number[] = [];
            const generatorData: number[] = [];
            const timeCategories: string[] = [];

            powerGeneralData.forEach(item => {
                bessData.push(Number(Number(item.bessPower)?.toFixed(2)));
                solarData.push(Number(Number(item.solarPower)?.toFixed(2)));
                gridData.push(Number(Number(item.gridPower)?.toFixed(2)));
                generatorData.push(Number(Number(item.generatorPower)?.toFixed(2)));
                timeCategories.push(formatTime(item.time));
            });

            setLineChartState(prevState => ({
                ...prevState,
                series: [
                    {
                        name: "BESS",
                        data: bessData
                    },
                    {
                        name: "Solar",
                        data: solarData
                    },
                    {
                        name: "Grid",
                        data: gridData
                    },
                    {
                        name: "Generator",
                        data: generatorData
                    }
                ],
                options: {
                    ...prevState.options,
                    xaxis: {
                        ...prevState.options.xaxis,
                        categories: timeCategories,
                        labels: {
                            rotate: -45,
                            rotateAlways: true,
                            maxHeight: 100,
                            trim: true,
                            style: {
                                fontSize: '11px'
                            },
                            formatter: function (value: any) {
                                if (!value) return '';
                                const parts = String(value).split(' ');
                                return parts.length > 1 ? parts[1] : value;
                            }
                        },
                        tickAmount: 10,
                        axisTicks: {
                            show: true
                        }
                    }
                }
            }));
        }
    }, [powerGeneralData]);

    const content = (
        <div className="flex align-items-center justify-content-between w-full">
            <div className="ml-2">
                <i className="pi pi-exclamation-triangle mr-2"></i>
                Unusual energy usuage detected!.</div>
            <Button text link label="See More" icon="pi pi-arrow-right" iconPos='right' />
        </div>
    );

    return (
        <div className="w-full h-full text-900">
            <div className="card">
                <h3 className="text-2xl font-bold mb-2">{t('powerGeneral.title')}</h3>
                <div className='flex flex-column gap-3 my-4'>
                    <Message className='w-full justify-content-start' severity="info" text={t('powerGeneral.description')} />
                    <Message className='w-full justify-content-start' severity="warn" text="Unusual energy usuage detected!" content={content} />
                </div>
            </div>
            <div className="card">
                <div className="grid">
                    <div className="col-12 md:col-3">
                        <CustomDropdown
                            value={period}
                            className="w-full"
                            label={t('powerGeneral.selectPeriod')}
                            options={[{ label: t("common.now"), value: "now" }, { label: t("common.oneMonth"), value: "1month" }, { label: t("common.threeMonths"), value: "3month" }, { label: t("common.sixMonths"), value: "6month" }, { label: t("common.oneYear"), value: "1year" }]}
                            placeholder="Select a facility"
                            onChange={(value) => setPeriod(value)}
                        />
                    </div>
                </div>
            </div>
            <div className="grid nested-grid">
                <div className="col-12 xl:col-6">
                    <div className="card p-0">
                        <div className='flex flex-row justify-content-between'>
                            <h5 className="text-xl mb-4 pt-4 pl-4">
                                {t('powerGeneral.totalPowerConsumedByFacility')}
                            </h5>
                            <div className='flex flex-row justify-content-end'>
                                <Button text link label={t("common.reportView")} icon="pi pi-arrow-right text-primary" iconPos='right' />
                            </div>
                        </div>
                        <hr />
                        <div className="p-4">
                            <ReactApexChart
                                options={donutChartState.options as ApexOptions}
                                series={donutChartState.series}
                                type="donut"
                                height={325}
                            />
                        </div>
                    </div>
                </div>
                <div className="col-12 xl:col-6">
                    <div className="grid">
                        <div className="col-12 md:col-6">
                            <div className="card p-0">
                                <h5 className="text-xl mb-4 pt-4 px-4">{t('powerGeneral.powerFactor')}</h5>
                                <div className="w-full text-left text-6xl font-bold p-6 h-full">
                                    {data?.facilityData?.acPower?.totalPowerFactor?.toFixed(2)}<span className='text-sm'>P/S</span>
                                    <div className='text-left font-medium text-sm flex gap-3 w-full justify-content-between mt-5'>
                                        <div className='flex flex-column'>
                                            <span>{t('powerGeneral.activePower')}</span>
                                            <span className='font-bold'>{data?.facilityData?.acPower?.totalActivePower?.toFixed(2)} kW</span>
                                        </div>
                                        <div className='flex flex-column text-right'>
                                            <span>{t('powerGeneral.reactivePower')}</span>
                                            <span className='font-bold'>{data?.facilityData?.acPower?.totalReactivePower?.toFixed(2)}  kVAR</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="col-12 md:col-6">
                            <div className="card p-0 h-full relative">
                                <h5 className="text-xl text-xl mb-4 pt-4 px-4">{t('powerGeneral.gridPrice')}</h5>
                                <div className='absolute top-5 right-5'>
                                    <Tooltip target=".custom-target-icon" />
                                    <i className="custom-target-icon pi pi-info-circle p-text-secondary p-overlay-badge"
                                        data-pr-tooltip="Info"
                                        data-pr-position="top"
                                        style={{ fontSize: '1.5rem', cursor: 'pointer', marginRight: "10px" }}>
                                    </i>
                                </div>
                                <div className="w-full text-left text-6xl font-bold p-4 md:p-6">
                                    6.130 <span className='text-sm'>₺/kWh</span>
                                </div>
                                <span className='text-sm pl-4 md:pl-6 pb-3 block'>Energy prices are provided by EPDK</span>
                            </div>
                        </div>
                        <div className="col-12 md:col-6">
                            <div className="card p-0">
                                <h5 className="text-xl mb-4 pt-4 px-4">{t('powerGeneral.co2Emission')}</h5>
                                <div className="w-full text-left text-6xl font-bold p-4 md:p-6">
                                    19.12 <span className='text-sm'>tons this month                                    </span>
                                </div>
                            </div>
                        </div>
                        <div className="col-12 md:col-6">
                            <div className="card p-0 relative">
                                <h5 className="text-xl mb-4 pt-4 px-4">{t('common.thisMonth')}</h5>
                                <div className='absolute top-5 right-5'>
                                    <Tooltip target=".custom-target-icon" />
                                    <i className="custom-target-icon pi pi-info-circle p-text-secondary p-overlay-badge"
                                        data-pr-tooltip="Info"
                                        data-pr-position="top"
                                        style={{ fontSize: '1.5rem', cursor: 'pointer', marginRight: "10px" }}>
                                    </i>
                                </div>
                                <div className="w-full text-left text-6xl font-bold p-4 md:p-6">
                                    5344.58  ₺
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div className="grid mt-4">
                <div className="col-12 xl:col-12">
                    <div className="card p-0">
                        <div className='flex flex-row justify-content-between'>
                            <h5 className="text-xl mb-4 pt-4 pl-4">
                                {t('powerGeneral.powerConsumptionTrend')}
                            </h5>
                            <div className='flex flex-row justify-content-end'>
                                <Button text link label={t("common.reportView")} icon="pi pi-arrow-right" iconPos='right' />
                            </div>
                        </div>
                        <hr />
                        <div className="p-4">
                            <ReactApexChart
                                options={lineChartState.options}
                                series={lineChartState.series}
                                type="area"
                                height={500}
                            />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default PowerGeneralPage;

