'use client';
import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Card } from 'primereact/card';
import { FaBatteryFull, FaFan, FaThermometerHalf } from 'react-icons/fa';
import { MeterGroup } from 'primereact/metergroup';
import { Button } from 'primereact/button';
import { Divider } from 'primereact/divider';
import { useTranslations } from 'next-intl';
import { BessDevice } from '@/src/models/DashboardRealTimeData';
import { BESSStatus } from '@/src/types/node-types';


interface BESSCardProps {
    data: BessDevice;
    index: number;
}

const BESSCard: React.FC<BESSCardProps> = ({ data, index }) => {
    const t = useTranslations();
    const router = useRouter();
    const [showDetails, setShowDetails] = useState(false);

    const handleToggleDetails = () => {
        setShowDetails(!showDetails);
    };

    const getBESSStatusText = (status: BESSStatus): string => {
        switch (status) {
            case BESSStatus.STANDBY:
                return t("common.standby");
            case BESSStatus.CHARGING:
                return t("common.charging");
            case BESSStatus.DISCHARGING:
                return t("common.discharging");
            case BESSStatus.ERROR:
                return t("common.error");
            default:
                return t("common.unknown");
        }
    };

    const getBESSStatusColor = (status: BESSStatus): string => {
        switch (status) {
            case BESSStatus.STANDBY:
                return "text-yellow-500";
            case BESSStatus.CHARGING:
                return "text-green-500";
            case BESSStatus.DISCHARGING:
                return "text-red-500";
            case BESSStatus.ERROR:
                return "text-red-700";
            default:
                return "text-gray-500";
        }
    };

    const getTemperatureColor = (temp: number) => {
        if (temp >= 39) return 'text-red-500';
        return 'text-green-500';
    };

    const getSOHColor = (soh: number): string => {
        if (soh > 85) return '#22C55E'; // yeşil
        if (soh >= 75) return '#FB923C'; // turuncu
        return '#EF4444'; // kırmızı
    };

    const start = ({ totalPercent }: { totalPercent: number }) => (
        <div className="flex justify-content-between mt-3 mb-2 relative">
            <span>{t('BESS.stateOfCharge')}</span>
            <span className="font-medium">{Number(totalPercent).toFixed(2)}%</span>
        </div>
    );

    return (
        <Card className="mb-2 bg-white rounded-lg shadow relative" style={{ minHeight: '360px' }}>
            <div className="flex justify-content-between align-items-start mb-4">
                <h2 className="text-2xl font-bold mb-0">Bess - {data.deviceId}</h2>
                <div className="flex flex-column align-items-end">
                    <div className="flex justify-content-between align-items-center gap-2">
                        <span className={getBESSStatusColor(data.chargeStatus)}>
                            {getBESSStatusText(data.chargeStatus)}
                        </span>
                        <FaBatteryFull size={24} className={getBESSStatusColor(data.chargeStatus)} />
                    </div>
                </div>
            </div>

            <div className="space-y-4 gap-3 flex flex-column mb-4">
                <div className="w-full rounded-full h-2">
                    <MeterGroup values={[{ label: 'State of Charge', value: Number(data.stateOfCharge), color: getSOHColor(Number(data.stateOfHealth)) }]} labelPosition='start' start={start} labelList={() => null} />
                </div>

                <div className="flex justify-content-between align-items-center ">
                    <span>{t('BESS.powerOutput')}</span>
                    <span>{Number(data.dcPower.power).toFixed(2)} kW</span>
                </div>

                <div className="flex justify-content-between align-items-center ">
                    <span>{t('BESS.temperature')}</span>
                    <div className="flex align-items-center gap-2">
                        <FaThermometerHalf className={getTemperatureColor(data.batteryTemperature)} />
                        <span className={getTemperatureColor(data.batteryTemperature)}>
                            {Number(data.batteryTemperature).toFixed(2)}°C
                        </span>
                    </div>
                </div>

                <div className="flex justify-content-between align-items-center ">
                    <span>{t('BESS.soh')}</span>
                    <span>{Number(data.stateOfHealth).toFixed(2)}%</span>
                </div>

                <div className="flex justify-content-between align-items-center ">
                    <span>{t('BESS.cooling')}</span>
                    <div className="flex align-items-center gap-2 text-blue-500">
                        <FaFan className="animate-spin" />
                        <span>{data.coolingActive ? 'Active' : 'Inactive'}</span>
                    </div>
                </div>
            </div>

            {!showDetails ? (
                <div className='w-full flex justify-content-center absolute bottom-0 left-0'>
                    <Button className='' text onClick={handleToggleDetails}>
                        Show more <i className="pi pi-angle-double-down"></i>
                    </Button>
                </div>
            ) : (
                <>
                    <Divider />
                    <div className="flex flex-column gap-3">
                        <div className="flex justify-content-between ">
                            <span>{t('BESS.type')}</span>
                            <span>LFP/Li-on</span>
                        </div>
                        <div className="flex justify-content-between ">
                            <span>{t('BESS.batteryCapacity')}</span>
                            <span>{Number(data.capacity).toFixed(2)} kWh</span>
                        </div>
                        <div className="flex justify-content-between ">
                            <span>{t('BESS.cellNumber')}</span>
                            <span>100</span>
                        </div>
                        <div className="flex justify-content-between ">
                            <span>{t('BESS.batteryCycles')}</span>
                            <span>9000</span>
                        </div>
                        <Divider />
                        <div className='w-full flex flex-column gap-2'>
                            <Button
                                label={t('general.viewDetails')}
                                icon="pi pi-server"
                                className="w-full"
                                severity="info"
                                outlined
                                onClick={() => router.push('/monitoring/bmu')}
                            />
                        </div>
                        <div className='w-full flex justify-content-center mt-4'>
                            <Button className='' text onClick={handleToggleDetails}>
                                Show less <i className="pi pi-angle-double-up"></i>
                            </Button>
                        </div>
                    </div>
                </>
            )}
        </Card>
    );
};

export default BESSCard;
