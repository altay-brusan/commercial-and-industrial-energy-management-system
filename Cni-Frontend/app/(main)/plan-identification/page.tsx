'use client';
import React, { useState } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { actionBodyTemplate, header, statusBodyTemplate } from '@/src/utils/datatable-body';
import { useFetch } from '@/src/hooks/use-fetch';
import { PaginatedList } from '@/src/types/common';
import { getPlans } from '@/src/services/plans';
import { useLoading } from '@/src/context/LoadingContext';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { tryCatch } from '@/src/utils/tryCatch';
import { deletePlan as deletePlanService } from '@/src/services/plans';

interface Plan {
    id: number;
    planName: string;
    modules: string;
    apiAccess: string;
    facilityCount: number;
    userCount: number;
    price: string;
    status: string;
}

export default function FacilityIdentification() {
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { setIsLoading } = useLoading();
    const { openToaster } = useToaster();

    const [planList, reload] = useFetch<PaginatedList<Plan>>(
        () => getPlans(pageNumber, pageSize),
        { reloadDeps: [pageNumber, pageSize], deps: [pageNumber, pageSize], setLoading: setIsLoading }
    );

    const handlePlanDelete = async (planId: number) => {
        const [error] = await tryCatch(deletePlanService(planId));
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

    const showDeleteConfirmDialog = (planId: number) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handlePlanDelete(planId),
            reject: () => { }
        });
    };

    const columns = [
        { field: 'name', header: t('plan.planName'), style: { minWidth: '12rem' } },
        { field: 'modules', header: t('plan.modules'), style: { minWidth: '12rem' } },
        { field: 'apiAccess', header: t('plan.apiAccess'), style: { minWidth: '12rem' } },
        { field: 'maxFacilities', header: t('plan.maxFacilities'), style: { minWidth: '12rem' } },
        { field: 'maxUsers', header: t('plan.maxUsers'), style: { minWidth: '12rem' } },
        { field: 'price', header: t('plan.price'), style: { minWidth: '12rem' } },
        { field: 'status', header: t('common.status'), style: { minWidth: '12rem' }, body: statusBodyTemplate },
        { field: 'action', header: t('common.actions'), style: { minWidth: '12rem' }, body: (rowData: any) => actionBodyTemplate(rowData, () => router.push(`/plan-identification/edit/${rowData.id}`), () => showDeleteConfirmDialog(rowData.id)) },
    ];

    const data = [
        {
            id: 1,
            planName: 'Temel Plan',
            modules: 'Modül A, Modül B',
            apiAccess: 'Evet',
            facilityCount: 5,
            userCount: 10,
            price: '100 TL',
            status: 'Active',
        },
        {
            id: 2,
            planName: 'Gelişmiş Plan',
            modules: 'Modül A, Modül B, Modül C',
            apiAccess: 'Hayır',
            facilityCount: 10,
            userCount: 20,
            price: '200 TL',
            status: 'Passive',
        },
    ];

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card title={t('plan.pageTitle')} contentClassName='p-4' customHeader={<Button onClick={() => { router.push('/plan-identification/add') }} label={t('plan.add')} icon={<i className="pi pi-plus mr-2" />} />}>
                <CustomDataTable
                    columns={columns}
                    data={planList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={planList?.totalCount}
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
