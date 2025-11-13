'use client';
import React, { useState, useRef } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { actionBodyTemplate, header } from '@/src/utils/datatable-body';
import { notificationService } from '@/src/services/notification';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputText } from 'primereact/inputtext';
import { Notification, NotificationFilter } from '@/src/types/notification';
import { Calendar } from 'primereact/calendar';
import { Tag } from 'primereact/tag';
import { Badge } from 'primereact/badge';

export default function NotificationPage() {
    const { setIsLoading } = useLoading();
    const t = useTranslations();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();
    const [filter, setFilter] = useState<NotificationFilter>({});
    const [detailDialogVisible, setDetailDialogVisible] = useState(false);
    const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null);
    const [filtersVisible, setFiltersVisible] = useState(false);

    const [notificationList, reload] = useFetch<PaginatedList<Notification>>(
        () => notificationService.list({ pageNumber, pageSize, filter }),
        { reloadDeps: [pageNumber, pageSize, filter], deps: [pageNumber, pageSize, filter], setLoading: setIsLoading }
    );

    const severityOptions = [
        { name: t('notification.info'), value: 'info' },
        { name: t('notification.warning'), value: 'warning' },
        { name: t('notification.error'), value: 'error' },
        { name: t('notification.success'), value: 'success' }
    ];

    const statusOptions = [
        { name: t('notification.read'), value: 'read' },
        { name: t('notification.unread'), value: 'unread' },
        { name: t('notification.archived'), value: 'archived' }
    ];

    const openDetailDialog = (notification: Notification) => {
        if (notification.status === 'unread') {
            handleStatusChange(notification.id, 'read');
        }
        setSelectedNotification(notification);
        setDetailDialogVisible(true);
    };

    const hideDetailDialog = () => {
        setDetailDialogVisible(false);
        setSelectedNotification(null);
    };

    const handleStatusChange = async (notificationId: string, status: 'read' | 'unread' | 'archived') => {
        setIsLoading(true);
        const [error] = await tryCatch(notificationService.updateStatus(notificationId, status));
        setIsLoading(false);

        if (error) {
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
            return;
        }

        openToaster('top-right', {
            severity: 'success',
            summary: t('common.success'),
            detail: t('notification.statusUpdated')
        });

        reload();
    };

    const handleMarkAllAsRead = async () => {
        setIsLoading(true);
        const [error] = await tryCatch(notificationService.markAllAsRead());
        setIsLoading(false);

        if (error) {
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
            return;
        }

        openToaster('top-right', {
            severity: 'success',
            summary: t('common.success'),
            detail: t('notification.allMarkedAsRead')
        });

        reload();
    };

    const handleDelete = async (notificationId: string) => {
        setIsLoading(true);
        const [error] = await tryCatch(notificationService.deleteNotification(notificationId));
        setIsLoading(false);

        if (error) {
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
            return;
        }

        openToaster('top-right', {
            severity: 'success',
            summary: t('common.success'),
            detail: t('notification.deleted')
        });

        reload();
    };

    const showDeleteConfirmDialog = (notificationId: string) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleDelete(notificationId),
            reject: () => { }
        });
    };

    const severityBodyTemplate = (rowData: Notification) => {
        const severityMap: Record<string, { color: string, icon: string }> = {
            'info': { color: 'info', icon: 'pi pi-info-circle' },
            'warning': { color: 'warning', icon: 'pi pi-exclamation-triangle' },
            'error': { color: 'danger', icon: 'pi pi-times-circle' },
            'success': { color: 'success', icon: 'pi pi-check-circle' }
        };

        const severityInfo = severityMap[rowData.severity] || severityMap.info;

        return (
            <Tag
                severity={severityInfo.color as any}
                icon={severityInfo.icon}
            >
                {t(`notification.${rowData.severity}`)}
            </Tag>
        );
    };

    const statusBodyTemplate = (rowData: Notification) => {
        const statusMap: Record<string, { color: string, icon: string }> = {
            'read': { color: 'info', icon: 'pi pi-eye' },
            'unread': { color: 'warning', icon: 'pi pi-eye-slash' },
            'archived': { color: 'secondary', icon: 'pi pi-inbox' }
        };

        const statusInfo = statusMap[rowData.status] || statusMap.unread;

        return (
            <Tag
                severity={statusInfo.color as any}
                icon={statusInfo.icon}
            >
                {t(`notification.${rowData.status}`)}
            </Tag>
        );
    };

    const dateBodyTemplate = (rowData: Notification) => {
        const date = new Date(rowData.createdAt);
        return date.toLocaleString();
    };

    const titleBodyTemplate = (rowData: Notification) => {
        return (
            <div className="flex align-items-center gap-2">
                <span className={rowData.status === 'unread' ? 'font-bold' : ''}>
                    {rowData.title}
                </span>
                {rowData.status === 'unread' && (
                    <Badge value="2" severity="danger" />
                )}
            </div>
        );
    };

    const actionBodyTemplate = (rowData: Notification) => {
        return (
            <div className="flex justify-content-end gap-2">
                <Button
                    icon="pi pi-eye"
                    rounded
                    severity="info"
                    onClick={() => openDetailDialog(rowData)}
                    tooltip={t('notification.view')}
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-inbox"
                    rounded
                    severity="secondary"
                    onClick={() => handleStatusChange(rowData.id, 'archived')}
                    tooltip={t('notification.archive')}
                    tooltipOptions={{ position: 'top' }}
                />
                <Button
                    icon="pi pi-trash"
                    rounded
                    severity="danger"
                    onClick={() => showDeleteConfirmDialog(rowData.id)}
                    tooltip={t('notification.delete')}
                    tooltipOptions={{ position: 'top' }}
                />
            </div>
        );
    };

    const columns = [
        {
            field: 'title',
            header: t('notification.title'),
            sortable: true,
            filter: true,
            body: titleBodyTemplate
        },
        {
            field: 'deviceName',
            header: t('notification.device'),
            sortable: true,
            filter: true
        },
        {
            field: 'severity',
            header: t('notification.severity'),
            sortable: true,
            filter: true,
            body: severityBodyTemplate
        },
        {
            field: 'status',
            header: t('notification.status'),
            sortable: true,
            filter: true,
            body: statusBodyTemplate
        },
        {
            field: 'createdAt',
            header: t('notification.date'),
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
                header={t('notification.filters')}
                visible={filtersVisible}
                onHide={() => setFiltersVisible(false)}
                style={{ width: '450px' }}
                footer={(
                    <div className="flex justify-content-end gap-2">
                        <Button
                            label={t('notification.resetFilters')}
                            icon="pi pi-refresh"
                            severity="secondary"
                            onClick={resetFilters}
                        />
                        <Button
                            label={t('notification.applyFilters')}
                            icon="pi pi-check"
                            onClick={applyFilters}
                        />
                    </div>
                )}
            >
                <div className="field mb-4">
                    <label htmlFor="severity" className="font-bold block mb-2">{t('notification.severity')}</label>
                    <Dropdown
                        id="severity"
                        value={filter.severity}
                        onChange={(e) => setFilter(prev => ({ ...prev, severity: e.value }))}
                        options={severityOptions}
                        optionLabel="name"
                        placeholder={t('notification.selectSeverity')}
                        className="w-full"
                    />
                </div>

                <div className="field mb-4">
                    <label htmlFor="status" className="font-bold block mb-2">{t('notification.status')}</label>
                    <Dropdown
                        id="status"
                        value={filter.status}
                        onChange={(e) => setFilter(prev => ({ ...prev, status: e.value }))}
                        options={statusOptions}
                        optionLabel="name"
                        placeholder={t('notification.selectStatus')}
                        className="w-full"
                    />
                </div>

                <div className="field mb-4">
                    <label htmlFor="deviceId" className="font-bold block mb-2">{t('notification.deviceId')}</label>
                    <InputText
                        id="deviceId"
                        value={filter.deviceId || ''}
                        onChange={(e) => setFilter(prev => ({ ...prev, deviceId: e.target.value }))}
                        placeholder={t('notification.enterDeviceId')}
                        className="w-full"
                    />
                </div>

                <div className="field mb-4">
                    <label htmlFor="fromDate" className="font-bold block mb-2">{t('notification.fromDate')}</label>
                    <Calendar
                        id="fromDate"
                        value={filter.fromDate ? new Date(filter.fromDate) : undefined}
                        onChange={(e) => setFilter(prev => ({ ...prev, fromDate: e.value ? e.value.toISOString() : undefined }))}
                        showTime
                        placeholder={t('notification.selectFromDate')}
                        className="w-full"
                    />
                </div>

                <div className="field mb-4">
                    <label htmlFor="toDate" className="font-bold block mb-2">{t('notification.toDate')}</label>
                    <Calendar
                        id="toDate"
                        value={filter.toDate ? new Date(filter.toDate) : undefined}
                        onChange={(e) => setFilter(prev => ({ ...prev, toDate: e.value ? e.value.toISOString() : undefined }))}
                        showTime
                        placeholder={t('notification.selectToDate')}
                        className="w-full"
                    />
                </div>
            </Dialog>
        );
    };

    return (
        <div className="custom-page">
            <ConfirmDialog />
            {renderFilters()}

            <Card
                title={t('notification.pageTitle')}
                contentClassName="p-4"
                customHeader={
                    <div className="flex flex-column md:flex-row gap-2">
                        <Button
                            label={t('notification.filters')}
                            icon="pi pi-filter"
                            severity="info"
                            onClick={() => setFiltersVisible(true)}
                        />
                        <Button
                            label={t('notification.markAllAsRead')}
                            icon="pi pi-check"
                            severity="success"
                            onClick={handleMarkAllAsRead}
                        />
                    </div>
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={notificationList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={notificationList?.totalCount}
                    rows={pageSize}
                    first={first}
                    onPageChange={(e) => {
                        setPageNumber(e.page + 1);
                        setPageSize(e.rows);
                        setFirst(e.first);
                    }}
                />
            </Card>

            <Dialog
                visible={detailDialogVisible}
                style={{ width: '600px' }}
                header={t('notification.details')}
                modal
                onHide={hideDetailDialog}
                footer={
                    <div className="flex justify-content-end">
                        <Button
                            label={t('common.close')}
                            icon="pi pi-times"
                            severity="secondary"
                            onClick={hideDetailDialog}
                        />
                    </div>
                }
            >
                {selectedNotification && (
                    <div className="grid">
                        <div className="col-12 md:col-6">
                            <h5>{t('notification.title')}</h5>
                            <p>{selectedNotification.title}</p>
                        </div>
                        <div className="col-12 md:col-6">
                            <h5>{t('notification.date')}</h5>
                            <p>{new Date(selectedNotification.createdAt).toLocaleString()}</p>
                        </div>
                        <div className="col-12 md:col-6">
                            <h5>{t('notification.severity')}</h5>
                            <div>{severityBodyTemplate(selectedNotification)}</div>
                        </div>
                        <div className="col-12 md:col-6">
                            <h5>{t('notification.status')}</h5>
                            <div>{statusBodyTemplate(selectedNotification)}</div>
                        </div>
                        {selectedNotification.deviceName && (
                            <div className="col-12 md:col-6">
                                <h5>{t('notification.device')}</h5>
                                <p>{selectedNotification.deviceName}</p>
                            </div>
                        )}
                        {selectedNotification.deviceId && (
                            <div className="col-12 md:col-6">
                                <h5>{t('notification.deviceId')}</h5>
                                <p>{selectedNotification.deviceId}</p>
                            </div>
                        )}
                        <div className="col-12">
                            <h5>{t('notification.message')}</h5>
                            <p style={{ whiteSpace: 'pre-wrap' }}>{selectedNotification.message}</p>
                        </div>
                    </div>
                )}
            </Dialog>
        </div>
    );
}