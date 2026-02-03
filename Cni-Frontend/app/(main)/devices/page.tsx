'use client';
import React, { useState, useMemo } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { header } from '@/src/utils/datatable-body';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { Device, DeviceFilter, DeviceSummary, DeviceType, DeviceStatus } from '@/src/types/device';
import { Tag } from 'primereact/tag';

// Static mock data
const mockDevices: Device[] = [
    { id: '1', name: 'BESS-001', type: 'BESS', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'BSS-2024-001', manufacturer: 'Tesla', model: 'Megapack', capacity: 1000, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-06-01T08:00:00' },
    { id: '2', name: 'BESS-002', type: 'BESS', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'BSS-2024-002', manufacturer: 'Tesla', model: 'Megapack', capacity: 1000, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-06-01T08:00:00' },
    { id: '3', name: 'BMU-001', type: 'BMU', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'BMU-2024-001', manufacturer: 'BYD', model: 'BMU-500', capacity: 500, lastUpdate: '2024-01-15T09:45:00', createdAt: '2023-06-15T09:00:00' },
    { id: '4', name: 'BMU-002', type: 'BMU', status: 'maintenance', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'BMU-2024-002', manufacturer: 'BYD', model: 'BMU-500', capacity: 500, lastUpdate: '2024-01-14T14:20:00', createdAt: '2023-06-15T09:00:00' },
    { id: '5', name: 'INV-001', type: 'Inverter', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'INV-2024-001', manufacturer: 'SMA', model: 'Sunny Tripower', capacity: 100, lastUpdate: '2024-01-15T10:00:00', createdAt: '2023-07-01T10:00:00' },
    { id: '6', name: 'INV-002', type: 'Inverter', status: 'fault', facilityId: 2, facilityName: 'Güneş Çiftliği', serialNumber: 'INV-2024-002', manufacturer: 'SMA', model: 'Sunny Tripower', capacity: 100, lastUpdate: '2024-01-15T08:30:00', createdAt: '2023-07-01T10:00:00' },
    { id: '7', name: 'INV-003', type: 'Inverter', status: 'active', facilityId: 2, facilityName: 'Güneş Çiftliği', serialNumber: 'INV-2024-003', manufacturer: 'Huawei', model: 'SUN2000', capacity: 150, lastUpdate: '2024-01-15T10:15:00', createdAt: '2023-07-15T11:00:00' },
    { id: '8', name: 'SOLAR-001', type: 'Solar', status: 'active', facilityId: 2, facilityName: 'Güneş Çiftliği', serialNumber: 'SLR-2024-001', manufacturer: 'JinkoSolar', model: 'Tiger Pro', capacity: 500, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-08-01T08:00:00' },
    { id: '9', name: 'SOLAR-002', type: 'Solar', status: 'active', facilityId: 2, facilityName: 'Güneş Çiftliği', serialNumber: 'SLR-2024-002', manufacturer: 'JinkoSolar', model: 'Tiger Pro', capacity: 500, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-08-01T08:00:00' },
    { id: '10', name: 'PCS-001', type: 'PCS', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'PCS-2024-001', manufacturer: 'ABB', model: 'PCS100', capacity: 200, lastUpdate: '2024-01-15T09:00:00', createdAt: '2023-09-01T09:00:00' },
    { id: '11', name: 'GEN-001', type: 'Generator', status: 'offline', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'GEN-2024-001', manufacturer: 'Caterpillar', model: 'C18', capacity: 750, lastUpdate: '2024-01-10T16:00:00', createdAt: '2023-05-01T07:00:00' },
    { id: '12', name: 'GRID-001', type: 'Grid', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'GRD-2024-001', manufacturer: 'Siemens', model: 'Grid Connect', capacity: 5000, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-04-01T06:00:00' },
    { id: '13', name: 'EVC-001', type: 'EVCharger', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'EVC-2024-001', manufacturer: 'ABB', model: 'Terra 54', capacity: 50, lastUpdate: '2024-01-15T10:00:00', createdAt: '2023-10-01T10:00:00' },
    { id: '14', name: 'EVC-002', type: 'EVCharger', status: 'inactive', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'EVC-2024-002', manufacturer: 'ABB', model: 'Terra 54', capacity: 50, lastUpdate: '2024-01-12T15:00:00', createdAt: '2023-10-01T10:00:00' },
    { id: '15', name: 'TRAFO-001', type: 'Transformer', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'TRF-2024-001', manufacturer: 'Siemens', model: 'GEAFOL', capacity: 2000, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-03-01T05:00:00' },
    { id: '16', name: 'SM-001', type: 'SmartMeter', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'SM-2024-001', manufacturer: 'Schneider', model: 'ION9000', capacity: 0, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-11-01T11:00:00' },
    { id: '17', name: 'SM-002', type: 'SmartMeter', status: 'active', facilityId: 2, facilityName: 'Güneş Çiftliği', serialNumber: 'SM-2024-002', manufacturer: 'Schneider', model: 'ION9000', capacity: 0, lastUpdate: '2024-01-15T10:30:00', createdAt: '2023-11-01T11:00:00' },
    { id: '18', name: 'HP-001', type: 'HeatPump', status: 'active', facilityId: 1, facilityName: 'Ana Tesis', serialNumber: 'HP-2024-001', manufacturer: 'Daikin', model: 'Altherma 3', capacity: 16, lastUpdate: '2024-01-15T10:00:00', createdAt: '2023-12-01T12:00:00' },
];

export default function DevicesPage() {
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();
    const [filter, setFilter] = useState<DeviceFilter>({});
    const [filtersVisible, setFiltersVisible] = useState(false);
    const [devices, setDevices] = useState<Device[]>(mockDevices);

    // Filter and paginate devices
    const filteredDevices = useMemo(() => {
        let result = [...devices];
        if (filter.type) {
            result = result.filter(d => d.type === filter.type);
        }
        if (filter.status) {
            result = result.filter(d => d.status === filter.status);
        }
        if (globalFilter) {
            const search = globalFilter.toLowerCase();
            result = result.filter(d =>
                d.name.toLowerCase().includes(search) ||
                d.facilityName?.toLowerCase().includes(search) ||
                d.serialNumber?.toLowerCase().includes(search)
            );
        }
        return result;
    }, [devices, filter, globalFilter]);

    const paginatedDevices = useMemo(() => {
        const start = (pageNumber - 1) * pageSize;
        return filteredDevices.slice(start, start + pageSize);
    }, [filteredDevices, pageNumber, pageSize]);

    // Calculate summary from current devices
    const summary: DeviceSummary = useMemo(() => ({
        total: devices.length,
        active: devices.filter(d => d.status === 'active').length,
        fault: devices.filter(d => d.status === 'fault').length,
        maintenance: devices.filter(d => d.status === 'maintenance').length,
        offline: devices.filter(d => d.status === 'offline').length,
    }), [devices]);

    const deviceTypeOptions: { name: string; value: DeviceType }[] = [
        { name: t('device.types.bess'), value: 'BESS' },
        { name: t('device.types.bmu'), value: 'BMU' },
        { name: t('device.types.inverter'), value: 'Inverter' },
        { name: t('device.types.solar'), value: 'Solar' },
        { name: t('device.types.pcs'), value: 'PCS' },
        { name: t('device.types.generator'), value: 'Generator' },
        { name: t('device.types.grid'), value: 'Grid' },
        { name: t('device.types.evcharger'), value: 'EVCharger' },
        { name: t('device.types.heatpump'), value: 'HeatPump' },
        { name: t('device.types.transformer'), value: 'Transformer' },
        { name: t('device.types.smartmeter'), value: 'SmartMeter' },
    ];

    const deviceStatusOptions: { name: string; value: DeviceStatus }[] = [
        { name: t('device.status.active'), value: 'active' },
        { name: t('device.status.inactive'), value: 'inactive' },
        { name: t('device.status.fault'), value: 'fault' },
        { name: t('device.status.maintenance'), value: 'maintenance' },
        { name: t('device.status.offline'), value: 'offline' },
    ];

    const handleDeviceDelete = (deviceId: string) => {
        setDevices(prev => prev.filter(d => d.id !== deviceId));
        openToaster('top-right', {
            severity: 'success',
            summary: t('common.success'),
            detail: t('device.deleteSuccess')
        });
    };

    const showDeleteConfirmDialog = (deviceId: string) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleDeviceDelete(deviceId),
            reject: () => { }
        });
    };

    const getTypeIcon = (type: DeviceType): string => {
        const iconMap: Record<DeviceType, string> = {
            'BESS': 'pi pi-database',
            'BMU': 'pi pi-server',
            'Inverter': 'pi pi-bolt',
            'Solar': 'pi pi-sun',
            'PCS': 'pi pi-cog',
            'Generator': 'pi pi-cog',
            'Grid': 'pi pi-sitemap',
            'EVCharger': 'pi pi-car',
            'HeatPump': 'pi pi-cloud',
            'Transformer': 'pi pi-box',
            'SmartMeter': 'pi pi-gauge',
        };
        return iconMap[type] || 'pi pi-microchip';
    };

    const typeBodyTemplate = (rowData: Device) => {
        if (!rowData.type) {
            return <span>-</span>;
        }
        return (
            <div className="flex align-items-center gap-2">
                <i className={getTypeIcon(rowData.type)} />
                <span>{t(`device.types.${rowData.type.toLowerCase()}`)}</span>
            </div>
        );
    };

    const statusBodyTemplate = (rowData: Device) => {
        const statusMap: Record<DeviceStatus, { color: 'success' | 'info' | 'warning' | 'danger' | 'secondary'; icon: string }> = {
            'active': { color: 'success', icon: 'pi pi-check-circle' },
            'inactive': { color: 'secondary', icon: 'pi pi-minus-circle' },
            'fault': { color: 'danger', icon: 'pi pi-times-circle' },
            'maintenance': { color: 'warning', icon: 'pi pi-wrench' },
            'offline': { color: 'info', icon: 'pi pi-wifi' },
        };

        if (!rowData.status) {
            return <span>-</span>;
        }

        const statusInfo = statusMap[rowData.status] || statusMap.inactive;

        return (
            <Tag severity={statusInfo.color} icon={statusInfo.icon}>
                {t(`device.status.${rowData.status}`)}
            </Tag>
        );
    };

    const dateBodyTemplate = (rowData: Device) => {
        if (!rowData.lastUpdate) {
            return <span>-</span>;
        }
        const date = new Date(rowData.lastUpdate);
        return date.toLocaleString();
    };

    const actionBodyTemplate = (rowData: Device) => {
        return (
            <div className="flex justify-content-end gap-2">
                <Button
                    icon="pi pi-eye"
                    rounded
                    severity="info"
                    onClick={() => router.push(`/devices/detail/${rowData.id}`)}
                    tooltip={t('general.viewDetails')}
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-pencil"
                    rounded
                    severity="success"
                    onClick={() => router.push(`/devices/edit/${rowData.id}`)}
                    tooltip={t('common.edit')}
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-trash"
                    rounded
                    severity="warning"
                    onClick={() => showDeleteConfirmDialog(rowData.id)}
                    tooltip={t('common.delete')}
                    tooltipOptions={{ position: 'top' }}
                />
            </div>
        );
    };

    const columns = [
        { field: 'name', header: t('device.name'), sortable: true, filter: true },
        {
            field: 'type',
            header: t('common.type'),
            sortable: true,
            filter: true,
            body: typeBodyTemplate
        },
        {
            field: 'status',
            header: t('common.status'),
            sortable: true,
            filter: true,
            body: statusBodyTemplate
        },
        { field: 'facilityName', header: t('device.facility'), sortable: true, filter: true },
        {
            field: 'lastUpdate',
            header: t('device.lastUpdate'),
            sortable: true,
            filter: true,
            body: dateBodyTemplate
        },
        {
            field: 'action',
            header: t('common.actions'),
            style: { minWidth: '12rem' },
            body: actionBodyTemplate
        }
    ];

    const resetFilters = () => {
        setFilter({});
    };

    const applyFilters = () => {
        setFiltersVisible(false);
        setPageNumber(1);
        setFirst(0);
    };

    const renderFilters = () => {
        return (
            <Dialog
                header={t('device.filters')}
                visible={filtersVisible}
                onHide={() => setFiltersVisible(false)}
                style={{ width: '450px' }}
                footer={(
                    <div className="flex justify-content-end gap-2">
                        <Button
                            label={t('common.reset')}
                            icon="pi pi-refresh"
                            severity="secondary"
                            onClick={resetFilters}
                        />
                        <Button
                            label={t('common.filter')}
                            icon="pi pi-check"
                            onClick={applyFilters}
                        />
                    </div>
                )}
            >
                <div className="field mb-4">
                    <label htmlFor="type" className="font-bold block mb-2">{t('common.type')}</label>
                    <Dropdown
                        id="type"
                        value={filter.type}
                        onChange={(e) => setFilter(prev => ({ ...prev, type: e.value }))}
                        options={deviceTypeOptions}
                        optionLabel="name"
                        optionValue="value"
                        placeholder={t('device.selectType')}
                        className="w-full"
                        showClear
                    />
                </div>

                <div className="field mb-4">
                    <label htmlFor="status" className="font-bold block mb-2">{t('common.status')}</label>
                    <Dropdown
                        id="status"
                        value={filter.status}
                        onChange={(e) => setFilter(prev => ({ ...prev, status: e.value }))}
                        options={deviceStatusOptions}
                        optionLabel="name"
                        optionValue="value"
                        placeholder={t('device.selectStatus')}
                        className="w-full"
                        showClear
                    />
                </div>
            </Dialog>
        );
    };

    const renderSummaryCards = () => {
        const cards = [
            { label: t('device.summary.total'), value: summary?.total || 0, icon: 'pi pi-microchip', color: 'bg-blue-100 text-blue-600' },
            { label: t('device.summary.active'), value: summary?.active || 0, icon: 'pi pi-check-circle', color: 'bg-green-100 text-green-600' },
            { label: t('device.summary.fault'), value: summary?.fault || 0, icon: 'pi pi-times-circle', color: 'bg-red-100 text-red-600' },
            { label: t('device.summary.maintenance'), value: summary?.maintenance || 0, icon: 'pi pi-wrench', color: 'bg-yellow-100 text-yellow-600' },
        ];

        return (
            <div className="grid mb-4">
                {cards.map((card, index) => (
                    <div key={index} className="col-12 md:col-6 lg:col-3">
                        <div className="surface-card shadow-2 p-3 border-round">
                            <div className="flex justify-content-between align-items-center mb-3">
                                <span className="text-500 font-medium">{card.label}</span>
                                <div className={`flex align-items-center justify-content-center border-round ${card.color}`} style={{ width: '2.5rem', height: '2.5rem' }}>
                                    <i className={card.icon} />
                                </div>
                            </div>
                            <div className="text-900 font-bold text-2xl">{card.value}</div>
                        </div>
                    </div>
                ))}
            </div>
        );
    };

    return (
        <div className="custom-page">
            <ConfirmDialog />
            {renderFilters()}

            {renderSummaryCards()}

            <Card
                title={t('device.pageTitle')}
                contentClassName="p-4"
                customHeader={
                    <div className="flex flex-column md:flex-row gap-2">
                        <Button
                            label={t('device.filters')}
                            icon="pi pi-filter"
                            severity="info"
                            onClick={() => setFiltersVisible(true)}
                        />
                        <Button
                            onClick={() => router.push('/devices/add')}
                            label={t('device.add')}
                            icon={<i className="pi pi-plus mr-2" />}
                        />
                    </div>
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={paginatedDevices}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={filteredDevices.length}
                    rows={pageSize}
                    first={first}
                    onPageChange={(e) => {
                        setPageNumber(e.page + 1);
                        setPageSize(e.rows);
                        setFirst(e.first);
                    }}
                />
            </Card>
        </div>
    );
}
