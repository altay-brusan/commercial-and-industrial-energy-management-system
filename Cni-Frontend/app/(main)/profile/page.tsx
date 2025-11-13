/* eslint-disable react-hooks/exhaustive-deps */
'use client';

import React, { useState, useRef, useEffect, useCallback } from 'react';
import { Card } from 'primereact/card';
import { Button } from 'primereact/button';
import { Avatar } from 'primereact/avatar';
import { Divider } from 'primereact/divider';
import { Dialog } from 'primereact/dialog';
import { Toast } from 'primereact/toast';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { classNames } from 'primereact/utils';
import { useLoading } from '@/src/context/LoadingContext';
import { useToaster } from '@/src/context/ToasterContext';
import Form from '@/src/components/ui/Form';
import { tryCatch } from '@/src/utils/tryCatch';
import { authService } from '@/src/services/auth';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useUser } from '@/src/hooks/use-user';
import { TreeTable } from 'primereact/treetable';
import { Column } from 'primereact/column';
import Input from '@/src/components/ui/Input';

interface UserProfile {
    name: string;
    surname: string;
    email: string;
    roles: string[];
    newPassword: string;
    resetCode: string;
}

const ProfilePage = () => {
    const [activeTab, setActiveTab] = useState('kisisel');
    const [treeTableData, setTreeTableData] = useState<any[]>([]);
    const [show2FAResetDialog, setShow2FAResetDialog] = useState(false);
    const [resetting2FA, setResetting2FA] = useState(false);
    const { showLoading, hideLoading } = useLoading();
    const { openToaster } = useToaster();
    const { user, handleStorageChange } = useUser();
    const router = useRouter();
    const t = useTranslations();
    const toast = useRef<Toast>(null);

    interface TreeNodeData {
        key: string;
        data: {
            name: string;
            type: string;
            id: number;
        };
        children?: TreeNodeData[];
    }

    const prepareTreeTableData = useCallback((tenants: any[]): TreeNodeData[] => {
        const data: TreeNodeData[] = [];

        tenants.forEach((tenant: any, index: number) => {
            const tenantNode: TreeNodeData = {
                key: `tenant-${index}`,
                data: {
                    name: tenant.name,
                    type: 'Tenant',
                    id: tenant.id
                },
                children: []
            };

            if (tenant.items && tenant.items.length > 0) {
                tenant.items.forEach((company: any, companyIndex: number) => {
                    const companyNode: TreeNodeData = {
                        key: `tenant-${index}-company-${companyIndex}`,
                        data: {
                            name: company.name,
                            type: 'Firma',
                            id: company.id
                        },
                        children: []
                    };

                    if (company.items && company.items.length > 0) {
                        company.items.forEach((facility: any, facilityIndex: number) => {
                            const facilityNode: TreeNodeData = {
                                key: `tenant-${index}-company-${companyIndex}-facility-${facilityIndex}`,
                                data: {
                                    name: facility.name,
                                    type: 'Tesis',
                                    id: facility.id
                                }
                            };
                            companyNode.children?.push(facilityNode);
                        });
                    }

                    tenantNode.children?.push(companyNode);
                });
            }

            data.push(tenantNode);
        });

        return data;
    }, []);

    const schema = yup.object({
        name: yup.string().required(t("validation.required")),
        surname: yup.string().required(t("validation.required")),
        email: yup.string().email(t("validation.email"))
            .required(t("validation.required")),
    });

    const schemaConfirm = yup.object({
        resetCode: yup.string().required(t("validation.required")),
        newPassword: yup
            .string()
            .required(t("validation.required"))
            .test(
                "no-4-same",
                t("validation.passwordNoSame"),
                value => !/(\d)\1\1\1/.test(value ?? "")
            )
            .test("sequential-check", t("validation.passwordSequential"), value => {
                if (!value) return false;
                const digits = value.split("").map(Number);
                let isAscending = true;
                let isDescending = true;
                for (let i = 0; i < digits.length - 1; i++) {
                    if (digits[i + 1] !== digits[i] + 1) {
                        isAscending = false;
                    }
                    if (digits[i + 1] !== digits[i] - 1) {
                        isDescending = false;
                    }
                }
                return !(isAscending || isDescending);
            })
    });

    const defaultValues: UserProfile = {
        name: '',
        surname: '',
        email: '',
        roles: [],
        newPassword: '',
        resetCode: ''
    };

    const {
        control,
        handleSubmit,
        reset,
        formState: { errors }
    } = useForm<any>({
        defaultValues,
        resolver: yupResolver(schema)
    });

    const {
        control: controlConfirm,
        handleSubmit: handleSubmitConfirm,
        reset: resetConfirm,
        watch: watchConfirm,
        setValue: setValueConfirm,
        formState: { errors: errorsConfirm }
    } = useForm<any>({
        defaultValues,
        resolver: yupResolver(schemaConfirm)
    });

    useEffect(() => {
        reset({
            name: user?.name || '',
            surname: user?.surname || '',
            email: user?.email || '',
            roles: user?.roles || [],
            newPassword: '',
            resetCode: ''
        })

        resetConfirm({
            email: user?.email || '',
            resetCode: '',
            newPassword: ''
        })

        if (user?.tenants) {
            const data = prepareTreeTableData(user.tenants);
            setTreeTableData(data);
        }
    }, [user, reset, prepareTreeTableData])


    const logout = async () => {
        localStorage.clear();
        router.push('/auth/login');
    };

    const handleReset2FA = async () => {
        setResetting2FA(true);
        try {
            await new Promise(resolve => setTimeout(resolve, 1500));

            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: t("profile.2faResetSuccess"),
                life: 3000
            });
            setShow2FAResetDialog(false);
        } catch (error: any) {
            toast.current?.show({
                severity: 'error',
                summary: t("common.error"),
                detail: error.message || t("profile.2faResetError"),
                life: 3000
            });
        } finally {
            setResetting2FA(false);
        }
    };

    const render2FAResetDialogFooter = () => {
        return (
            <div className="flex justify-content-end gap-2">
                <Button
                    label="İptal"
                    icon="pi pi-times"
                    className="p-button-text"
                    onClick={() => setShow2FAResetDialog(false)}
                />
                <Button
                    label="Sıfırla"
                    icon="pi pi-check"
                    className="p-button-danger"
                    onClick={handleReset2FA}
                    loading={resetting2FA}
                />
            </div>
        );
    };

    const onSubmit = async (data: UserProfile) => {
        const [error] = await tryCatch(authService.updateUserProfile({
            name: data.name,
            surname: data.surname,
            email: data.email,
        }))

        if (error) {
            openToaster("top-right", {
                severity: "error",
                summary: t("common.error"),
                detail: error?.response?.data?.detail
            });
            return;
        }

        const [userInfoErr, userInfoRes] = await tryCatch(authService.userInfo());
        if (userInfoErr) {
            openToaster("top-right", {
                severity: "error",
                summary: t("common.error"),
                detail: userInfoErr?.response?.data?.detail
            });
            return;
        } else {
            localStorage.setItem('userInfo', JSON.stringify(userInfoRes));
            handleStorageChange();
        }

        openToaster("top-right", {
            severity: "success",
            summary: "Başarılı",
            detail: t("profile.updateProfileSuccess")
        });
    };

    const changePasswordSubmit = async (data: any) => {
        showLoading();
        const [error] = await tryCatch(authService.resetPassword({
            email: data?.email as string,
            resetCode: data.resetCode,
            newPassword: data.newPassword
        }))

        if (error) {
            openToaster("top-right", {
                severity: "error",
                summary: t("common.error"),
                detail: error?.response?.data?.detail
            });

            hideLoading();
            return;
        }

        logout();

        openToaster("top-right", {
            severity: "success",
            summary: "Başarılı",
            detail: t("profile.changePasswordSuccess")
        });

        hideLoading();
    };

    const fields = [
        {
            name: "name",
            type: "text",
            label: t("user.name"),
            component: "input"
        },
        {
            name: "surname",
            type: "text",
            label: t("user.surname"),
            component: "input"
        },
        {
            name: "email",
            type: "text",
            label:t("user.email"),
            component: "input"
        },
        {
            name: "roles",
            type: "text",
            label: t("user.role"),
            component: "input",
            disabled: true
        },
    ];

    const changePasswordFields = [
        {
            name: "resetCode",
            label: t("profile.resetCode"),
            component: "input",
            fieldCol: 6,
            col: 12
        },
        {
            name: "newPassword",
            type: "password",
            label: t("profile.newPassword"),
            component: "password",
            toggleMask: true,
            feedback: false,
            fieldCol: 6,
            col: 12
        },
    ]

    const handleSendVerificationCode = async () => {
        showLoading();
        const [error] = await tryCatch(authService.resendConfirmationEmail(watchConfirm('email')));

        if (error) {
            openToaster("top-right", {
                severity: 'error',
                summary: t("common.error"),
                detail: error?.response?.data?.detail,
                life: 3000
            });
            hideLoading();
            return;
        }

        openToaster("top-right", {
            severity: 'success',
            summary: 'Başarılı',
            detail: t("profile.verificationCodeSent"),
            life: 3000
        });
        hideLoading();
    }

    return (<>
        <div className="grid">
            <div className="col-12 md:col-5 xl:col-4">
                <Card className="shadow-2 h-full">
                    <div className="flex flex-column align-items-center">
                        <div className="relative mb-3">
                            <Avatar
                                icon="pi pi-user"
                                size="xlarge"
                                shape="circle"
                                className="shadow-4"
                                style={{ width: '80px', height: '80px' }}
                            />
                        </div>
                        <h3 className="my-2 text-center">{user?.name} {user?.surname}</h3>
                        <p className="text-color-secondary">{user?.email}</p>
                    </div>

                    <Divider />

                    <div className="flex flex-column">
                        <Button
                            text
                            label={t("profile.personal")}
                            icon="pi pi-user"
                            className={classNames('p-button-text justify-content-start mb-2', { 'bg-primary': activeTab === 'kisisel' })}
                            onClick={() => setActiveTab('kisisel')}
                        />
                        <Button
                            text
                            label={t("profile.password")}
                            icon="pi pi-lock"
                            className={classNames('p-button-text justify-content-start mb-2', { 'bg-primary': activeTab === 'sifre' })}
                            onClick={() => setActiveTab('sifre')}
                        />
                        <Button
                            text
                            label={t("profile.organizasyon")}
                            icon="pi pi-sitemap"
                            className={classNames('p-button-text justify-content-start mb-2', { 'bg-primary': activeTab === 'organizasyon' })}
                            onClick={() => setActiveTab('organizasyon')}
                        />
                    </div>
                </Card>
            </div>

            <div className="col-12 md:col-7 xl:col-8">
                <Card className="shadow-2 h-full">
                    {activeTab === 'kisisel' && (
                        <div>
                            <div className="flex align-items-center justify-content-between">
                                <div>
                                    <h2>{t("profile.personal")}</h2>
                                    <p className="text-color-secondary mb-4">{t("profile.personalDescription")}</p>
                                </div>
                                <div className="flex align-items-center">
                                    <Button
                                        label={t("profile.2faReset")}
                                        icon="pi pi-shield"
                                        className="p-button-outlined p-button-rounded p-button-info"
                                        onClick={() => setShow2FAResetDialog(true)}
                                    />
                                </div>
                            </div>

                            <Divider />

                            <Form buttonIgnore errors={errors} control={control} fields={fields} close={() => router.push('/')}></Form>
                            <div className="col-12 flex justify-content-end mt-4">
                                <Button label="Kaydet" type="submit" icon="pi pi-save" onClick={handleSubmit(onSubmit)} />
                            </div>

                            <Dialog
                                header={t("profile.2faReset")}
                                visible={show2FAResetDialog}
                                onHide={() => setShow2FAResetDialog(false)}
                                style={{ width: '450px' }}
                                modal
                                footer={render2FAResetDialogFooter}
                                className="p-dialog-custom"
                            >
                                <div className="flex flex-column align-items-center p-4">
                                    <div className="bg-red-500 border-round p-4 mb-3">
                                        <i className="pi pi-exclamation-triangle text-4xl text-white"></i>
                                    </div>
                                    <h3>{t("profile.warning")}</h3>
                                    <p className="text-center line-height-3 mt-2">
                                        {t("profile.2faResetDescription")}
                                    </p>
                                    <p className="text-center font-italic mt-2">
                                        {t("profile.warningDescription")}
                                    </p>
                                </div>
                            </Dialog>
                        </div>
                    )}

                    {activeTab === 'sifre' && (
                        <div>
                            <h2>Şifre Değiştir</h2>
                            <p className="text-color-secondary mb-4">{t("profile.passwordDescription")}</p>

                            <div className='w-full flex flex-column gap-4 mb-4'>
                                <Input className='col-6' label={t("common.email")} name="email" type="email" value={watchConfirm('email')} onChange={(e: any) => setValueConfirm('email', e.target.value)} />
                                <div className="flex justify-content-start">
                                    <Button label={t("profile.sendVerificationCode")} onClick={handleSendVerificationCode}></Button>
                                </div>
                            </div>
                            <Form buttonIgnore errors={errorsConfirm} control={controlConfirm} col={12} fields={changePasswordFields} close={() => router.push('/')} >
                                <div className='w-full flex justify-content-end p-2'>
                                    <Button label={t("common.update")} onClick={handleSubmitConfirm(changePasswordSubmit)} />
                                </div>
                            </Form>
                        </div>
                    )}

                    {activeTab === 'organizasyon' && (
                        <div>
                            <h2>{t("profile.organizasyon")}</h2>
                            <p className="text-color-secondary mb-4">{t("profile.organizasyonDescription")}</p>

                            {user?.tenants && user.tenants.length > 0 ? (
                                <div className="card">
                                    <TreeTable value={treeTableData} tableStyle={{ minWidth: '50rem' }}>
                                        <Column className='flex' field="name" header={t("user.name")} expander body={(rowData) => {
                                            const iconMap: Record<string, string> = {
                                                'Tenant': 'pi pi-building',
                                                'Firma': 'pi pi-briefcase',
                                                'Tesis': 'pi pi-map-marker'
                                            };
                                            return (
                                                <div className="flex align-items-center">
                                                    <i className={`${iconMap[rowData.data.type] || 'pi pi-file'} mr-2`}></i>
                                                    <span>{rowData.data.name}</span>
                                                </div>
                                            );
                                        }}></Column>
                                        <Column field="type" header={t("common.type")}></Column>
                                        <Column field="id" header={t("common.id")}></Column>
                                    </TreeTable>
                                </div>
                            ) : (
                                <div className="flex flex-column align-items-center justify-content-center p-5">
                                    <i className="pi pi-exclamation-circle text-5xl text-color-secondary mb-3"></i>
                                    <p className="text-xl text-center">{t("profile.organizasyonNotFound")}</p>
                                </div>
                            )}
                        </div>
                    )}
                </Card>
            </div>
        </div>
    </>
    );
};

export default ProfilePage;
