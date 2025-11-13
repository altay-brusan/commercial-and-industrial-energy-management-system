"use client"
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import { BreadCrumb } from 'primereact/breadcrumb'
import { Calendar } from 'primereact/calendar';
import React, { useState, useLayoutEffect } from 'react'
import * as am5 from "@amcharts/amcharts5";
import * as am5xy from "@amcharts/amcharts5/xy";
import am5themes_Animated from "@amcharts/amcharts5/themes/Animated";

export default function EfficiencyPage() {
    const [dates, setDates] = useState<Date[]>([new Date(new Date().setDate(new Date().getDate() - 7)), new Date()]);
    const t = useTranslations()

    const items = [
        { label: 'Monitoring' },
        { label: 'Inverters' },
        { label: 'Detail' },
        {
            label: '    ',
            template: () => <Link href="/monitoring/inverters/efficiency">Efficiency</Link>
        }
    ];
    const home = { icon: 'pi pi-home', url: '/' };

    useLayoutEffect(() => {
        let root = am5.Root.new("chartDiv");
        root.setThemes([am5themes_Animated.new(root)]);

        let chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true,
            panY: true,
            wheelX: "panX",
            wheelY: "zoomX",
            pinchZoomX: true,
            paddingLeft: 0
        }));

        let cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
            behavior: "none"
        }));
        cursor.lineY.set("visible", false);

        let xAxis = chart.xAxes.push(
            am5xy.DateAxis.new(root, {
                maxDeviation: 0.2,
                baseInterval: {
                    timeUnit: "day",
                    count: 1
                },
                renderer: am5xy.AxisRendererX.new(root, {
                    minorGridEnabled: true
                }),
                tooltip: am5.Tooltip.new(root, {})
            })
        );

        let yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererY.new(root, {
                pan: "zoom"
            })
        }));

        const generateData = (startDate: Date, endDate: Date) => {
            let data = [];
            let value = 100;
            let currentDate = new Date(startDate);

            while (currentDate <= endDate) {
                value = Math.round((Math.random() * 10 - 5) + value);
                data.push({
                    date: currentDate.getTime(),
                    value: Math.min(Math.max(value, 0), 100)
                });
                currentDate.setDate(currentDate.getDate() + 1);
            }
            return data;
        };

        let data = generateData(dates[0], dates[1]);
        let series = chart.series.push(am5xy.LineSeries.new(root, {
            name: "Series",
            xAxis: xAxis,
            yAxis: yAxis,
            valueYField: "value",
            valueXField: "date",
            tooltip: am5.Tooltip.new(root, {
                labelText: "{valueY}"
            })
        }));

        series.data.setAll(data);

        series.appear(1000);
        chart.appear(1000, 100);

        return () => {
            root.dispose();
        };
    }, [dates]);

    return (
        <div className='grid'>
            <div className='col-12'>
                <BreadCrumb className='mb-4' model={items} home={home} />
                <div className='flex flex-column gap-2 mb-4'>
                    <label>{t("inverters.efficiency.date")}</label>
                    <Calendar
                        className='w-24rem'
                        value={dates}
                        onChange={(e) => setDates(e.value?.filter((date): date is Date => date !== null) ?? [])}
                        selectionMode="range"
                        hideOnRangeSelection
                        showButtonBar
                        showTime
                        dateFormat="dd/mm/yy"
                        hourFormat="24"
                    />
                </div>
                <div id="chartDiv" className='w-full' style={{ height: '500px' }}></div>
            </div>
        </div>
    )
}
