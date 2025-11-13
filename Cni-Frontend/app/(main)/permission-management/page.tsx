'use client';
import React, { useState } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { header } from '@/src/utils/datatable-body';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';

import { deleteRole, getRoles } from '@/src/services/roles';

export default function PermissionManagement() {
    const { setIsLoading } = useLoading();
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();

    const [roleList, reload] = useFetch<PaginatedList<{ id: string, name: string }>>(() => getRoles(pageNumber, pageSize));


    const handleRoleDelete = async (roleId: string) => {
        const [error] = await tryCatch(deleteRole(roleId));
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

    const showDeleteConfirmDialog = (roleId: string) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleRoleDelete(roleId),
            reject: () => { }
        });
    };

    const columns = [
        { field: 'name', header: t('role.name'), sortable: true, filter: true },
        { field: 'description', header: t('role.description'), sortable: true, filter: true },
        {
            field: 'action',
            header: t('common.actions'),
            style: { minWidth: '12rem' },
            body: (rowData: any) => (
                <div className="flex justify-content-end">
                    <Button icon="pi pi-pencil" rounded severity="success" className="mr-2" onClick={() => router.push(`/permission-management/edit/${rowData.id}`)} />
                    <Button icon="pi pi-trash" rounded severity="warning" onClick={() => showDeleteConfirmDialog(rowData.id)} />
                </div>
            )
        }
    ];

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card
                title={t('role.pageTitle')}
                contentClassName="p-4"
                customHeader={
                    <Button
                        onClick={() => router.push('/permission-management/add')}
                        label={t('role.add')}
                        icon={<i className="pi pi-plus mr-2" />}
                    />
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={roleList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={roleList?.totalCount}
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
