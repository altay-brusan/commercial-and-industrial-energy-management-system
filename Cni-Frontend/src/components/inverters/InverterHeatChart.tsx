import React, { useMemo } from 'react';
import dynamic from 'next/dynamic';
import { ApexOptions } from 'apexcharts';

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false });

interface InverterData {
    time: string;
    prediction: number;
    production: number;
    inverterName: string;
}

type InverterHeatChartProps = {
    data?: InverterData[];
    className?: string;
};

const InverterHeatChart = ({ data = [], className = '' }: InverterHeatChartProps) => {
    const uniqueHours = useMemo(() => {
        const hours = new Set(data.map(item => {
            const date = new Date(item.time);
            return `${date.getHours().toString().padStart(2, '0')}:00`;
        }));
        return Array.from(hours).sort();
    }, [data]);

    const uniqueInverters = useMemo(() => {
        const inverters = new Set(data.map(item => item.inverterName));
        return Array.from(inverters).sort();
    }, [data]);

    const generateData = useMemo(() => {
        return uniqueHours.flatMap(hour =>
            uniqueInverters.map(inverter => {
                const matchingData = data.find(
                    item =>
                        item.inverterName === inverter &&
                        item.time.includes(hour)
                );
                return {
                    x: hour,
                    y: inverter,
                    value: matchingData ? matchingData.production : 0
                };
            })
        );
    }, [data, uniqueHours, uniqueInverters]);

    const chartOptions: ApexOptions = {
        chart: {
            type: 'heatmap',
            height: 400,
            toolbar: {
                show: false
            }
        },
        plotOptions: {
            heatmap: {
                shadeIntensity: 0.5,
                radius: 0,
                useFillColorAsStroke: true,
                colorScale: {
                    ranges: [
                        {
                            from: 0,
                            to: 30,
                            name: 'low',
                            color: '#FFB64D'
                        },
                        {
                            from: 30,
                            to: 60,
                            name: 'medium',
                            color: '#FF7F7F'
                        },
                        {
                            from: 60,
                            to: 100,
                            name: 'high',
                            color: '#FF0000'
                        }
                    ]
                }
            }
        },
        dataLabels: {
            enabled: false
        },
        xaxis: {
            type: 'category',
            categories: uniqueHours,
            labels: {
                rotate: -45,
                rotateAlways: true
            }
        },
        yaxis: {
            labels: {
                formatter: function (val: number) {
                    return uniqueInverters[val] || '';
                }
            }
        },
        tooltip: {
            y: {
                formatter: function (val: number) {
                    return val + ' kW';
                }
            }
        }
    };

    const chartSeries = [{
        name: 'Power',
        data: generateData
    }];

    return (
        <div className={`w-full ${className}`}>
            <Chart
                options={chartOptions}
                series={chartSeries}
                type="heatmap"
                height={400}
            />
        </div>
    );
};

export default InverterHeatChart;
