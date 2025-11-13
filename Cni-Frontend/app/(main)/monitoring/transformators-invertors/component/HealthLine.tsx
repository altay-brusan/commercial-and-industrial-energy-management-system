'use client';
import { useEffect, useRef } from 'react';
import * as am5 from '@amcharts/amcharts5';
import * as am5radar from '@amcharts/amcharts5/radar';
import * as am5xy from '@amcharts/amcharts5/xy';

import am5themes_Animated from '@amcharts/amcharts5/themes/Animated';

type HealthLineProps = {
    id: string;
};

const HealthLine = ({ id }: HealthLineProps) => {
    const chartRef = useRef<am5.Root | null>(null);

    useEffect(() => {
        if (chartRef.current) {
            chartRef.current.dispose();
        }

        const root = am5.Root.new(id);
        chartRef.current = root;
        root.setThemes([am5themes_Animated.new(root)]);

        const timelineChart = root.container.children.push(
            am5xy.XYChart.new(root, {
                panX: false,
                panY: false,
                wheelX: "none",
                wheelY: "none",
                paddingLeft: 0,
                layout: root.verticalLayout
            })
        );

        const yRenderer = am5xy.AxisRendererY.new(root, {
            visible: false,
            minGridDistance: 20,
            inversed: true
        });

        yRenderer.grid.template.set("visible", false);

        const yAxis = timelineChart.yAxes.push(
            am5xy.CategoryAxis.new(root, {
                maxDeviation: 0,
                renderer: yRenderer,
                categoryField: "weekday"
            })
        );

        // X ekseni oluştur
        const xRenderer = am5xy.AxisRendererX.new(root, {
            visible: true,
            minGridDistance: 30,
            opposite: true
        });

        xRenderer.grid.template.set("visible", false);

        const xAxisTimeline = timelineChart.xAxes.push(
            am5xy.CategoryAxis.new(root, {
                renderer: xRenderer,
                categoryField: "hour"
            })
        );

        // Heat series oluştur
        const series = timelineChart.series.push(
            am5xy.ColumnSeries.new(root, {
                calculateAggregates: true,
                stroke: am5.color(0xffffff),
                clustered: false,
                xAxis: xAxisTimeline,
                yAxis: yAxis,
                categoryXField: "hour",
                categoryYField: "weekday",
                valueField: "value"
            })
        );

        series.columns.template.setAll({
            tooltipText: "{value}",
            strokeOpacity: 1,
            strokeWidth: 2,
            width: am5.percent(100),
            height: am5.percent(100)
        });

        const heatLegend = timelineChart.bottomAxesContainer.children.push(
            am5.HeatLegend.new(root, {
                orientation: "horizontal",
                startColor: am5.color(0xffff00), // Sarı
                endColor: am5.color(0x00ff00)    // Yeşil
            })
        );

        series.columns.template.events.on("pointerover", function (event) {
            const di = event.target.dataItem as {
                get: (key: "value") => any;
            };
            if (di) {
                heatLegend.showValue(Number(di.get("value")));
            }
        });

        series.set("heatRules", [{
            target: series.columns.template,
            min: am5.color(0xffff00), // Sarı
            max: am5.color(0x00ff00), // Yeşil
            dataField: "value",
            key: "fill"
        }]);


        const generateHeatData = (): { hour: string; weekday: string; value: number }[] => {
            const data: { hour: string; weekday: string; value: number }[] = [];
            const hours = ["12pm", "1am", "2am", "3am", "4am", "5am", "6am", "7am", "8am", "9am", "10am", "11am",
                "12am", "1pm", "2pm", "3pm", "4pm", "5pm", "6pm", "7pm", "8pm", "9pm", "10pm", "11pm"];

            hours.forEach((hour, index) => {
                const value = index < 12 ? 2000 : 12000;
                data.push({
                    hour: hour,
                    weekday: "Today",
                    value: value
                });
            });
            return data;
        };

        const data = generateHeatData();
        series.data.setAll(data);

        // Eksenlere veri ayarla
        yAxis.data.setAll([{ weekday: "Today" }]);
        xAxisTimeline.data.setAll(data.map(item => ({ hour: item.hour })));

        // Animasyon
        series.appear(1000, 100);
        timelineChart.appear(1000, 100);


        return () => {
            root.dispose();
        };
    }, [id]);

    return <div id={id} style={{ width: '100%', height: '200px' }} />;
};

export default HealthLine;
