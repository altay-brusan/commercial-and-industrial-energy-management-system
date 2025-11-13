/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import Form from '@/src/components/ui/Form'
import React, { useEffect } from 'react'
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useTranslations } from 'next-intl';
import Card from '@/src/components/Card';
import { useRouter, useParams } from 'next/navigation';
import { useToaster } from '@/src/context/ToasterContext';
import { useLoading } from '@/src/context/LoadingContext';
import { tryCatch } from '@/src/utils/tryCatch';
import { createPlan, getPlanById, updatePlan } from '@/src/services/plans';
import { Divider } from 'primereact/divider';

interface PlanFormData {
    name: string;
    modules: string;
    apiAccess: boolean;
    maxFacilities: number;
    maxUsers: number;
    price: number;
}

export default function PlanIdentificationAction() {
    const t = useTranslations();
    const router = useRouter();
    const { openToaster } = useToaster();
    const { showLoading, hideLoading } = useLoading();
    const { mode } = useParams() as { mode: string };
    const modeParts = mode?.split('/');
    const id = modeParts?.[1];
    const isEdit = modeParts?.[0] === 'edit';

    const schema = () =>
        yup.object().shape({
            name: yup.string().required(t('validation.required')),
            modules: yup.string().required(t('validation.required')),
            apiAccess: yup.boolean().required(t('validation.required')),
            maxFacilities: yup.number().required(t('validation.required')).positive().integer(),
            maxUsers: yup.number().required(t('validation.required')).positive().integer(),
            price: yup.number().required(t('validation.required')).positive(),
        });

    const fields = [
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align='left' type='dashed' className='my-2 text-lg'>
                    <span className="p-tag">{t('plan.generalInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'name',
            label: t('plan.planName'),
            component: 'input',
        },
        {
            name: 'modules',
            label: t('plan.modules'),
            component: 'dropdown',
            options: [
                { value: 'module1', label: 'Modül 1' },
                { value: 'module2', label: 'Modül 2' },
            ],
        },
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align='left' type='dashed' className='my-2 text-lg'>
                    <span className="p-tag">{t('plan.properties')}</span>
                </Divider>
            )
        },
        {
            name: 'maxFacilities',
            label: t('plan.maxFacilities'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'maxUsers',
            label: t('plan.maxUsers'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'price',
            label: t('plan.price'),
            component: 'input',
            type: 'number',
        },
        {
            name: 'apiAccess',
            label: t('plan.apiAccess'),
            component: 'switch',
        },
    ];

    const {
        control,
        handleSubmit,
        formState: { errors },
        reset
    } = useForm<PlanFormData>({
        defaultValues: {
            name: '',
            modules: '',
            apiAccess: false,
            maxFacilities: 1,
            maxUsers: 1,
            price: 0,
        },
        resolver: yupResolver(schema())
    });

    useEffect(() => {
        const getDetail = async () => {
            showLoading();
            const [error, result] = await tryCatch(getPlanById(id));
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
    }, [isEdit, id, openToaster, t, reset]);

    const handleFormSubmit = async (data: PlanFormData) => {
        showLoading();

        const [error] = isEdit
            ? await tryCatch(updatePlan(id, data))
            : await tryCatch(createPlan(data));

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

        router.push('/plan-identification');
        hideLoading();
    };

    return (
        <div className="custom-page">
            <div className="col-12 p-0">
                <Card title={t('plan.pageTitle')} contentClassName='p-4'>
                    <Form
                        mode={isEdit ? 'edit' : 'add'}
                        control={control}
                        fields={fields}
                        onSubmit={handleSubmit(handleFormSubmit)}
                        errors={errors}
                        close={() => router.push('/plan-identification')}
                    />
                </Card>
            </div>
        </div>
    )
}
