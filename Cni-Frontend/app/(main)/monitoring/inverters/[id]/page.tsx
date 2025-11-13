"use client"
import React, { useRef } from 'react'
import { Card } from 'primereact/card'
import { useParams } from 'next/navigation'
import { useTranslations } from 'next-intl'
import EfficiencyGauge from '../../transformators-invertors/component/EfficiencyGauge'
import DcFluctuationChart from '../../transformators-invertors/component/DcFluctuation'
import HealthLine from '../../transformators-invertors/component/HealthLine'
import { Button } from 'primereact/button'
import { BreadCrumb } from 'primereact/breadcrumb'
import Link from 'next/link'
import { Menu } from 'primereact/menu'


export default function InverterMonitoringPage() {
    const { id } = useParams() as { id: string };
    const t = useTranslations()
    const menu = useRef<Menu>(null)
    const items = [
        { label: 'Monitoring' },
        { label: 'Inverters' },
        {
            label: 'InputText',
            template: () => <Link href="/monitoring/inverters">Details</Link>
        }
    ];
    const home = { icon: 'pi pi-home', url: '/' };


    const MENU_ITEMS = [
        {
            label: 'Efficiency',
            url: `/monitoring/inverters/${id}/efficiency`
        },
        {
            label: 'Temperature',
            url: `/monitoring/inverters/${id}/temperature`
        },
        {
            label: 'DC Fluctutation',
            url: `/monitoring/inverters/${id}/dc-fluctuation`
        },
        {
            label: 'Power Factor',
            url: `/monitoring/inverters/${id}/power-factor`
        }
    ];
    return (
        <div className='grid'>
            <div className='col-12'>
                <BreadCrumb className='mb-4' model={items} home={home} />
                <Menu model={MENU_ITEMS} popup ref={menu} id="popup_menu_left" />
                <Button label={t("general.viewDetails")} icon="pi pi-external-link" className="mb-4" onClick={(event) => menu?.current?.toggle(event)} aria-controls="popup_menu_left" aria-haspopup />
                <Card>
                    <div className='flex w-full align-items-start justify-content-between'>
                        <div className='flex align-items-center justify-content-start gap-5'>
                            <h2 className="text-2xl font-bold mb-0">PCS 1</h2>
                            <img src="/assets/icons/mask.png" alt="mask" />
                        </div>
                        <Button text link label={t("general.viewHistory")} icon="pi pi-history" className='p-0' iconPos='right' />
                    </div>
                    <div className="grid flex-column md:flex-row">
                        <div className="col-12 md:col-3">
                            <div className="mt-4">
                                <p className="text-xl font-bold">{t("transformatorsInvertors.energyDelivered")}: 100 kWh</p>
                                <div className="w-full text-left text-3xl font-bold border-1 border-gray-400 border-round p-3">
                                    <p>{t("powerGeneral.powerFactor")}: 0.94 P/S</p>
                                    <div className='font-bold text-left text-sm flex gap-3 w-full justify-content-start'>
                                        <div className='flex flex-column'>
                                            <span>Active Power</span>
                                            <span>8 kW</span>
                                        </div>
                                        <div className='flex flex-column'>
                                            <span>Reactive Power</span>
                                            <span>2.6 kVAR</span>
                                        </div>
                                    </div>
                                </div>
                                <p className="text-xl font-bold text-red-500 mt-4">{t("transformatorsInvertors.temperature")}: 52 °C</p>
                            </div>

                        </div>
                        <div className="col-12 md:col-6">
                            <DcFluctuationChart id="dcFluctuationChart" />
                        </div>
                        <div className="col-12 md:col-3">
                            <EfficiencyGauge id="gauge3" data={[]} />
                        </div>
                    </div>
                    <div className="mt-4">
                        <h3 className="text-xl font-bold mb-3">{t("transformatorsInvertors.transformerHealth")}</h3>
                        <HealthLine id="healthLine" />
                    </div>
                </Card>
            </div>
        </div>
    )
}
