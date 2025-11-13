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
import { userService } from '@/src/services/user';
import { useToaster } from '@/src/context/ToasterContext';
import { useParams, useRouter } from 'next/navigation';
import { useLoading } from '@/src/context/LoadingContext';
import { getFirms } from '@/src/services/firm';
import { PaginatedList } from '@/src/types/common';
import { useFetch } from '@/src/hooks/use-fetch';
import { getFacilities } from '@/src/services/facility';
import { getTenants } from '@/src/services/tenant';
import { ROLES } from '@/src/utils/roles';
import { Tag } from 'primereact/tag';

interface User {
    userId?: string;
    id?: number;
    email: string;
    tenantId: number | null;
    name: string;
    surname: string;
    title: string;
    description: string;
    password?: string;
    userFirmFacilities?: { firmId: number; facilityId: number }[];
    firms?: number[];
    facilities?: number[];
    roles?: string[];
}

interface FormData extends Omit<User, 'tenantId'> {
    tenantId?: number | null;
}

export default function UserActionPage() {
    const router = useRouter();
    const t = useTranslations();
    const { showLoading, hideLoading } = useLoading();
    const { openToaster } = useToaster();
    const { mode } = useParams() as { mode: string };
    const modeParts = mode?.split('/');
    const userId = modeParts?.[1];
    const isEdit = mode !== 'add';
    const userStorage = localStorage.getItem("userInfo");
    const userInfo = userStorage ? JSON.parse(userStorage) : null;

    const [firmList] = useFetch<PaginatedList<any>>(() => getFirms(1, 10000));
    const [tenantList] = useFetch<PaginatedList<any>>(() => getTenants(1, 10000));

    const schema = () =>
        yup.object().shape({
            userId: yup.string(),
            email: yup.string().email(t('validation.email')).required(t('validation.required')),
            name: yup.string().required(t('validation.required')),
            surname: yup.string().required(t('validation.required')),
            title: yup.string().required(t('validation.required')),
            description: yup.string().required(t('validation.required')),
            password: isEdit ? yup.string() : yup.string().required(t('validation.required')),
        });

    const { control, handleSubmit, formState: { errors }, reset, watch } = useForm<FormData>({
        defaultValues: {
            userId: undefined,
            email: '',
            name: '',
            surname: '',
            title: '',
            description: '',
            password: '',
            userFirmFacilities: [],
            firms: [],
            facilities: [],
            roles: []
        },
        resolver: yupResolver(schema())
    });

    console.log(watch('firms'));
    const [facilityList] = useFetch<PaginatedList<any>>(
        watch('firms')
            ? () => getFacilities(1, 10000, watch('firms')?.filter(Boolean) as number[])
            : () => Promise.resolve(),
        {
            reloadDeps: [watch('firms')],
            deps: [watch('firms')]
        }
    );

    const fields = [
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align="left" type="dashed" className="my-2 text-lg">
                    <span className="p-tag">{t('user.userInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'name',
            label: t('user.name'),
            component: 'input'
        },
        {
            name: 'surname',
            label: t('user.surname'),
            component: 'input'
        },
        {
            name: 'email',
            label: t('user.email'),
            component: 'input',
            type: 'email'
        },
        {
            name: 'title',
            label: t('user.title'),
            component: 'input'
        },
        {
            name: 'description',
            label: t('user.description'),
            component: 'textarea',
            col: 12,
            rows: 5,
            height: 'auto'
        },
        {
            name: 'password',
            label: t('user.password'),
            component: 'input',
            type: 'password',
            hide: isEdit
        },
        {
            name: 'tenantId',
            label: t('user.tenant'),
            component: 'dropdown',
            options: tenantList?.items,
            optionLabel: 'name',
            optionValue: 'id',
            type: 'number',
            hide: !userInfo?.roles.includes(ROLES.ADMINISTRATOR)
        },
        {
            name: "separator",
            component: 'separator',
            content: () => (
                <Divider align="left" type="dashed" className="my-2 text-lg">
                    <span className="p-tag">{t('user.companyInformation')}</span>
                </Divider>
            )
        },
        {
            name: 'firms',
            label: t('user.firms'),
            component: 'multiselect',
            options: firmList?.items,
            optionLabel: 'name',
            optionValue: 'id',
            multiple: true
        },
        {
            name: 'facilities',
            label: t('user.facilities'),
            component: 'multiselect',
            options: facilityList?.items,
            optionLabel: 'name',
            optionValue: 'id',
        }
    ];

    useEffect(() => {
        const getDetail = async () => {
            showLoading();
            const [error, result] = await tryCatch(userService.detail(userId));
            if (error) {
                openToaster('top-right', {
                    severity: 'error',
                    summary: t('common.error'),
                    detail: error.message
                });
                hideLoading();
            } else {

                const uniqueFirms = Array.from(new Set(result.userFirmFacilities.map((item: any) => item.firmId)));
                const uniqueFacilities = Array.from(new Set(result.userFirmFacilities.map((item: any) => item.facilityId)));

                reset({
                    userId,
                    email: result.email,
                    name: result.name,
                    surname: result.surname,
                    title: result.title,
                    description: result.description,
                    userFirmFacilities: result.userFirmFacilities,
                    firms: uniqueFirms as number[],
                    facilities: uniqueFacilities as number[],
                    roles: result.roles || []
                } as User);
                hideLoading();
            }
        };

        if (isEdit) {
            getDetail();
        }
    }, []);

    const onSubmit = async (data: FormData) => {
        showLoading();
        const tenantId = userInfo?.tenant?.id;

        const selectedFirms: number[] = data.firms || [];
        const selectedFacilities: number[] = data.facilities || [];

        let userFirmFacilities: { firmId: number | null; facilityId: number | null }[] = [];

        if (selectedFirms.length > 0 && selectedFacilities.length > 0) {
            selectedFirms.forEach((firm) => {
                selectedFacilities.forEach((facility) => {
                    userFirmFacilities.push({ firmId: firm, facilityId: facility });
                });
            });
        } else if (selectedFirms.length > 0) {
            selectedFirms.forEach((firm) => {
                userFirmFacilities.push({ firmId: firm, facilityId: null });
            });
        } else if (selectedFacilities.length > 0) {
            selectedFacilities.forEach((facility) => {
                userFirmFacilities.push({ firmId: null, facilityId: facility });
            });
        }

        const payload: any = { ...data, tenantId: tenantId || data.tenantId, userFirmFacilities };

        delete payload.firms;
        delete payload.facilities;

        const [error] = isEdit
            ? await tryCatch(userService.updateUser({...payload, userId }))
            : await tryCatch(userService.createUser(payload));

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

        router.push('/user-identification');
        hideLoading();
    };

    return (
        <div className="custom-page">
            <div className="col-12 p-0">
                <Card title={t('user.pageTitle')} contentClassName="p-4">
                    <Form
                        mode={isEdit ? 'edit' : 'add'}
                        control={control}
                        fields={fields}
                        onSubmit={handleSubmit(onSubmit)}
                        errors={errors}
                        close={() => router.push('/user-identification')}
                    >
                        {(watch('roles') as string[])?.length > 0 && (
                            <div className='flex flex-column gap-2 p-2'>
                                <h4>{t('user.role')}</h4>
                                {watch("roles")?.map((role: string) => (
                                    <Tag key={role} value={role} />
                                ))}
                            </div>
                        )}
                    </Form>
                </Card>
            </div>
        </div >
    );
}
