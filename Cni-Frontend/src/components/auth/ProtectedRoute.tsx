'use client';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { isAuthenticated } from '@/src/utils/auth';

interface ProtectedRouteProps {
    children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
    const router = useRouter();
    const [isReady, setIsReady] = useState(false);

    useEffect(() => {
        if (!isAuthenticated()) {
            router.push('/auth/login');
        } else {
            setIsReady(true);
        }
    }, [router]);

    if (!isReady) {
        return null;
    }

    return <>{children}</>;
};

export default ProtectedRoute;
