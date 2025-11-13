import React, { useMemo } from 'react';
import dynamic from 'next/dynamic';
import { ApexOptions } from 'apexcharts';

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false });

interface InverterLinesChartProps {
    title: string;
    chartId: string;
    height?: string;
    seriesName?: string;
    data?: Array<{
        time: string;
        prediction: number;
        production: number;
    }>;
}

const PowerGeneration = ({ title, chartId, height = '400px', seriesName, data = [] }: InverterLinesChartProps) => {
    const chartOptions: ApexOptions = {
        chart: {
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
        colors: ['#0d6efd', '#198754'],
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
                text: 'Power (kW)'
            },
            labels: {
                formatter: function (val: number) {
                    return val.toFixed(2);
                }
            }
        },
        xaxis: {
            categories: data.map(item => item.time),
            labels: {
                rotate: -45,
                rotateAlways: true,
                maxHeight: 100,
                trim: true,
                style: {
                    fontSize: '11px'
                },
                formatter: function(value: any) {
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
                    return val.toFixed(2) + ' kW';
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

    const chartSeries = useMemo(() => [
        {
            name: 'Prediction',
            data: data.map(item => item.prediction)
        },
        {
            name: 'Generation',
            data: data.map(item => item.production)
        }
    ], [data]);

    return (
        <div className="w-full h-full flex flex-column">
            <h3 className="text-xl font-semibold mb-4">{title}</h3>
            <Chart
                options={chartOptions}
                series={chartSeries}
                type="line"
                height={parseInt(height)}
            />
        </div>
    );
};

export default PowerGeneration;
