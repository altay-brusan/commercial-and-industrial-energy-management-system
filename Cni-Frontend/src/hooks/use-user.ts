import { useState, useEffect, useCallback } from 'react';

interface UserInfo {
    email: string;
    tenants: Array<{
        id: number;
        name: string;
        items: Array<{
            id: number;
            name: string;
            items: Array<{
                id: number;
                name: string;
                items: null | any[];
            }> | null | [];
        }>;
    }>;
    name: string;
    surname: string;
    title: string;
    roles: string[];
    fullName?: string;
    id?: string;
    firstName?: string;
    lastName?: string;
    phoneCode?: string;
    phoneNumber?: string;
    integratorUsers?: {
        roleId: string;
    };
}

export const useUser = () => {
    const [user, setUser] = useState<UserInfo | null>(null);

    const getUserInfo = () => {
        try {
            const userInfoString = localStorage.getItem('userInfo');
            if (userInfoString) {
                const userInfo = JSON.parse(userInfoString);
                const userId = localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user') || '{}')?.id : undefined;
                if (userId) {
                    userInfo.id = userId;
                }
                setUser(userInfo);
            }
        } catch (error) {
            console.error('Kullanıcı bilgileri alınırken hata oluştu:', error);
        }
    };

    const handleStorageChange = useCallback(() => {
        getUserInfo();
    }, []);

    useEffect(() => {

        getUserInfo();

        window.addEventListener('storage', handleStorageChange);
        return () => {
            window.removeEventListener('storage', handleStorageChange);
        };
    }, [handleStorageChange]);

    return { user, handleStorageChange };
};
