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
import { deviceService } from '@/src/services/device';
import { useToaster } from '@/src/context/ToasterContext';
import { useParams, useRouter } from 'next/navigation';
import { useLoading } from '@/src/context/LoadingContext';
import { useFetch } from '@/src/hooks/use-fetch';
import { PaginatedList } from '@/src/types/common';
import { getFacilities } from '@/src/services/facility';
import { DeviceType, DeviceStatus } from '@/src/types/device';

type DeviceFormData = {
    name: string;
    type: DeviceType;
    status: DeviceStatus;
    facilityId: number;
    serialNumber?: string;
    manufacturer?: string;
    model?: string;
    capacity?: number;
    latitude?: number;
    longitude?: number;
    description?: string;
};

export default function DeviceFormPage() {
    const router = useRouter();
    const t = useTranslations();
    const { showLoading, hideLoading } = useLoading();
    const { openToaster } = useToaster();
    const { mode } = useParams() as { mode: string };
    const modeParts = mode?.split('/');
    const pageMode = modeParts?.[0]; // 'add', 'edit', or 'detail'
    const id = modeParts?.[1];
    const isEdit = pageMode === 'edit';
    const isDetail = pageMode === 'detail';
    const isReadOnly = isDetail;

    const [facilityList] = useFetch<PaginatedList<any>>(() => getFacilities(1, 10000));

    const deviceTypeOptions = [
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

    const deviceStatusOptions = [
        { name: t('device.status.active'), value: 'active' },
        { name: t('device.status.inactive'), value: 'inactive' },
        { name: t('device.status.fault'), value: 'fault' },
        { name: t('device.status.maintenance'), value: 'maintenance' },
        { name: t('device.status.offline'), value: 'offline' },
    ];

    const schema = () =>
        yup.object().shape({
            name: yup.string().required(t('validation.required')),
            type: yup.string().required(t('validation.required')),
            status: yup.string().required(t('validation.required')),
            facilityId: yup
                .number()
                .typeError(t('validation.required'))
                .required(t('validation.required')),
            serialNumber: yup.string().optional(),
            manufacturer: yup.string().optional(),
            model: yup.string().optional(),
            capacity: yup.number().optional().nullable(),
            latitude: yup.number().optional().nullable(),
            longitude: yup.number().optional().nullable(),
            description: yup.string().optional(),
        });

    const fields = [
        // Basic Information
        {
            name: 'name',
            label: t('device.name'),
            component: 'input',
            disabled: isReadOnly,
        },
        {
            name: 'type',
            label: t('common.type'),
            component: 'dropdown',
            options: deviceTypeOptions,
            optionLabel: 'name',
            optionValue: 'value',
            disabled: isReadOnly,
        },
        {
            name: 'status',
            label: t('common.status'),
            component: 'dropdown',
            options: deviceStatusOptions,
            optionLabel: 'name',
            optionValue: 'value',
            disabled: isReadOnly,
        },
        {
            name: 'facilityId',
            label: t('device.facility'),
            component: 'dropdown',
            options: facilityList?.items,
            optionLabel: 'name',
            optionValue: 'id',
            disabled: isReadOnly,
        },
        // Technical Information
        {
            name: 'serialNumber',
            label: t('device.serialNumber'),
            component: 'input',
            disabled: isReadOnly,
        },
        {
            name: 'manufacturer',
            label: t('device.manufacturer'),
            component: 'input',
            disabled: isReadOnly,
        },
        {
            name: 'model',
            label: t('device.model'),
            component: 'input',
            disabled: isReadOnly,
        },
        {
            name: 'capacity',
            label: t('device.capacity'),
            component: 'input',
            type: 'number',
            disabled: isReadOnly,
        },
        // Location Information
        {
            name: 'latitude',
            label: t('device.latitude'),
            component: 'input',
            type: 'number',
            disabled: isReadOnly,
        },
        {
            name: 'longitude',
            label: t('device.longitude'),
            component: 'input',
            type: 'number',
            disabled: isReadOnly,
        },
        {
            name: 'description',
            label: t('device.description'),
            component: 'textarea',
            rows: 5,
            col: 12,
            height: 'auto',
            disabled: isReadOnly,
        },
    ];

    const {
        control,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<DeviceFormData>({
        defaultValues: {
            name: '',
            type: 'BESS',
            status: 'active',
            facilityId: undefined,
            serialNumber: '',
            manufacturer: '',
            model: '',
            capacity: undefined,
            latitude: undefined,
            longitude: undefined,
            description: '',
        },
        resolver: yupResolver(schema()),
    });

    useEffect(() => {
        if (pageMode === 'add' || !id) return;
        const getDetail = async () => {
            showLoading();
            const [error, result] = await tryCatch(deviceService.detail(id));
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
    }, [pageMode, id]);

    const onSubmit = async (data: DeviceFormData) => {
        if (isReadOnly) return;

        showLoading();
        const payload = { ...data };

        const [error] = isEdit
            ? await tryCatch(deviceService.update(id, payload))
            : await tryCatch(deviceService.create(payload));

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

        router.push('/devices');
        hideLoading();
    };

    const getPageTitle = () => {
        if (isDetail) return t('device.detail');
        if (isEdit) return t('device.edit');
        return t('device.add');
    };

    return (
        <div className="custom-page p-m-0">
            <div className="col-12 p-0">
                <Card title={getPageTitle()} contentClassName="p-4">
                    <Form
                        mode={isReadOnly ? 'detail' : (isEdit ? 'edit' : 'add')}
                        control={control}
                        fields={fields}
                        onSubmit={handleSubmit(onSubmit)}
                        errors={errors}
                        close={() => router.push('/devices')}
                    />
                </Card>
            </div>
        </div>
    );
}
