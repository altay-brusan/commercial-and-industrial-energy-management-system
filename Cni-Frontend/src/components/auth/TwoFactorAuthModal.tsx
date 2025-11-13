import React, { useState } from 'react';
import { Dialog } from 'primereact/dialog';
import { Button } from 'primereact/button';
import { Card } from 'primereact/card';
import Image from 'next/image';
import { useTranslations } from 'next-intl';
import { SiGoogleauthenticator } from 'react-icons/si';

type TwoFactorAuthOption = 'google' | 'microsoft' | 'sms' | null;

type TwoFactorAuthModalProps = {
    visible: boolean;
    onHide: () => void;
    onSelect: (method: TwoFactorAuthOption) => void;
};

const TwoFactorAuthModal = ({ visible, onHide, onSelect }: TwoFactorAuthModalProps) => {
    const t = useTranslations();
    const [selectedMethod, setSelectedMethod] = useState<TwoFactorAuthOption>(null);

    const handleSelect = (method: TwoFactorAuthOption) => {
        setSelectedMethod(method);
        onSelect(method);
    };

    const handleKeyDown = (e: React.KeyboardEvent, method: TwoFactorAuthOption) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            handleSelect(method);
        }
    };

    const renderHeader = () => {
        return (
            <div className="flex align-items-center justify-content-between w-full">
                <h2 className="text-xl font-medium m-0">{t('auth.twoFactor.title')}</h2>
                <Button
                    icon="pi pi-times"
                    className="p-button-rounded p-button-text"
                    onClick={onHide}
                    aria-label={t('auth.twoFactor.close')}
                />
            </div>
        );
    };

    return (
        <Dialog
            header={renderHeader}
            visible={visible}
            onHide={onHide}
            style={{ width: '50rem' }}
            breakpoints={{ '960px': '75vw', '641px': '90vw' }}
            className="p-fluid"
            modal
            closeOnEscape
            dismissableMask
        >
            <div className="p-4">
                <h3 className="text-lg font-medium mb-2">{t('auth.twoFactor.selectMethod')}</h3>
                <p className="text-color-secondary mb-4">
                    {t('auth.twoFactor.selectDescription')}
                </p>
                <p className="text-color-secondary mb-5 text-sm">
                    {t('auth.twoFactor.note')}
                </p>

                <div className="grid">
                    <div className="col-12 md:col-4 p-2">
                        <Card
                            className={`cursor-pointer h-full flex flex-column align-items-center justify-content-center p-3 border-1 hover:border-primary transition-duration-150 ${selectedMethod === 'google' ? 'border-primary' : 'border-300'
                                }`}
                            tabIndex={0}
                            onClick={() => handleSelect('google')}
                            onKeyDown={(e) => handleKeyDown(e, 'google')}
                            aria-label={t('auth.twoFactor.methods.google')}
                        >
                            <div className="flex flex-column align-items-center">
                                <div className="mb-3 flex align-items-center justify-content-center bg-gray-100 border-circle" style={{ width: '100px', height: '100px' }}>
                                    <SiGoogleauthenticator className='text-5xl text-primary-500' />
                                </div>
                                <h3 className="text-center font-medium mb-2">{t('auth.twoFactor.methods.google')}</h3>
                            </div>
                        </Card>
                    </div>

                    <div className="col-12 md:col-4 p-2">
                        <Card
                            className={`cursor-pointer h-full flex flex-column align-items-center justify-content-center p-3 border-1 hover:border-primary transition-duration-150 ${selectedMethod === 'microsoft' ? 'border-primary' : 'border-300'
                                }`}
                            tabIndex={0}
                            onClick={() => handleSelect('microsoft')}
                            onKeyDown={(e) => handleKeyDown(e, 'microsoft')}
                            aria-label={t('auth.twoFactor.methods.microsoft')}
                        >
                            <div className="flex flex-column align-items-center">
                                <div className="mb-3 flex align-items-center justify-content-center bg-blue-50 border-circle" style={{ width: '100px', height: '100px' }}>
                                    <i className="pi pi-shield text-5xl text-blue-500"></i>
                                </div>
                                <h3 className="text-center font-medium mb-2">{t('auth.twoFactor.methods.microsoft')}</h3>
                            </div>
                        </Card>
                    </div>

                    <div className="col-12 md:col-4 p-2">
                        <Card
                            className={`cursor-pointer h-full flex flex-column align-items-center justify-content-center p-3 border-1 hover:border-primary transition-duration-150 ${selectedMethod === 'sms' ? 'border-primary' : 'border-300'
                                }`}
                            tabIndex={0}
                            onClick={() => handleSelect('sms')}
                            onKeyDown={(e) => handleKeyDown(e, 'sms')}
                            aria-label={t('auth.twoFactor.methods.sms')}
                        >
                            <div className="flex flex-column align-items-center">
                                <div className="mb-3 flex align-items-center justify-content-center bg-indigo-50 border-circle" style={{ width: '100px', height: '100px' }}>
                                    <i className="pi pi-send text-5xl text-indigo-500"></i>
                                </div>
                                <h3 className="text-center font-medium mb-2">{t('auth.twoFactor.methods.sms')}</h3>
                            </div>
                        </Card>
                    </div>
                </div>
            </div>
        </Dialog>
    );
};

export default TwoFactorAuthModal;
