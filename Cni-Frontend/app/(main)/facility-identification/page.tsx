'use client';
import React, { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Button } from 'primereact/button';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { actionBodyTemplate, header } from '@/src/utils/datatable-body';
import { getFacilities, deleteFacility as deleteFacilityService } from '@/src/services/facility';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';

interface Facility {
    id: number;
    tenantId: number;
    firmId: number;
    name: string;
    address: string;
    capacity: number;
    longitude: number;
    latitude: number;
}

const FacilityIdentification = () => {
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState<string>('');
    const { setIsLoading } = useLoading();
    const [pageNumber, setPageNumber] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [first, setFirst] = useState<number>(0);
    const { openToaster } = useToaster();

    const [facilityList, reload] = useFetch<PaginatedList<Facility>>(
        () => getFacilities(pageNumber, pageSize),
        { reloadDeps: [pageNumber, pageSize], deps: [pageNumber, pageSize], setLoading: setIsLoading }
    );

    const handleFacilityDelete = async (facilityId: number) => {
        const [error] = await tryCatch(deleteFacilityService(facilityId));
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

    const showDeleteConfirmDialog = (facilityId: number) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleFacilityDelete(facilityId),
            reject: () => {}
        });
    };

    const columns = [
        { field: 'name', header: t('facility.name'), sortable: true, filter: true },
        { field: 'address', header: t('facility.address'), sortable: true, filter: true },
        { field: 'capacity', header: t('facility.capacity'), sortable: true, filter: true },
        { field: 'longitude', header: t('facility.longitude'), sortable: true, filter: true },
        { field: 'latitude', header: t('facility.latitude'), sortable: true, filter: true },
        {
            field: 'action',
            header: t('common.actions'),
            body: (rowData: Facility) =>
                actionBodyTemplate(
                    rowData,
                    () => router.push(`/facility-identification/edit/${rowData.id}`),
                    () => showDeleteConfirmDialog(rowData.id)
                )
        }
    ];

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card
                title={t('facility.title')}
                contentClassName="p-4"
                customHeader={
                    <Button
                        onClick={() => router.push('/facility-identification/add')}
                        label={t('facility.add')}
                        icon={<i className="pi pi-plus mr-2" />}
                    />
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={facilityList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={facilityList?.totalCount}
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
};

export default FacilityIdentification;
