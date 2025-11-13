/* eslint-disable react-hooks/exhaustive-deps */
/* Imports */
import React, { useEffect, useRef } from 'react';
import * as am5 from "@amcharts/amcharts5";
import * as am5xy from "@amcharts/amcharts5/xy";
import am5themes_Animated from "@amcharts/amcharts5/themes/Animated";

interface DcFluctuationChartProps {
    id: string;
}

const DcFluctuationChart: React.FC<DcFluctuationChartProps> = ({ id }) => {
    const chartRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!chartRef.current) return;

        let root = am5.Root.new(id as string);

        root.setThemes([
            am5themes_Animated.new(root)
        ]);

        let chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true,
            panY: true,
            wheelX: "panX",
            wheelY: "zoomX",
            pinchZoomX: true,
            paddingLeft: 0
        }));
        chart?.get("colors")?.set("step", 3);

        let cursor = chart?.set("cursor", am5xy.XYCursor.new(root, {}));
        cursor?.lineY?.set("visible", false);

        let xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
            maxDeviation: 0.3,
            baseInterval: {
                timeUnit: "hour",
                count: 1
            },
            renderer: am5xy.AxisRendererX.new(root, {
                minorGridEnabled: true,
                minGridDistance: 70
            }),
            tooltip: am5.Tooltip.new(root, {})
        }));
        xAxis.get("renderer").labels.template.set("text", "Saat");

        let yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            maxDeviation: 0.3,
            renderer: am5xy.AxisRendererY.new(root, {})
        }));
        yAxis.get("renderer").labels.template.set("text", "kV");

        let series = chart.series.push(am5xy.LineSeries.new(root, {
            name: "Series 1",
            xAxis: xAxis,
            yAxis: yAxis,
            valueYField: "value",
            valueXField: "date",
            tooltip: am5.Tooltip.new(root, {
                labelText: "{valueY}"
            })
        }));
        series.strokes.template.setAll({
            strokeWidth: 5,
            strokeDasharray: [3, 3]
        });

        series.bullets.push(function (root, series, dataItem: any) {
            if (dataItem.dataContext && dataItem.dataContext.bullet) {
                let container = am5.Container.new(root, {});
                let circle0 = container.children.push(am5.Circle.new(root, {
                    radius: 5,
                    fill: am5.color(0xff0000)
                }));
                let circle1 = container.children.push(am5.Circle.new(root, {
                    radius: 5,
                    fill: am5.color(0xff0000)
                }));

                circle1.animate({
                    key: "radius",
                    to: 20,
                    duration: 1000,
                    easing: am5.ease.out(am5.ease.cubic),
                    loops: Infinity
                });
                circle1.animate({
                    key: "opacity",
                    to: 0,
                    from: 1,
                    duration: 1000,
                    easing: am5.ease.out(am5.ease.cubic),
                    loops: Infinity
                });

                return am5.Bullet.new(root, {
                    sprite: container
                })
            }
        })

        let data = [{
            date: new Date(2021, 5, 12, 3, 0).getTime(),
            value: 50
        }, {
            date: new Date(2021, 5, 12, 1, 0).getTime(),
            value: 53
        }, {
            date: new Date(2021, 5, 12, 2, 0).getTime(),
            value: 56
        }, {
            date: new Date(2021, 5, 12, 3, 0).getTime(),
            value: 52
        }, {
            date: new Date(2021, 5, 12, 4, 0).getTime(),
            value: 48
        }, {
            date: new Date(2021, 5, 12, 5, 0).getTime(),
            value: 47
        }, {
            date: new Date(2021, 5, 12, 6, 0).getTime(),
            value: 59,
            bullet: true
        }]

        series.data.setAll(data);

        series.appear(1000);
        chart.appear(1000, 100);

        chart.children.push(am5.Label.new(root, {
            text: "DC Fluctuation",
            fontSize: 20,
            x: am5.percent(50),
            centerX: am5.percent(50),
            paddingTop: 10,
            paddingBottom: 10
        }));

        return () => {
            root.dispose();
        };
    }, []);

    return (
        <div id={id} ref={chartRef} style={{ width: "100%", height: "500px" }}></div>
    );
};

export default DcFluctuationChart;
