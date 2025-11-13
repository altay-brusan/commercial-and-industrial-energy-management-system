'use client';
import { useTranslations } from 'next-intl';
import React from 'react';
import { Button } from 'primereact/button';
import EfficiencyGauge from './component/EfficiencyGauge';
import HealthLine from './component/HealthLine';
import { Card } from 'primereact/card';
import DcFluctuationChart from './component/DcFluctuation';

const TransformatorsInvertorsPage = () => {
    const t = useTranslations();

    return (
        <div className="grid">
            <div className="col-12">
                <Card className='mb-4'>
                    <h2 className='text-3xl font-bold mb-0'>{t("menu.transformators&Invertors")}</h2>
                </Card>
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
                <Card className='mb-4 mt-4'>
                    <h2 className='text-3xl font-bold mb-0'>{t("transformatorsInvertors.transformators")}</h2>
                </Card>
                <Card>
                    <div className='flex w-full align-items-start justify-content-between'>
                        <h2 className="text-2xl font-bold">Transformator 1</h2>
                        <Button text link label={t("general.viewHistory")} icon="pi pi-history" className='p-0' iconPos='right' />
                    </div>
                    <div className="flex flex-column md:flex-row">
                        <div className="flex-1">
                            <div className="mt-4">
                                <p className="text-xl font-bold">{t("transformatorsInvertors.power")}: 30 Kw</p>
                                <p className="text-xl font-bold">{t("transformatorsInvertors.current")}: 80 A</p>
                                <p className="text-xl font-bold">{t("transformatorsInvertors.transformerLoss")}: %5</p>
                                <p className="text-xl font-bold text-red-500">{t("transformatorsInvertors.temperature")}: 52 °C</p>
                            </div>
                        </div>
                        <div className="flex-1">
                            <EfficiencyGauge id="gauge" data={[]} />
                        </div>
                    </div>
                    <div className="mt-4">
                        <h3 className="text-xl font-bold mb-3">{t("transformatorsInvertors.transformerHealth")}</h3>
                        <HealthLine id="healthLine2" />
                    </div>
                </Card>
            </div>
        </div>
    );
};

export default TransformatorsInvertorsPage;
