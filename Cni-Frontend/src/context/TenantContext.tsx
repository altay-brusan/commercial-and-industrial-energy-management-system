'use client';
import React, { createContext, useState, useContext, useEffect } from 'react';
import { tryCatch } from '../utils/tryCatch';
import { scadaService } from '../services/scada';
import { formatDateISO } from '../utils/dateFormatTR';

const TenantContext = createContext<any>(null);

export const TenantProvider = ({ children }: { children: React.ReactNode }) => {
    const [tenant, setTenant] = useState<any>(null);
    const [startDate, setStartDate] = useState<Date | null>(null);
    const [userInfo, setUserInfo] = useState<any>({});

    const changeTime = async (time: Date | null | string) => {
        if (!time) return;
        const formattedTime = typeof time === 'string' ? time : formatDateISO(time)?.slice(0, 19);
        const [error, result] = await tryCatch(scadaService.changeTime(formattedTime || new Date()));
        if (error) throw error;
    }

    useEffect(() => {
        changeTime(startDate || new Date());
    }, [startDate]);

    useEffect(() => {
        const storedUserInfo = typeof window !== 'undefined' ?
            JSON.parse(localStorage?.getItem('userInfo') || '{}') : {};
        setUserInfo(storedUserInfo);

        if (storedUserInfo.tenants?.length > 0) {
            setTenant(6);
        }
        setTenant(6);
    }, []);

    return (
        <TenantContext.Provider value={{ tenant, setTenant, startDate, setStartDate }}>
            {children}
        </TenantContext.Provider>
    );
};

export const useTenant = () => useContext(TenantContext);