/* eslint-disable @next/next/no-img-element */
'use client';
import { useRouter } from 'next/navigation';
import React, { useContext, useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Button } from 'primereact/button';
import { LayoutContext } from '../../../../layout/context/layoutcontext';
import { InputText } from 'primereact/inputtext';
import { classNames } from 'primereact/utils';
import { tryCatch } from '@/src/utils/tryCatch';
import { authService } from '@/src/services/auth';
import { useLoading } from '@/src/context/LoadingContext';
import { useToaster } from '@/src/context/ToasterContext';
import { isAuthenticated } from '@/src/utils/auth';
import { httpClient } from '@/src/services/http';
import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { loadCaptchaEnginge, LoadCanvasTemplate, validateCaptcha } from 'react-simple-captcha';
import type { NextPage } from 'next';
import { Skeleton } from 'primereact/skeleton';

const LoginPage: NextPage = () => {
    const t = useTranslations();
    const { showLoading, hideLoading, isLoading } = useLoading();
    const { openToaster } = useToaster();
    const { register, handleSubmit } = useForm();
    const { layoutConfig } = useContext(LayoutContext);
    const router = useRouter();
    const containerClassName = classNames('surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden', { 'p-input-filled': layoutConfig.inputStyle === 'filled' });
    const [isClient, setIsClient] = useState(false);
    const [isCaptchaLoaded, setIsCaptchaLoaded] = useState(false);
    useEffect(() => {
        setIsClient(true);
        if (isAuthenticated()) {
            router.push('/');
        }
    }, [router]);

    useEffect(() => {
        setTimeout(() => {
            setIsCaptchaLoaded(true);
        }, 1000);

        setTimeout(() => {
            if (typeof loadCaptchaEnginge === 'function') {
                loadCaptchaEnginge(6);
            }
        }, 2000);
    }, []);

    if (!isClient) {
        return null;
    }

    const onSubmit = async (data: any) => {
        if (!validateCaptcha(data.captcha)) {
            openToaster('top-right', { severity: 'error', summary: 'Error', detail: 'Invalid captcha' });
            return;
        }

        showLoading();
        const [err, res] = await tryCatch(authService.login(data));
        if (err) {
            openToaster('top-right', { severity: 'error', summary: 'Error', detail: err.message });
            hideLoading();
        } else {
            localStorage.setItem('accessToken', res.accessToken);
            localStorage.setItem('refreshToken', res.refreshToken);

            httpClient.updateToken(res.accessToken);

            const [userInfoErr, userInfoRes] = await tryCatch(authService.userInfo());
            if (userInfoErr) {
                console.log(userInfoErr);
            } else {
                localStorage.setItem('userInfo', JSON.stringify(userInfoRes));
            }

            router.push('/');
        }
        hideLoading();
    };

    return (
        <div className={containerClassName}>
            <div className="flex flex-column align-items-center justify-content-center">
                <img src="/logo-light.svg" alt="logo" className="mb-5 w-20rem flex-shrink-0" />
                <div
                    style={{
                        borderRadius: '56px',
                        padding: '0.3rem',
                        background: 'linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)'
                    }}
                >
                    <form onSubmit={handleSubmit(onSubmit)} className="w-full surface-card py-8 px-5 sm:px-8" style={{ borderRadius: '53px' }}>
                        <div>
                            <label htmlFor="email1" className="block text-900 text-xl font-medium mb-2">
                                {t("auth.login.emailInputLabel")}
                            </label>
                            <InputText id="email1" type="text" placeholder={t("auth.login.emailInputPlaceholder")} className="w-full md:w-30rem mb-5" style={{ padding: '1rem' }} {...register('email')} />

                            <label htmlFor="password1" className="block text-900 font-medium text-xl mb-2">
                                {t("auth.login.passwordInputLabel")}
                            </label>
                            <InputText id="password1" type="password" placeholder="Password" className="w-full md:w-30rem mb-5" style={{ padding: '1rem' }} {...register('password')} />
                            <div className='card p-4'>
                                {!isCaptchaLoaded ? <Skeleton height='5rem' width="100%" className="mb-2"></Skeleton> : <LoadCanvasTemplate />}
                                <InputText id="captcha" type="text" placeholder="Enter the captcha" className="w-full mt-5" style={{ padding: '1rem' }} {...register('captcha')} />
                            </div>
                            <div className="flex align-items-center justify-content-between mb-5 gap-5">
                                <Link href={"/auth/forgot-password"} className="font-medium no-underline ml-2 text-right cursor-pointer" style={{ color: 'var(--primary-color)' }}>
                                        {t("auth.login.forgotPasswordLink")}
                                </Link>
                            </div>
                            <Button loading={isLoading} label={t("auth.login.signInButton")} className="w-full p-3 text-xl" type="submit"></Button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default LoginPage;
