/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useTranslations } from 'next-intl';
import Card from '@/src/components/Card';
import Form from '@/src/components/ui/Form';
import { tryCatch } from '@/src/utils/tryCatch';
import { createFacility, getFacilityById, updateFacility } from '@/src/services/facility';
import { useToaster } from '@/src/context/ToasterContext';
import { useParams, useRouter } from 'next/navigation';
import { useLoading } from '@/src/context/LoadingContext';
import { useFetch } from '@/src/hooks/use-fetch';
import { PaginatedList } from '@/src/types/common';
import { getFirms } from '@/src/services/firm';
import { useTenant } from '@/src/context/TenantContext';

type FacilityFormData = {
    name: string;
    address: string;
    capacity: number;
    longitude: number;
    latitude: number;
    tenantId?: string;
};

export default function FacilityIdentificationAction() {
    const router = useRouter();
    const t = useTranslations();
    const { showLoading, hideLoading } = useLoading();
    const { openToaster } = useToaster();
    const params = useParams() as { mode: string };
    const mode = params.mode;
    const modeParts = mode?.split('/');
    const id = modeParts?.[1];
    const isEdit = mode !== 'add';
    const { tenant } = useTenant();
    const [firmList] = useFetch<PaginatedList<any>>(() => getFirms(1, 10000));

    const schema = () =>
        yup.object().shape({
            name: yup.string().required(t('validation.required')),
            address: yup.string().required(t('validation.required')),
            capacity: yup
                .number()
                .typeError(t('validation.required'))
                .required(t('validation.required')),
            longitude: yup
                .number()
                .typeError(t('validation.required'))
                .required(t('validation.required')),
            latitude: yup
                .number()
                .typeError(t('validation.required'))
                .required(t('validation.required')),
        });

    const fields = [
        {
            name: 'firmId',
            label: t('facility.firm'),
            component: 'dropdown',
            options: firmList?.items,
            optionLabel: 'name',
            optionValue: 'id',
        },
        {
            name: 'name',
            label: t('facility.name'),
            component: 'input',
        },
        {
            name: 'capacity',
            label: t('facility.capacity'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'longitude',
            label: t('facility.longitude'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'latitude',
            label: t('facility.latitude'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'address',
            label: t('facility.address'),
            component: 'textarea',
            rows: 5,
            col: 12,
            height: 'auto'
        },
    ];

    const {
        control,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<FacilityFormData>({
        defaultValues: {
            name: '',
            address: '',
            capacity: 0,
            longitude: 0,
            latitude: 0,
        },
        resolver: yupResolver(schema()),
    });

    useEffect(() => {
        if (!isEdit) return;
        const getDetail = async () => {
            showLoading();
            const [error, result] = await tryCatch(getFacilityById(id));
            if (error) {
                openToaster('top-right', {
                    severity: 'error',
                    summary: t('common.error'),
                    detail: error.message,
                });
                hideLoading();
                return;
            }
            reset(result);
            hideLoading();
        };
        getDetail();
    }, [isEdit, id]);

    const onSubmit = async (data: FacilityFormData) => {
        showLoading();
        const tenantId = data.tenantId || tenant;
        const payload = { ...data, tenantId };

        const [error] = isEdit
            ? await tryCatch(updateFacility(id, payload))
            : await tryCatch(createFacility(payload));

        if (error) {
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: error.message,
            });
            hideLoading();
            return;
        }

        openToaster('top-right', {
            severity: 'success',
            summary: t('common.success'),
            detail: t('common.successMessage'),
        });

        router.push('/facility-identification');
        hideLoading();
    };

    return (
        <div className="custom-page p-m-0">
            <div className="col-12 p-0">
                <Card title={t('facility.title')} contentClassName="p-4">
                    <Form
                        mode={isEdit ? 'edit' : 'add'}
                        control={control}
                        fields={fields}
                        onSubmit={handleSubmit(onSubmit)}
                        errors={errors}
                        close={() => router.push('/facility-identification')}
                    />
                </Card>
            </div>
        </div>
    );
}
