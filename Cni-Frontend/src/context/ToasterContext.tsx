import React, { createContext, useCallback, useMemo, useState, useRef, useContext, ReactNode } from 'react';

import { Toast } from 'primereact/toast';

type ToastPosition = 'center' | 'top-right' | 'top-center' | 'top-left' | 'bottom-center' | 'bottom-left' | 'bottom-right';

interface ToasterContextType {
  openToaster: (position: string, body?: any) => void;
}

export const ToasterContext = createContext<ToasterContextType | null>(null);

interface ToasterContextProviderProps {
  children: ReactNode;
}

export function ToasterContextProvider({ children }: ToasterContextProviderProps) {
  const toast = useRef<Toast>(null);
  const [toasterPosition, setToasterPosition] = useState<ToastPosition>('top-right');

  const openToaster = useCallback((position: string, body: any = {}) => {
    setToasterPosition(position as ToastPosition);
    toast.current?.show(body);
  }, []);

  const context = useMemo(() => ({ openToaster }), [openToaster]);

  return (
    <>
      <ToasterContext.Provider value={context}>{children}</ToasterContext.Provider>
      <Toast ref={toast} position={toasterPosition} />
    </>
  );
}

export const useToaster = () => {
  const context = useContext(ToasterContext);

  if (!context) {
    throw new Error('useToaster hook must be used within a ToasterProvider');
  }

  return context;
}

