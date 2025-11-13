/* eslint-disable @next/next/no-img-element */

import Link from 'next/link';
import { classNames } from 'primereact/utils';
import React, { forwardRef, useContext, useEffect, useImperativeHandle, useRef, useState } from 'react';
import { AppTopbarRef } from '@/src/types';
import { LayoutContext } from './context/layoutcontext';
import LanguageSwitcher from '@/src/components/LanguageSwitcher';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { TreeSelect } from 'primereact/treeselect';
import { useTenant } from '@/src/context/TenantContext';
import { ConfirmPopup, confirmPopup } from 'primereact/confirmpopup';
import CustomCalendar from '@/src/components/ui/CustomCalendar';
import { subDays } from 'date-fns';
import { Button } from 'primereact/button';
import { formatDateISO } from '@/src/utils/dateFormatTR';

const AppTopbar = forwardRef<AppTopbarRef>((props, ref) => {
    const t = useTranslations();
    const router = useRouter();
    const { layoutConfig, layoutState, onMenuToggle, showProfileSidebar } = useContext(LayoutContext);
    const menubuttonRef = useRef(null);
    const topbarmenuRef = useRef(null);
    const topbarmenubuttonRef = useRef(null);
    const [selectedNodeKey, setSelectedNodeKey] = useState<any>(null);
    const userInfo = JSON.parse(localStorage.getItem('userInfo') || '{}');
    const { tenant, setTenant, startDate, setStartDate } = useTenant();
    const [date, setDate] = useState<any>(startDate || null);

    useEffect(() => {
        if (tenant) {
            setSelectedNodeKey(tenant);
        }
    }, [tenant, selectedNodeKey, setTenant]);

    const handleTenantChange = (value: any) => {
        setTenant(value);
        setSelectedNodeKey(value);
    };

    useImperativeHandle(ref, () => ({
        menubutton: menubuttonRef.current,
        topbarmenu: topbarmenuRef.current,
        topbarmenubutton: topbarmenubuttonRef.current
    }));

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('userInfo');
        router.push('/auth/login');
    };

    const confirmLogout = (event: React.MouseEvent<HTMLButtonElement>) => {
        confirmPopup({
            target: event.currentTarget,
            message: t("common.confirmLogout"),
            icon: 'pi pi-exclamation-triangle',
            accept: handleLogout,
            acceptLabel: t("common.yes"),
            rejectLabel: t("common.cancel")
        });
    };

    interface TreeNode {
        key: string;
        label: string;
        data: string;
        icon: string;
        children: TreeNode[];
        selectable: boolean;
    }

    const convertTenantToTreeNodes = (tenants: any[]): TreeNode[] => {
        return tenants?.map(tenant => ({
            key: tenant.id.toString(),
            label: tenant.name,
            data: tenant.name,
            icon: 'pi pi-fw pi-building',
            children: tenant.items ? convertTenantToTreeNodes(tenant.items) : [],
            selectable: !tenant.items || tenant.items.length === 0
        }));
    };

    return (
        <div className="layout-topbar">
            <ConfirmPopup />
            <Link href="/" className="layout-topbar-logo">
                <img src={`${layoutConfig.colorScheme === 'light' ? '/logo-light.svg' : '/logo-dark.svg'}`} alt="OKLIDIS" />
            </Link>

            <button ref={menubuttonRef} type="button" className="p-link layout-menu-button layout-topbar-button" onClick={onMenuToggle}>
                <i className="pi pi-bars" />
            </button>

            <button ref={topbarmenubuttonRef} type="button" className="p-link layout-topbar-menu-button layout-topbar-button" onClick={showProfileSidebar}>
                <i className="pi pi-ellipsis-v" />
            </button>

            <div ref={topbarmenuRef} className={classNames('layout-topbar-menu', { 'layout-topbar-menu-mobile-active': layoutState.profileSidebarVisible })}>
                <div className='flex align-items-center mb-2 md:mb-0'>
                    {startDate !== null && <span onClick={() => setStartDate(null)} className='md:mr-3 cursor-pointer'><i className='pi pi-circle-fill text-red-500'></i> {t('common.live')}</span>}
                    <CustomCalendar
                        maxDate={subDays(new Date(), 1)}
                        label={t('common.selectDate')}
                        className='mr-3 h-3rem'
                        showTime
                        value={date}
                        onChange={(e: any) => setDate(e.value)}
                        footerTemplate={(options: { onHide: () => void }) => (
                            <div className="flex justify-content-end gap-2">
                                <Button
                                    label={t('common.ok')}
                                    className="p-button-sm"
                                    onClick={() => {
                                        setStartDate(formatDateISO(date)?.slice(0, 19));
                                        options.onHide();
                                    }}
                                />
                            </div>
                        )}
                    />
                </div>
                <TreeSelect value={selectedNodeKey} onChange={(e) => handleTenantChange(e.value)} options={convertTenantToTreeNodes(userInfo.tenants)}
                    className="md:w-20rem w-full md:mr-3" placeholder={t('common.selectTenant')}></TreeSelect>
                <div className='flex align-items-center gap-2'>
                    <Link href="/profile" type="button" className="p-link layout-topbar-button">
                        <i className="pi pi-user"></i>
                        <span>Profile</span>
                    </Link>
                    <LanguageSwitcher />
                </div>
                <button className="p-link layout-topbar-button" onClick={confirmLogout}>
                    <i className="pi pi-sign-out"></i>
                    <span>{t('common.logout')}</span>
                </button>
            </div>
        </div>
    );
});

AppTopbar.displayName = 'AppTopbar';

export default AppTopbar;
