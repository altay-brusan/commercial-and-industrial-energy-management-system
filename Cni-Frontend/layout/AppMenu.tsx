/* eslint-disable @next/next/no-img-element */

import React, { useContext } from 'react';
import AppMenuitem from './AppMenuitem';
import { LayoutContext } from './context/layoutcontext';
import { MenuProvider } from './context/menucontext';
import { AppMenuItem } from '@/src/types/layout';
import { useTranslations } from 'next-intl';
import { useUser } from '@/src/hooks/use-user';

const AppMenu = () => {
    const t = useTranslations();
    const { user } = useUser();

    const model: AppMenuItem[] = [
        {
            label: "",
            items: [
                {
                    label: t('menu.dashboard'),
                    icon: 'pi pi-fw pi-home',
                    to: '/',
                },
                {
                    label: t('menu.insights'), icon: 'pi pi-fw pi-home', items: [
                        {
                            label: t('menu.monitoring'),
                            icon: 'pi pi-fw pi-chart-line',
                            to: '/monitoring',
                            items: [
                                { label: t('menu.bess'), icon: 'pi pi-fw pi-th-large', to: '/monitoring/bess' },
                                {
                                    label: t('menu.bmu'),
                                    icon: 'pi pi-fw pi-server',
                                    items: [
                                        { label: t('menu.bmuOverview'), icon: 'pi pi-fw pi-eye', to: '/monitoring/bmu' },
                                        { label: t('menu.bmuFaults'), icon: 'pi pi-fw pi-exclamation-triangle', to: '/monitoring/bmu/faults' },
                                        { label: t('menu.bmuThermal'), icon: 'pi pi-fw pi-sun', to: '/monitoring/bmu/thermal' },
                                        { label: t('menu.bmuChillerAlarms'), icon: 'pi pi-fw pi-bell', to: '/monitoring/bmu/thermal/alarms' },
                                        { label: t('menu.bmuHvcu'), icon: 'pi pi-fw pi-sliders-h', to: '/monitoring/bmu/hvcu' },
                                    ]
                                },
                                // { label: t('menu.transformators&Invertors'), icon: 'pi pi-fw pi-th-large', to: '/monitoring/transformators-invertors' },
                                { label: t('menu.inverters'), icon: 'pi pi-fw pi-th-large', to: '/monitoring/inverters' },
                                // { label: t('menu.grid'), icon: 'pi pi-fw pi-th-large', to: '/monitoring/grid' },
                                { label: t('menu.solar'), icon: 'pi pi-fw pi-sun', to: '/monitoring/solar' },
                                { label: t('menu.powerGeneral'), icon: 'pi pi-fw pi-power-off', to: '/monitoring/power-general' },
                                // { label: t('menu.generator'), icon: 'pi pi-fw pi-cog', to: '/monitoring/generator' },
                                // { label: t('menu.load'), icon: 'pi pi-fw pi-th-large', to: '/monitoring/load' },
                            ]
                        },
                        // {
                        //     label: t('menu.analytics'),
                        //     icon: 'pi pi-fw pi-chart-pie',
                        //     to: '/analytics',
                        //     items: [
                        //         { label: t('menu.energySummary'), icon: 'pi pi-fw pi-chart-bar', to: '/analytics/energy-summary' },
                        //         { label: t('menu.efficiencyKpis'), icon: 'pi pi-fw pi-chart-line', to: '/analytics/efficiency-kpis' },
                        //         { label: t('menu.optimizationRecommendations'), icon: 'pi pi-fw pi-lightbulb', to: '/analytics/optimization-recommendations' },
                        //         { label: t('menu.predictiveInsights'), icon: 'pi pi-fw pi-eye', to: '/analytics/predictive-insights' },
                        //         { label: t('menu.productionInsights'), icon: 'pi pi-fw pi-chart-line', to: '/analytics/production-insights' },
                        //     ]
                        // },
                        // {
                        //     label: t('menu.controlAction'),
                        //     icon: 'pi pi-fw pi-cog',
                        //     to: '/control-action',
                        //     items: [
                        //         { label: t('menu.mainPowerControl'), icon: 'pi pi-fw pi-power-off', to: '/main-power-control' },
                        //         { label: t('menu.bessControl'), icon: 'pi pi-fw pi-th-large', to: '/bess-control' },
                        //         { label: t('menu.energyOptimizationControl'), icon: 'pi pi-fw pi-chart-line', to: '/energy-optimization' },
                        //         { label: t('menu.solarSystemControl'), icon: 'pi pi-fw pi-sun', to: '/solar-system' },
                        //         { label: t('menu.generatorControl'), icon: 'pi pi-fw pi-cog', to: '/generator-control' },
                        //     ]
                        // },
                        // { label: t('menu.digitalTwin'), icon: 'pi pi-fw pi-cog', to: '/digital-twin' },
                        // { label: t('menu.reporting'), icon: 'pi pi-fw pi-file-pdf', to: '/reporting' },
                        { label: t('menu.notification'), icon: 'pi pi-fw pi-bell', to: '/notification' },]
                },
                {
                    label: t('menu.automation'),
                    icon: 'pi pi-fw pi-cog',
                    items: [
                        { label: t('menu.scada'), icon: 'pi pi-fw pi-home', to: '/scada' },
                        { label: t('menu.facilityMap'), icon: 'pi pi-fw pi-map', to: '/facility-map' },
                    ]
                },
                // {
                //     label: t('menu.virtualPowerPlant'),
                //     icon: 'pi pi-fw pi-cog',
                //     to: '/virtual-power-plant'
                // },
                {
                    label: t('menu.systemManagement'),
                    icon: 'pi pi-fw pi-cog',
                    visible: user?.roles.includes('Administrator'),
                    items: [
                        {
                            label: t('menu.panelManagement'),
                            icon: 'pi pi-fw pi-cog',
                            items: [
                                { label: t('menu.holdingIdentification'), icon: 'pi pi-fw pi-building', to: '/holding-identification' },
                                { label: t('menu.facilityIdentification'), icon: 'pi pi-fw pi-building', to: '/facility-identification' },
                                { label: t('menu.userIdentification'), icon: 'pi pi-fw pi-user', to: '/user-identification' },
                                { label: t('menu.deviceManagement'), icon: 'pi pi-fw pi-microchip', to: '/devices' },
                                { label: t('menu.planDefinition'), icon: 'pi pi-fw pi-file', to: '/plan-identification' },
                                { label: t('menu.blockedAddresses'), icon: 'pi pi-fw pi-ban', to: '/blocked-addresses' },
                            ]
                        },
                        {
                            label: t('menu.systemConfiguration'),
                            icon: 'pi pi-fw pi-cog',
                            items: [
                                { label: t('menu.smtpConfiguration'), icon: 'pi pi-fw pi-envelope', to: '/smtp-config' },
                                { label: t('menu.authExample'), icon: 'pi pi-fw pi-lock', to: '/auth-example' }
                            ]
                        },
                        {
                            label: t('menu.permissionManagement'),
                            icon: 'pi pi-fw pi-lock',
                            to: '/permission-management'
                        },
                    ],
                }
            ],
        },

    ];

    return (
        <MenuProvider>
            <ul className="layout-menu">
                {model.map((item, i) => {
                    return !item?.seperator ? <AppMenuitem item={item} root={true} index={i} key={item.label} /> : <li className="menu-separator"></li>;
                })}
            </ul>
        </MenuProvider>
    );
};

export default AppMenu;
