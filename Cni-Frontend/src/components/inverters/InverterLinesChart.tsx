import React, { useMemo } from 'react';
import dynamic from 'next/dynamic';
import { ApexOptions } from 'apexcharts';

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false });

export interface ChartData {
    series: {
        name: string;
        data: number[];
    }[];
    categories: string[];
}

interface InverterLinesChartProps {
    title: string;
    chartId: string;
    height?: string;
    data?: ChartData;
}

const getUnitByChartId = (chartId: string): string => {
    switch (chartId) {
        case 'activePower':
            return 'kW';
        case 'reactivePower':
            return 'kVAR';
        case 'powerFactor':
            return 'P/S';
        case 'acVoltage':
        case 'dcVoltage':
            return 'V';
        case 'acCurrent':
        case 'dcCurrent':
            return 'A';
        case 'dcPower':
            return 'kW';
        case 'temperature':
            return 'Celcius';
        default:
            return '';
    }
};

const InverterLinesChart = ({
    title,
    height = '400px',
    data,
    chartId
}: InverterLinesChartProps) => {
    const unit = getUnitByChartId(chartId);

    const chartOptions: ApexOptions = {
        chart: {
            background: "#fff",
            type: 'line',
            height: parseInt(height),
            zoom: {
                type: 'x',
                enabled: true,
                autoScaleYaxis: true
            },
            toolbar: {
                show: true,
                tools: {
                    download: false,
                    selection: true,
                    zoom: true,
                    zoomin: true,
                    zoomout: true,
                    pan: true,
                    reset: true
                }
            }
        },
        colors: ['#0d6efd'],
        dataLabels: {
            enabled: false
        },
        stroke: {
            curve: 'smooth',
            width: 2
        },
        markers: {
            size: 3,
            hover: {
                size: 6
            }
        },
        grid: {
            show: true,
            xaxis: {
                lines: {
                    show: true
                }
            },
            yaxis: {
                lines: {
                    show: true
                }
            }
        },
        yaxis: {
            title: {
                text: unit ? `${title} (${unit})` : title
            },
            labels: {
                formatter: function (val: number) {
                    return val.toFixed(2);
                }
            }
        },
        xaxis: {
            categories: data?.categories || [],
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
            },
            axisBorder: {
                show: true
            },
            tooltip: {
                enabled: true
            }
        },
        tooltip: {
            shared: true,
            intersect: false,
            y: {
                formatter: function (val: number) {
                    return val.toFixed(2) + (unit ? ` ${unit}` : '');
                }
            }
        },
        legend: {
            position: 'top',
            horizontalAlign: 'right',
            floating: true,
            offsetY: -25,
            offsetX: -5
        }
    };

    return (
        <div className="w-full h-full flex flex-column">
            <h3 className="text-xl font-semibold mb-4">{title}</h3>
            <Chart
                options={chartOptions}
                series={data?.series || []}
                type="line"
                height={parseInt(height)}
            />
        </div>
    );
};

export default InverterLinesChart;
