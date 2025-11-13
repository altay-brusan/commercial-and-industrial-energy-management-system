/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import Form from '@/src/components/ui/Form'
import React, { useEffect } from 'react'
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useTranslations } from 'next-intl';
import Card from '@/src/components/Card';
import { Divider } from 'primereact/divider';
import { tryCatch } from '@/src/utils/tryCatch';
import { createFirm, getFirmById, updateFirm } from '@/src/services/firm';
import { useToaster } from '@/src/context/ToasterContext';
import { useParams, useRouter } from 'next/navigation';
import { useLoading } from '@/src/context/LoadingContext';
import { PaginatedList } from '@/src/types/common';
import { useFetch } from '@/src/hooks/use-fetch';
import { getTenants } from '@/src/services/tenant';
import { ROLES } from '@/src/utils/roles';

export default function HoldingIdentificationAction() {
    const router = useRouter();
    const t = useTranslations();
    const { showLoading, hideLoading } = useLoading();
    const { openToaster } = useToaster();
    const params = useParams() as { mode: string };
    const mode = params.mode;
    const modeParts = mode?.split('/');
    const id = modeParts?.[1];
    const isEdit = mode !== 'add';
    const user = localStorage.getItem("userInfo");
    const userInfo = user ? JSON.parse(user) : null;

    const [tenantList] = useFetch<PaginatedList<any>>(() => getTenants(1, 10000));

    const schema = () =>
        yup.object().shape({
            name: yup.string().required(t('validation.required')),
            logoUrl: yup.mixed(),
            type: yup.string().required(t('validation.required')),
            country: yup.string().required(t('validation.required')),
            state: yup.string().required(t('validation.required')),
            city: yup.string().required(t('validation.required')),
            district: yup.string().required(t('validation.required')),
            postalCode: yup.string().required(t('validation.required')),
            address: yup.string().required(t('validation.required')),
            contactEmail: yup.string().email(t('validation.email')).required(t('validation.required')),
            contactPhone: yup.string().required(t('validation.required')),
            fax: yup.string(),
            taxNumber: yup.string().required(t('validation.required')),
            taxOffice: yup.string().required(t('validation.required'))
        });

    const fields = [
        {
            name: 'tenantId',
            label: t('common.tenant'),
            component: 'dropdown',
            options: tenantList?.items,
            optionLabel: 'name',
            optionValue: 'id',
            hide: !userInfo?.roles.includes(ROLES.ADMINISTRATOR)
        },
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align='left' type='dashed' className='my-2 text-lg'>
                    <span className="p-tag">{t('company.generalInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'name',
            label: t('company.name'),
            component: 'input',
        },
        {
            name: 'logoUrl',
            label: t('company.logo'),
            component: 'file',
            accept: 'image/*'
        },
        {
            name: 'type',
            label: t('company.type'),
            component: 'dropdown',
            options: [
                { value: 'corporation', label: t('company.types.corporation') },
                { value: 'llc', label: t('company.types.llc') },
                { value: 'partnership', label: t('company.types.partnership') }
            ],
            optionLabel: 'label',
            optionValue: 'value',
        },
        {
            name: 'country',
            label: t('company.country'),
            component: 'dropdown',
            options: [
                { value: 'TR', label: 'Türkiye' },
                { value: 'US', label: 'Amerika Birleşik Devletleri' },
                { value: 'DE', label: 'Almanya' },
                { value: 'FR', label: 'Fransa' },
                { value: 'GB', label: 'Birleşik Krallık' }
            ],
            optionLabel: 'label',
            optionValue: 'value',
        },
        {
            name: 'state',
            label: t('company.state'),
            component: 'dropdown',
            options: [
                { value: 'active', label: t('common.active') },
                { value: 'inactive', label: t('common.inactive') }
            ],
            optionLabel: 'label',
            optionValue: 'value',
        },
        {
            name: 'city',
            label: t('company.city'),
            component: 'dropdown',
            options: [
                { value: 'istanbul', label: 'İstanbul' },
                { value: 'ankara', label: 'Ankara' },
                { value: 'izmir', label: 'İzmir' },
                { value: 'bursa', label: 'Bursa' },
                { value: 'adana', label: 'Adana' }
            ],
            optionLabel: 'label',
            optionValue: 'value',
        },
        {
            name: 'district',
            label: t('company.district'),
            component: 'dropdown',
            options: [
                { value: 'beşiktaş', label: 'Beşiktaş' },
                { value: 'kadıköy', label: 'Kadıköy' },
                { value: 'esenyurt', label: 'Esenyurt' },
                { value: 'kartal', label: 'Kartal' },
                { value: 'sultanbeyli', label: 'Sultanbeyli' }
            ],
            optionLabel: 'label',
            optionValue: 'value',
        },
        {
            name: 'postalCode',
            label: t('company.postalCode'),
            component: 'input',
        },
        {
            name: 'address',
            label: t('company.address'),
            component: 'textarea',
            col: 12,
            rows: 5,
            height: 'auto',
        },
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align='left' type='dashed' className='my-2 text-lg'>
                    <span className="p-tag">{t('company.contactInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'contactEmail',
            label: t('company.contactEmail'),
            component: 'input',
            type: 'email',
        },
        {
            name: 'contactPhone',
            label: t('company.contactPhone'),
            component: 'input',
            type: 'phone',
        },
        {
            name: 'fax',
            label: t('company.fax'),
            component: 'input',
            type: 'phone'
        },
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align='left' type='dashed' className='my-2 text-lg'>
                    <span className="p-tag">{t('company.taxInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'taxNumber',
            label: t('company.taxNumber'),
            component: 'input',
        },
        {
            name: 'taxOffice',
            label: t('company.taxOffice'),
            component: 'input',
        },
    ];

    const {
        control,
        handleSubmit,
        formState: { errors },
        reset
    } = useForm({
        defaultValues: {
            name: undefined,
            logoUrl: undefined,
            type: undefined,
            country: undefined,
            state: undefined,
            city: undefined,
            district: undefined,
            postalCode: undefined,
            address: undefined,
            contactEmail: undefined,
            contactPhone: undefined,
            fax: undefined,
            taxNumber: undefined,
            taxOffice: undefined
        },
        resolver: yupResolver(schema())
    });

    useEffect(() => {
        const getDetail = async () => {
            showLoading();
            const [error, result] = await tryCatch(getFirmById(id));
            if (error) {
                openToaster('top-right', {
                    severity: 'error',
                    summary: t('common.error'),
                    detail: error.message
                });
                hideLoading();
            } else {
                reset(result);
                hideLoading();
            }
        };

        if (isEdit) {
            getDetail();
        }
    }, [isEdit, id, openToaster, t]);

    const onSubmit = async (data: any) => {
        showLoading();
        const tenantId = data.tenantId || userInfo?.tenant?.id;

        const [error] = isEdit
            ? await tryCatch(updateFirm(id, { ...data, tenantId }))
            : await tryCatch(createFirm({ ...data, tenantId }));

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

        router.push('/holding-identification');
        hideLoading();
    };

    return (
        <div className="custom-page">
            <div className="col-12 p-0">
                <Card title={t('company.title')} contentClassName='p-4'>
                    <Form
                        mode={isEdit ? 'edit' : 'add'}
                        control={control}
                        fields={fields}
                        onSubmit={handleSubmit(onSubmit)}
                        errors={errors}
                        close={() => router.push('/holding-identification')}
                    />
                </Card>
            </div>
        </div>
    )
}
