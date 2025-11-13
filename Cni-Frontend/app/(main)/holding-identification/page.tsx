'use client';
import React, { useState } from 'react'
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { actionBodyTemplate, header, imageBodyTemplate, statusBodyTemplate } from '@/src/utils/datatable-body';
import { getFirms } from '@/src/services/firm';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { deleteFirm as deleteFirmService } from '@/src/services/firm';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';

interface Firm {
    id: number;
    tenantId: number;
    name: string;
    logoUrl: string;
    type: string;
    country: string;
    state: string;
    city: string;
    district: string;
    postalCode: string;
    address: string;
    contactEmail: string;
    contactPhone: string;
    fax: string;
    taxNumber: string;
    taxOffice: string;
    plan: string | null;
}

export default function HoldingIdentification() {
    const { setIsLoading } = useLoading();
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();

    const [firmList, reload] = useFetch<PaginatedList<Firm>>(
        () => getFirms(pageNumber, pageSize),
        { reloadDeps: [pageNumber, pageSize], deps: [pageNumber, pageSize], setLoading: setIsLoading }
    );

    const handleFirmDelete = async (firmId: number) => {
        const [error] = await tryCatch(deleteFirmService(firmId));
        if (error) {
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
        } else {
            openToaster('top-right', {
                severity: 'success',
                summary: t('common.success'),
                detail: t('common.successMessage')
            });

            reload();
        }
    };

    const showDeleteConfirmDialog = (firmId: number) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleFirmDelete(firmId),
            reject: () => { }
        });
    };

    const columns = [
        { field: 'name', header: t('company.name'), style: { minWidth: '12rem' }, sortable: true, filter: true },
        { field: 'logoUrl', header: t('company.logo'), body: imageBodyTemplate, style: { minWidth: '12rem' }, sortable: true, filter: true },
        { field: 'type', header: t('company.type'), sortable: true, filter: true },
        { field: 'country', header: t('company.country'), sortable: true, filter: true },
        { field: 'state', header: t('company.state'), sortable: true, filter: true },
        { field: 'city', header: t('company.city'), sortable: true, filter: true },
        { field: 'district', header: t('company.district'), sortable: true, filter: true },
        { field: 'postalCode', header: t('company.postalCode'), sortable: true, filter: true },
        { field: 'action', header: t('common.actions'), style: { minWidth: '10rem' }, body: (rowData: any) => actionBodyTemplate(rowData, () => router.push(`/holding-identification/edit/${rowData.id}`), () => showDeleteConfirmDialog(rowData.id)) },
    ];

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card title={t('company.title')} contentClassName='p-4' customHeader={<Button onClick={() => { router.push('/holding-identification/add') }} label={t('company.add')} icon={<i className="pi pi-plus mr-2" />} />}>
                <CustomDataTable
                    columns={columns}
                    data={firmList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={firmList?.totalCount}
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
    )
}
