'use client';
import React, { useState, useEffect, useRef } from 'react';
import { useTranslations } from 'next-intl';
import CustomDataTable from '@/src/components/CustomDataTable';
import Card from '@/src/components/Card';
import { Button } from 'primereact/button';
import { useRouter } from 'next/navigation';
import { actionBodyTemplate, header } from '@/src/utils/datatable-body';
import { userService } from '@/src/services/user';
import { useFetch } from '@/src/hooks/use-fetch';
import { tryCatch } from '@/src/utils/tryCatch';
import { useToaster } from '@/src/context/ToasterContext';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { PaginatedList } from '@/src/types/common';
import { useLoading } from '@/src/context/LoadingContext';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { getRoles } from '@/src/services/roles';

interface User {
    id: string;
    email: string;
    name: string;
    surname: string;
    title: string;
    description: string;
}

interface Role {
    id: string;
    name: string;
}

export default function UserIdentification() {
    const { setIsLoading } = useLoading();
    const t = useTranslations();
    const router = useRouter();
    const [globalFilter, setGlobalFilter] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [first, setFirst] = useState(0);
    const { openToaster } = useToaster();

    const [roleDialogVisible, setRoleDialogVisible] = useState(false);
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [selectedRole, setSelectedRole] = useState<Role | null>(null);
    const [loadingRoles, setLoadingRoles] = useState(false);

    const [userList, reload] = useFetch<PaginatedList<User>>(
        () => userService.list({ pageNumber, pageSize }),
        { reloadDeps: [pageNumber, pageSize], deps: [pageNumber, pageSize], setLoading: setIsLoading }
    );

    const [roles] = useFetch<PaginatedList<{ id: string, name: string }>>(() => getRoles(1, 100));

    const openRoleDialog = (user: User) => {
        setSelectedUser(user);
        setSelectedRole(null);
        setRoleDialogVisible(true);
    };

    const hideRoleDialog = () => {
        setRoleDialogVisible(false);
        setSelectedUser(null);
        setSelectedRole(null);
    };

    const assignRole = async () => {
        if (!selectedUser || !selectedRole) return;

        try {
            setIsLoading(true);
            await userService.userRoles(selectedUser.id, {
                userId: selectedUser.id,
                roleName: selectedRole.name
            });
            openToaster('top-right', {
                severity: 'success',
                summary: t('common.success'),
                detail: t('common.successMessage')
            });
            hideRoleDialog();
            reload();
        } catch (error) {
            console.error('Error assigning role:', error);
            openToaster('top-right', {
                severity: 'error',
                summary: t('common.error'),
                detail: t('common.errorMessage')
            });
        } finally {
            setIsLoading(false);
        }
    };

    const handleUserDelete = async (userId: string) => {
        const [error] = await tryCatch(userService.deleteUser(userId));
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

    const showDeleteConfirmDialog = (userId: string) => {
        confirmDialog({
            message: t('common.confirmDeleteMessage'),
            header: t('common.confirm'),
            icon: 'pi pi-exclamation-triangle',
            acceptLabel: t('common.accept'),
            rejectLabel: t('common.reject'),
            accept: () => handleUserDelete(userId),
            reject: () => { }
        });
    };

    const columns = [
        { field: 'name', header: t('user.name'), sortable: true, filter: true },
        { field: 'surname', header: t('user.surname'), sortable: true, filter: true },
        { field: 'email', header: t('user.email'), sortable: true, filter: true },
        { field: 'title', header: t('user.title'), sortable: true, filter: true },
        { field: 'description', header: t('user.description'), sortable: true, filter: true },
        {
            field: 'action',
            header: t('common.actions'),
            style: { minWidth: '12rem' },
            body: (rowData: any) => (
                <>
                    <Button
                        icon="pi pi-user-edit"
                        rounded
                        outlined
                        severity="info"
                        onClick={() => openRoleDialog(rowData)}
                        tooltip={t('user.roleAssignment')}
                        tooltipOptions={{ position: 'top' }}
                        className='mr-2'
                    />
                    <Button icon="pi pi-pencil" rounded severity="success" className="mr-2" onClick={() => router.push(`/user-identification/edit/${rowData.id}`)} />
                    <Button icon="pi pi-trash" rounded severity="warning" onClick={() => showDeleteConfirmDialog(rowData.id)} />
                </>
            )
        }
    ];

    const roleDialogFooter = (
        <>
            <Button label={t('common.cancel')} icon="pi pi-times" outlined onClick={hideRoleDialog} />
            <Button
                label={t('general.save')}
                icon="pi pi-check"
                onClick={assignRole}
                disabled={!selectedRole}
            />
        </>
    );

    return (
        <div className="custom-page">
            <ConfirmDialog />
            <Card
                title={t('user.pageTitle')}
                contentClassName="p-4"
                customHeader={
                    <Button
                        onClick={() => router.push('/user-identification/add')}
                        label={t('user.add')}
                        icon={<i className="pi pi-plus mr-2" />}
                    />
                }
            >
                <CustomDataTable
                    columns={columns}
                    data={userList?.items || []}
                    globalFilter={globalFilter}
                    onGlobalFilterChange={setGlobalFilter}
                    renderHeader={header(t, setGlobalFilter)}
                    totalRecords={userList?.totalCount}
                    rows={pageSize}
                    first={first}
                    onPageChange={(e) => {
                        setPageNumber(e.page + 1);
                        setPageSize(e.rows);
                        setFirst(e.first);
                    }}
                />
            </Card>

            <Dialog
                visible={roleDialogVisible}
                style={{ width: '450px' }}
                header={t('user.roleAssignment')}
                modal
                className="p-fluid"
                footer={roleDialogFooter}
                onHide={hideRoleDialog}
            >
                {selectedUser && (
                    <div className="mb-4">
                        <p className="font-bold">{selectedUser.name} {selectedUser.surname}</p>
                        <p className="text-sm text-gray-600">{selectedUser.email}</p>
                    </div>
                )}
                <div className="field">
                    <label htmlFor="role" className="font-bold">{t('user.role')}</label>
                    <Dropdown
                        id="role"
                        value={selectedRole}
                        onChange={(e) => setSelectedRole(e.value)}
                        options={roles?.items || []}
                        optionLabel="name"
                        placeholder={t('user.selectRole')}
                        className="w-full"
                        loading={loadingRoles}
                    />
                </div>
            </Dialog>
        </div>
    );
}
