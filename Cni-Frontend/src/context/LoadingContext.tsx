'use client';

import { createContext, useContext, useState, type ReactNode } from 'react';
import { ProgressSpinner } from 'primereact/progressspinner';

interface LoadingContextType {
    isLoading: boolean;
    showLoading: () => void;
    hideLoading: () => void;
    setIsLoading: (isLoading: boolean) => void;
}

const LoadingContext = createContext<LoadingContextType | undefined>(undefined);

interface LoadingProviderProps {
    children: ReactNode;
}

export const LoadingProvider = ({ children }: LoadingProviderProps) => {
    const [isLoading, setIsLoading] = useState(false);

    const showLoading = () => setIsLoading(true);
    const hideLoading = () => setIsLoading(false);

    return (
        <LoadingContext.Provider value={{ isLoading, showLoading, hideLoading , setIsLoading}}>
            {children}
            {isLoading && (
                <div className="overlay">
                    <ProgressSpinner style={{ zIndex: 1000 }} />
                </div>
            )}
        </LoadingContext.Provider>
    );
};

export const useLoading = () => {
    const context = useContext(LoadingContext);

    if (!context) {
        throw new Error('useLoading hook must be used within a LoadingProvider');
    }

    return context;
};
