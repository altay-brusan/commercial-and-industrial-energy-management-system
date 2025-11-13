'use client';

import { LayoutProvider } from '../layout/context/layoutcontext';
import { PrimeReactProvider } from 'primereact/api';
import { LoadingProvider } from '@/src/context/LoadingContext';
import { ToasterContextProvider } from '@/src/context/ToasterContext';
import { TenantProvider } from '@/src/context/TenantContext';
interface ProvidersProps {
    children: React.ReactNode;
}

const Providers = ({ children }: ProvidersProps) => {
    return (
        <LoadingProvider>
            <ToasterContextProvider>
                <TenantProvider>
                    <PrimeReactProvider>
                        <LayoutProvider>{children}</LayoutProvider>
                    </PrimeReactProvider>
                </TenantProvider>
            </ToasterContextProvider>
        </LoadingProvider>
    );
};

export default Providers;
