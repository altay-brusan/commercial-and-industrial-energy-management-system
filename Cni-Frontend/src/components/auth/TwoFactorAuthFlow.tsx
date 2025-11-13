import React, { useState, useEffect, useCallback } from 'react';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { InputText } from 'primereact/inputtext';
import { Password } from 'primereact/password';
import TwoFactorAuthModal from './TwoFactorAuthModal';
import { Card } from 'primereact/card';
import { InputOtp } from 'primereact/inputotp';
import { useTranslations } from 'next-intl';
import { SiGoogleauthenticator } from "react-icons/si";
import { ProgressBar } from 'primereact/progressbar';
import { Badge } from 'primereact/badge';
import { QRCodeCanvas } from 'qrcode.react';
import { FaMicrosoft } from 'react-icons/fa';

type TwoFactorAuthOption = 'google' | 'microsoft' | 'sms' | null;

const TwoFactorAuthFlow = () => {
    const t = useTranslations();
    const [loginModalVisible, setLoginModalVisible] = useState(false);
    const [twoFactorModalVisible, setTwoFactorModalVisible] = useState(false);
    const [codeVerificationVisible, setCodeVerificationVisible] = useState(false);
    const [selectedMethod, setSelectedMethod] = useState<TwoFactorAuthOption>(null);
    const [verificationCode, setVerificationCode] = useState('');
    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);
    const [countdown, setCountdown] = useState(60);
    const [isRefreshDisabled, setIsRefreshDisabled] = useState(false);
    const [userEmail, setUserEmail] = useState('user@example.com');
    const [otpSecret, setOtpSecret] = useState('JBSWY3DPEHPK3PXP');

    const resetForm = () => {
        setCountdown(60);
        setIsRefreshDisabled(false);
        setLoginModalVisible(false);
        setTwoFactorModalVisible(false);
        setCodeVerificationVisible(false);
        setSelectedMethod(null);
        setVerificationCode('');
        setLoading(false);
        setSuccess(false);
    };

    const generateOtpUri = () => {
        return `otpauth://totp/Oklidis:${userEmail}?secret=${otpSecret}&issuer=Oklidis&algorithm=SHA1&digits=6&period=30`;
    };

    const handleLogin = () => {
        setLoading(true);
        setTimeout(() => {
            setLoading(false);
            setLoginModalVisible(false);
            setTwoFactorModalVisible(true);
        }, 1000);
    };

    const startCountdown = useCallback(() => {
        setCountdown(60);
        setIsRefreshDisabled(false);
        const timer = setInterval(() => {
            setCountdown((prev) => {
                if (prev <= 1) {
                    clearInterval(timer);
                    setIsRefreshDisabled(false);
                    return 0;
                }
                return prev - 1;
            });
        }, 1000);

        return () => clearInterval(timer);
    }, []);

    useEffect(() => {
        if (codeVerificationVisible) {
            const cleanup = startCountdown();
            return () => cleanup();
        }
    }, [codeVerificationVisible, startCountdown]);

    const handleRefresh = async () => {
        setIsRefreshDisabled(true);
        setVerificationCode('');
        setLoading(true);

        try {
            // API çağrısı burada yapılacak
            await new Promise(resolve => setTimeout(resolve, 1000));
            setTwoFactorModalVisible(true);
            setCodeVerificationVisible(false);
        } catch (error) {
            console.error('Yenileme hatası:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleAuthMethodSelect = (method: TwoFactorAuthOption) => {
        setSelectedMethod(method);
        setTimeout(() => {
            setTwoFactorModalVisible(false);
            setCodeVerificationVisible(true);
        }, 500);
    };

    const handleCodeVerification = () => {
        setLoading(true);
        setTimeout(() => {
            setLoading(false);
            setCodeVerificationVisible(false);
            setSuccess(true);
        }, 1500);
    };

    const renderLoginModal = () => {
        return (
            <Dialog
                header={t('auth.login.title')}
                visible={loginModalVisible}
                style={{ width: '30rem' }}
                breakpoints={{ '960px': '75vw', '641px': '90vw' }}
                onHide={() => setLoginModalVisible(false)}
            >
                <div className="p-fluid mt-2">
                    <div className="field mb-4">
                        <label htmlFor="email" className="font-medium text-900 mb-2 block">{t('auth.login.email')}</label>
                        <InputText id="email" type="email" placeholder={t('auth.login.emailPlaceholder')} aria-label={t('auth.login.email')} />
                    </div>
                    <div className="field mb-4">
                        <label htmlFor="password" className="font-medium text-900 mb-2 block">{t('auth.login.password')}</label>
                        <Password id="password" toggleMask feedback={false} placeholder={t('auth.login.passwordPlaceholder')} aria-label={t('auth.login.password')} />
                    </div>
                    <Button
                        label={t('auth.login.loginButton')}
                        className="mt-2"
                        onClick={handleLogin}
                        loading={loading}
                        aria-label={t('auth.login.loginButton')}
                    />
                </div>
            </Dialog>
        );
    };

    const renderCodeVerificationModal = () => {
        let methodText = "";
        let methodIcon = "";

        switch (selectedMethod) {
            case 'google':
                methodText = "Google Authenticator";
                methodIcon = "pi-clock";
                break;
            case 'microsoft':
                methodText = "Microsoft Authenticator";
                methodIcon = "pi-shield";
                break;
            case 'sms':
                methodText = "SMS";
                methodIcon = "pi-send";
                break;
        }

        return (
            <Dialog
                header={t('auth.twoFactor.verification.title')}
                visible={codeVerificationVisible}
                style={{ width: '30rem' }}
                breakpoints={{ '960px': '75vw', '641px': '90vw' }}
                onHide={() => setCodeVerificationVisible(false)}
            >
                <div className="p-fluid">
                    <div className="flex align-items-center justify-content-center mb-4">
                        {selectedMethod === 'google' ? (
                            <SiGoogleauthenticator className="text-3xl text-primary mr-2" />
                        ) : selectedMethod === 'microsoft' ? (
                            <FaMicrosoft className="text-3xl text-primary mr-2" />
                        ) : (
                            <i className={`pi ${methodIcon} text-3xl text-primary mr-2`}></i>
                        )}
                        <span className="text-lg font-medium">{methodText}</span>
                    </div>

                    <p className="text-center mb-4">
                        {selectedMethod === 'sms'
                            ? t('auth.twoFactor.verification.smsDescription')
                            : t('auth.twoFactor.verification.appDescription', { method: methodText })}
                    </p>

                    {selectedMethod === 'microsoft' && (
                        <div className="flex flex-column align-items-center justify-content-center mb-4">
                            <div className="surface-card p-3 border-1 border-round shadow-2 mb-3">
                                <h4 className="text-center font-medium mt-0 mb-3">Cihazınızı kaydetmek için barkodu tarayın</h4>
                                <div className="flex justify-content-center p-2 bg-white border-1 border-round">
                                    <QRCodeCanvas
                                        value={generateOtpUri()}
                                        size={200}
                                        level="H"
                                        imageSettings={{
                                            src: '/logo-light.svg',
                                            x: undefined,
                                            y: undefined,
                                            height: 10,
                                            width: 60,
                                            excavate: true,
                                        }}
                                    />
                                </div>
                                <p className="text-center text-sm text-color-secondary mt-2 mb-0">
                                    Microsoft Authenticator uygulamasını açın ve QR kodu tarayın
                                </p>
                            </div>
                        </div>
                    )}

                    <div className="flex flex-column align-items-center gap-3 mb-4">
                        <div className="flex align-items-center justify-content-between w-full">
                            <Button
                                icon="pi pi-refresh"
                                className="p-button-rounded p-button-outlined"
                                disabled={isRefreshDisabled || loading || countdown > 0}
                                onClick={handleRefresh}
                                loading={loading}
                                aria-label={t('auth.twoFactor.verification.refreshButton')}
                                tooltip={countdown > 0 ? t('auth.twoFactor.verification.waitForRefresh') : t('auth.twoFactor.verification.clickToRefresh')}
                                tooltipOptions={{ position: 'top' }}
                            />
                            <Badge
                                value={countdown > 0 ? `${countdown}` : '✓'}
                                severity={countdown > 0 ? 'info' : 'success'}
                                size="large"
                            />
                        </div>
                        {countdown > 0 && (
                            <ProgressBar
                                value={(countdown / 60) * 100}
                                showValue={false}
                                className="w-full"
                                style={{ height: '4px' }}
                            />
                        )}
                    </div>
                    <div className="field mb-4">
                        <label htmlFor="code" className="font-medium mb-2 block">{t('auth.twoFactor.verification.codeLabel')}</label>
                        <div className="flex justify-content-center">
                            <InputOtp
                                id="code"
                                value={verificationCode}
                                onChange={(e: any) => setVerificationCode(e.value)}
                                length={6}
                                className="text-center"
                                aria-label="Doğrulama Kodu"
                                autoFocus
                            />
                        </div>
                    </div>

                    <Button
                        label={t('auth.twoFactor.verification.verifyButton')}
                        className="mt-2"
                        onClick={handleCodeVerification}
                        disabled={verificationCode.length !== 6}
                        loading={loading}
                        aria-label={t('auth.twoFactor.verification.verifyButton')}
                    />
                </div>
            </Dialog>
        );
    };

    return (
        <div className="flex flex-column align-items-center justify-content-center p-4">
            {renderLoginModal()}

            <TwoFactorAuthModal
                visible={twoFactorModalVisible}
                onHide={() => setTwoFactorModalVisible(false)}
                onSelect={handleAuthMethodSelect}
            />

            {renderCodeVerificationModal()}

            {!success ? (
                <Button
                    label={t('auth.login.sessionButton')}
                    icon="pi pi-user"
                    className="p-button-lg"
                    onClick={() => setLoginModalVisible(true)}
                    aria-label={t('auth.login.sessionButton')}
                />
            ) : (
                <Card className="text-center p-5 shadow-4">
                    <i className="pi pi-check-circle text-6xl text-green-500 mb-3"></i>
                    <h2 className="text-xl font-medium mb-3">{t('auth.success.title')}</h2>
                    <p className="mb-4">{t('auth.success.message')}</p>
                    <Button
                        label={t('auth.success.retryButton')}
                        icon="pi pi-refresh"
                        onClick={resetForm}
                        aria-label={t('auth.success.retryButton')}
                    />
                </Card>
            )}
        </div>
    );
};

export default TwoFactorAuthFlow;
