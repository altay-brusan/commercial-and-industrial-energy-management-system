'use client';
import React, { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { actionBodyTemplate, header } from '@/src/utils/datatable-body';
import { blockedIpService, BlockedIpMail } from '@/src/services/blockedIp';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Dropdown } from 'primereact/dropdown';
import { InputTextarea } from 'primereact/inputtextarea';
import { Checkbox } from 'primereact/checkbox';
import { classNames } from 'primereact/utils';
import { Tag } from 'primereact/tag';

export default function BlockedAddresses() {
    const { setIsLoading } = useLoading();
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();
    const [typeFilter, setTypeFilter] = useState<'IP' | 'EMAIL' | ''>('');

    // Dialog states
    const [dialogVisible, setDialogVisible] = useState(false);
    const [dialogMode, setDialogMode] = useState<'add' | 'edit'>('add');
    const [submitted, setSubmitted] = useState(false);

    // Form state
    const [blockedItem, setBlockedItem] = useState<Partial<BlockedIpMail>>({
        address: '',
        type: 'IP',
        reason: '',
        status: true
    });

    const types = [
        { label: t('blockedAddress.ip'), value: 'IP' },
        { label: t('blockedAddress.email'), value: 'EMAIL' }
    ];

    const [addressList, reload] = useFetch<PaginatedList<BlockedIpMail>>(
        () => blockedIpService.list({ pageNumber, pageSize, type: typeFilter }),
        {
            reloadDeps: [pageNumber, pageSize, typeFilter],
            deps: [pageNumber, pageSize, typeFilter],
            setLoading: setIsLoading
        }
    );

    const handleDelete = async (id: string) => {
        const [error] = await tryCatch(blockedIpService.deleteBlockedAddress(id));
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
            detail: t('common.successMessage')
        });
        reload();
    };

    const showDeleteConfirmDialog = (id: string) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleDelete(id),
            reject: () => { }
        });
    };

    const openAddDialog = () => {
        setBlockedItem({
            address: '',
            type: 'IP',
            reason: '',
            status: true
        });
        setDialogMode('add');
        setDialogVisible(true);
        setSubmitted(false);
    };

    const openEditDialog = (item: BlockedIpMail) => {
        setBlockedItem({ ...item });
        setDialogMode('edit');
        setDialogVisible(true);
        setSubmitted(false);
    };

    const hideDialog = () => {
        setDialogVisible(false);
        setSubmitted(false);
    };

    const saveBlockedItem = async () => {
        setSubmitted(true);

        if (!blockedItem.address || !blockedItem.type || !blockedItem.reason) {
            return;
        }

        try {
            setIsLoading(true);

            if (dialogMode === 'add') {
                await blockedIpService.createBlockedAddress(blockedItem as Omit<BlockedIpMail, 'id' | 'createdAt' | 'updatedAt'>);
                openToaster('top-right', {
                    severity: 'success',
                    summary: t('common.success'),
                    detail: t('blockedAddress.addSuccess')
                });
            } else {
                await blockedIpService.updateBlockedAddress(
                    blockedItem.id as string,
                    {
                        address: blockedItem.address,
                        type: blockedItem.type,
                        reason: blockedItem.reason,
                        status: blockedItem.status
                    }
                );
                openToaster('top-right', {
                    severity: 'success',
                    summary: t('common.success'),
                    detail: t('blockedAddress.updateSuccess')
                });
            }

            hideDialog();
            reload();
        } catch (error) {
            console.error('Error saving blocked item:', error);
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
        } finally {
            setIsLoading(false);
        }
    };

    const toggleStatus = async (item: BlockedIpMail) => {
        try {
            setIsLoading(true);
            await blockedIpService.updateBlockedAddress(
                item.id,
                { status: !item.status }
            );
            openToaster('top-right', {
                severity: 'success',
                summary: t('common.success'),
                detail: t('blockedAddress.statusUpdateSuccess')
            });
            reload();
        } catch (error) {
            console.error('Error toggling status:', error);
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
        } finally {
            setIsLoading(false);
        }
    };

    const statusBodyTemplate = (rowData: BlockedIpMail) => {
        return (
            <Tag
                value={rowData.status ? t('common.active') : t('common.inactive')}
                severity={rowData.status ? 'success' : 'danger'}
            />
        );
    };

    const typeBodyTemplate = (rowData: BlockedIpMail) => {
        return (
            <Tag
                value={rowData.type === 'IP' ? t('blockedAddress.ip') : t('blockedAddress.email')}
                severity={rowData.type === 'IP' ? 'info' : 'warning'}
            />
        );
    };

    const columns = [
        { field: 'address', header: t('blockedAddress.address'), sortable: true, filter: true },
        {
            field: 'type',
            header: t('blockedAddress.type'),
            sortable: true,
            filter: true,
            body: typeBodyTemplate
        },
        { field: 'reason', header: t('blockedAddress.reason'), sortable: true, filter: true },
        {
            field: 'status',
            header: t('blockedAddress.status'),
            sortable: true,
            filter: true,
            body: statusBodyTemplate
        },
        { field: 'createdAt', header: t('common.createdAt'), sortable: true, filter: true },
        {
            field: 'action',
            header: t('common.actions'),
            style: { minWidth: '12rem' },
            body: (rowData: BlockedIpMail) => (
                <>
                    <Button
                        icon={rowData.status ? "pi pi-ban" : "pi pi-check"}
                        rounded
                        outlined
                        severity={rowData.status ? "danger" : "success"}
                        className="mr-2"
                        onClick={() => toggleStatus(rowData)}
                        tooltip={rowData.status ? t('blockedAddress.deactivate') : t('blockedAddress.activate')}
                        tooltipOptions={{ position: 'top' }}
                    />
                    <Button
                        icon="pi pi-pencil"
                        rounded
                        severity="success"
                        className="mr-2"
                        onClick={() => openEditDialog(rowData)}
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
                </>
            )
        }
    ];

    const dialogFooter = (
        <>
            <Button label={t('common.cancel')} icon="pi pi-times" outlined onClick={hideDialog} />
            <Button label={t('common.save')} icon="pi pi-check" onClick={saveBlockedItem} />
        </>
    );

    const filterOptions = [
        { label: t('common.all'), value: '' },
        { label: t('blockedAddress.ip'), value: 'IP' },
        { label: t('blockedAddress.email'), value: 'EMAIL' }
    ];

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card
                title={t('blockedAddress.pageTitle')}
                contentClassName="p-4"
                customHeader={
                    <div className="flex flex-wrap gap-2 justify-content-between align-items-center">
                        <Button
                            onClick={openAddDialog}
                            label={t('blockedAddress.add')}
                            icon={<i className="pi pi-plus mr-2" />}
                        />
                    </div>
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={addressList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={addressList?.totalCount}
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
                visible={dialogVisible}
                style={{ width: '450px', maxWidth: '90vw' }}
                header={dialogMode === 'add' ? t('blockedAddress.add') : t('blockedAddress.edit')}
                modal
                className="p-fluid"
                footer={dialogFooter}
                onHide={hideDialog}
            >
                <div className="field">
                    <label htmlFor="address" className="font-bold">
                        {t('blockedAddress.address')}
                    </label>
                    <InputText
                        id="address"
                        value={blockedItem.address}
                        onChange={(e) => setBlockedItem({ ...blockedItem, address: e.target.value })}
                        required
                        className={classNames({ 'p-invalid': submitted && !blockedItem.address })}
                    />
                    {submitted && !blockedItem.address && (
                        <small className="p-error">{t('common.required')}</small>
                    )}
                </div>
                <div className="field">
                    <label htmlFor="type" className="font-bold">
                        {t('blockedAddress.type')}
                    </label>
                    <Dropdown
                        id="type"
                        value={blockedItem.type}
                        options={types}
                        onChange={(e) => setBlockedItem({ ...blockedItem, type: e.value })}
                        required
                        className={classNames({ 'p-invalid': submitted && !blockedItem.type })}
                    />
                    {submitted && !blockedItem.type && (
                        <small className="p-error">{t('common.required')}</small>
                    )}
                </div>
                <div className="field">
                    <label htmlFor="reason" className="font-bold">
                        {t('blockedAddress.reason')}
                    </label>
                    <InputTextarea
                        id="reason"
                        value={blockedItem.reason}
                        onChange={(e) => setBlockedItem({ ...blockedItem, reason: e.target.value })}
                        required
                        rows={3}
                        className={classNames({ 'p-invalid': submitted && !blockedItem.reason })}
                    />
                    {submitted && !blockedItem.reason && (
                        <small className="p-error">{t('common.required')}</small>
                    )}
                </div>
                <div className="field flex align-items-center">
                    <Checkbox
                        inputId="status"
                        checked={blockedItem.status ?? false}
                        onChange={(e) => setBlockedItem({ ...blockedItem, status: e.checked })}
                    />
                    <label htmlFor="status" className="ml-2">
                        {t('blockedAddress.isActive')}
                    </label>
                </div>
            </Dialog>
        </div>
    );
}
