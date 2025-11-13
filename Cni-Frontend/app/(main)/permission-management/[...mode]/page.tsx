"use client"
import React from 'react'
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import Card from '@/src/components/Card';
import Input from '@/src/components/ui/Input';
import { InputSwitch } from 'primereact/inputswitch';
import { useState } from 'react';
import { Button } from 'primereact/button';

export default function PermissionManagementActionPage() {
    const t = useTranslations();
    const [checked, setChecked] = useState<number[]>([]);
    const router = useRouter();

    return (
        <div className="custom-page">
            <div className="col-12 p-0">
                <Card title={t('role.pageTitle')} contentClassName="p-4">
                    <div className="grid">
                        <div className='col-12 md:col-6'>
                            <Input className="w-full" label={t('role.name')} />
                        </div>
                        <div className='col-12 md:col-6'>
                            <Input className="w-full" label={t('role.description')} />
                        </div>
                        <div className='col-12 mt-4 md:mt-0'>
                            <div className="grid">
                                {["Monitoring", "Reporting", "Analysis", "Facility", "User Management", "Audit", "Blocked Address", "Reporting", "Holding Identification", "Scada Access", "Facility Map"].map((i: string, index: number) => (
                                    <div key={index} className="flex align-items-center gap-2 col-12 md:col-3">
                                        <InputSwitch checked={checked.includes(index)} onChange={(e) => setChecked((prev) => [...prev, index])} />
                                        <label htmlFor={`input-switch-${index}`}>{i}</label>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                    <div className="flex w-full justify-content-end gap-2">
                        <Button label={t('common.cancel')} onClick={() => router.push('/permission-management')} />
                        <Button label={t('common.save')} severity="success" onClick={() => { }} />
                    </div>
                </Card>
            </div>
        </div >
    )
}
