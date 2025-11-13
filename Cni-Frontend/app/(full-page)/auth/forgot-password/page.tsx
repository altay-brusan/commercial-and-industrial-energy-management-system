"use client"
import React, { useState, useRef, useContext } from "react";
import { Button } from "primereact/button";
import { Stepper } from "primereact/stepper";
import { StepperPanel } from "primereact/stepperpanel";
import classNames from "classnames";
import { LayoutContext } from "@/layout/context/layoutcontext";
import Input from "@/src/components/ui/Input";
import Link from "next/link";
import { useTranslations } from "next-intl";

const ForgotPasswordPage = () => {
    const t = useTranslations();
    const stepperRef = useRef<any>(null);
    const { layoutConfig } = useContext(LayoutContext);
    const containerClassName = classNames('surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden', { 'p-input-filled': layoutConfig.inputStyle === 'filled' });
    const [email, setEmail] = useState<string>("");
    const [emailError, setEmailError] = useState<string>("");

    const [verificationCode, setVerificationCode] = useState<string>("");
    const [verificationError, setVerificationError] = useState<string>("");

    const [password, setPassword] = useState<string>("");
    const [confirmPassword, setConfirmPassword] = useState<string>("");
    const [passwordError, setPasswordError] = useState<string>("");

    const isValidEmail = (email: string): boolean => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    };

    const handleEmailNext = (): void => {
        if (!email) {
            setEmailError(t("auth.forgotPassword.emailRequiredError"));
            return;
        }
        if (!isValidEmail(email)) {
            setEmailError(t("auth.forgotPassword.emailInvalidError"));
            return;
        }
        setEmailError("");
        stepperRef.current?.nextCallback();
    };

    const handleVerificationNext = (): void => {
        if (!verificationCode) {
            setVerificationError(t("auth.forgotPassword.verificationCodeRequiredError"));
            return;
        }
        setVerificationError("");
        stepperRef.current?.nextCallback();
    };

    const handlePasswordSubmit = (): void => {
        if (!password || !confirmPassword) {
            setPasswordError(t("auth.forgotPassword.allFieldsRequiredError"));
            return;
        }
        if (password !== confirmPassword) {
            setPasswordError(t("auth.forgotPassword.passwordMismatchError"));
            return;
        }
        setPasswordError("");
        console.log(t("auth.forgotPassword.passwordResetSuccessMessage"), { email, verificationCode, password });
    };

    return (
        <div className={containerClassName}>
            <div className="flex flex-column align-items-center justify-content-center mobile-w-full">
                <img src="/logo-light.svg" alt="logo" className="mb-5 w-20rem flex-shrink-0" />
                <div
                    className="mobile-w-full"
                    style={{
                        borderRadius: '56px',
                        padding: '0.3rem',
                        background: 'linear-gradient(180deg, var(--primary-color) 10%, rgba(33, 150, 243, 0) 30%)'
                    }}
                >
                    <div className="w-full surface-card py-8 px-5 sm:px-8" style={{ borderRadius: '53px' }}>
                        <Stepper linear={true} ref={stepperRef}>
                            <StepperPanel header={t("auth.forgotPassword.stepEmailHeader")}>
                                <div className="flex flex-column gap-3 card">
                                    <div className="flex flex-column">
                                        <Input
                                            id="emailInput"
                                            value={email}
                                            onChange={(e: any) => setEmail(e.target.value)}
                                            aria-label={t("auth.forgotPassword.emailInputLabel")}
                                            className="w-full"
                                            label={t("auth.forgotPassword.emailInputLabel")}
                                            errorMsg={emailError}
                                        />
                                    </div>
                                    <div className="flex justify-content-end">
                                        <Button
                                            label={t("auth.forgotPassword.continueButton")}
                                            icon="pi pi-arrow-right"
                                            iconPos="right"
                                            onClick={handleEmailNext}
                                            aria-label={t("auth.forgotPassword.continueButton")}
                                        />
                                    </div>
                                </div>
                            </StepperPanel>
                            <StepperPanel header={t("auth.forgotPassword.stepVerificationHeader")}>
                                <div className="flex flex-column gap-3">
                                    <div className="flex flex-column">
                                        <Input
                                            id="verificationInput"
                                            value={verificationCode}
                                            onChange={(e:any) => setVerificationCode(e.target.value)}
                                            aria-label={t("auth.forgotPassword.verificationInputLabel")}
                                            className="w-full"
                                            errorMsg={verificationError}
                                            label={t("auth.forgotPassword.verificationInputLabel")}
                                        />
                                    </div>
                                    <div className="flex justify-content-between">
                                        <Button
                                            label={t("auth.forgotPassword.backButton")}
                                            severity="secondary"
                                            icon="pi pi-arrow-left"
                                            onClick={() => stepperRef.current?.prevCallback()}
                                            aria-label={t("auth.forgotPassword.backButton")}
                                        />
                                        <Button
                                            label={t("auth.forgotPassword.continueButton")}
                                            icon="pi pi-arrow-right"
                                            iconPos="right"
                                            onClick={handleVerificationNext}
                                            aria-label={t("auth.forgotPassword.continueButton")}
                                        />
                                    </div>
                                </div>
                            </StepperPanel>
                            <StepperPanel header={t("auth.forgotPassword.stepPasswordHeader")}>
                                <div className="flex flex-column gap-3">
                                    <div className="flex flex-column">
                                        <Input
                                            id="passwordInput"
                                            value={password}
                                            onChange={(e: any) => setPassword(e.target.value)}
                                            aria-label={t("auth.forgotPassword.newPasswordInputLabel")}
                                            errorMsg={passwordError}
                                            type="password"
                                            label={t("auth.forgotPassword.newPasswordInputLabel")}
                                            className="w-full"
                                        />
                                    </div>
                                    <div className="flex flex-column">
                                        <Input
                                            id="confirmPasswordInput"
                                            value={confirmPassword}
                                            onChange={(e:any) => setConfirmPassword(e.target.value)}
                                            aria-label={t("auth.forgotPassword.confirmPasswordInputLabel")}
                                            errorMsg={passwordError}
                                            type="password"
                                            label={t("auth.forgotPassword.confirmPasswordInputLabel")}
                                            className="w-full"
                                        />
                                    </div>
                                    <div className="flex justify-content-between">
                                        <Button
                                            label={t("auth.forgotPassword.backButton")}
                                            severity="secondary"
                                            icon="pi pi-arrow-left"
                                            onClick={() => stepperRef.current?.prevCallback()}
                                            aria-label={t("auth.forgotPassword.backButton")}
                                        />
                                        <Button
                                            label={t("auth.forgotPassword.saveButton")}
                                            icon="pi pi-check"
                                            iconPos="right"
                                            onClick={handlePasswordSubmit}
                                            aria-label={t("auth.forgotPassword.saveButton")}
                                        />
                                    </div>
                                </div>
                            </StepperPanel>
                        </Stepper>
                        <Link href="/auth/login" className="pl-4 font-bold">
                            <i className="pi pi-arrow-left"></i> {t("auth.forgotPassword.loginLink")}
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ForgotPasswordPage;
