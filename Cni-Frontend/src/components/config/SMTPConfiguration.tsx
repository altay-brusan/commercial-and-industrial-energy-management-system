import React, { useRef, useState } from 'react';
import { InputText } from 'primereact/inputtext';
import { Password } from 'primereact/password';
import { Button } from 'primereact/button';
import { Dropdown } from 'primereact/dropdown';
import { InputSwitch } from 'primereact/inputswitch';
import { InputNumber } from 'primereact/inputnumber';
import { Card } from 'primereact/card';
import { Toast } from 'primereact/toast';
import { Dialog } from 'primereact/dialog';

type SMTPConfigType = {
    server: string;
    port: number;
    username: string;
    password: string;
    encryption: 'none' | 'ssl' | 'tls';
    fromEmail: string;
    fromName: string;
    enableAuth: boolean;
    timeout: number;
};

type SMTPConfigurationProps = {
    onSave?: (config: SMTPConfigType) => Promise<void>;
    initialConfig?: Partial<SMTPConfigType>;
};

const SMTPConfiguration = ({ onSave, initialConfig }: SMTPConfigurationProps) => {
    const toast = useRef<Toast>(null);
    const defaultConfig: SMTPConfigType = {
        server: '',
        port: 587,
        username: '',
        password: '',
        encryption: 'tls',
        fromEmail: '',
        fromName: '',
        enableAuth: true,
        timeout: 30
    };

    const [config, setConfig] = useState<SMTPConfigType>({
        ...defaultConfig,
        ...initialConfig
    });

    const [testing, setTesting] = useState<boolean>(false);
    const [testEmailModalVisible, setTestEmailModalVisible] = useState<boolean>(false);
    const [testEmail, setTestEmail] = useState<string>('');
    const [saving, setSaving] = useState<boolean>(false);

    const encryptionOptions = [
        { label: 'Yok', value: 'none' },
        { label: 'SSL', value: 'ssl' },
        { label: 'TLS', value: 'tls' }
    ];

    const updateConfig = (key: keyof SMTPConfigType, value: any) => {
        setConfig(prev => ({
            ...prev,
            [key]: value
        }));
    };

    const handleSave = async () => {
        if (!validateForm()) return;

        setSaving(true);
        try {
            if (onSave) {
                await onSave(config);
            }
            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: 'SMTP ayarları başarıyla kaydedildi.',
                life: 3000
            });
        } catch (error) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'SMTP ayarları kaydedilirken bir hata oluştu.',
                life: 3000
            });
        } finally {
            setSaving(false);
        }
    };

    const validateForm = (): boolean => {
        if (!config.server) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'SMTP sunucusu adresi gereklidir.',
                life: 3000
            });
            return false;
        }

        if (!config.port) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Port numarası gereklidir.',
                life: 3000
            });
            return false;
        }

        if (config.enableAuth && (!config.username || !config.password)) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Kimlik doğrulama etkinleştirildiğinde kullanıcı adı ve şifre gereklidir.',
                life: 3000
            });
            return false;
        }

        if (!config.fromEmail) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Gönderen e-posta adresi gereklidir.',
                life: 3000
            });
            return false;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(config.fromEmail)) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Geçerli bir e-posta adresi girin.',
                life: 3000
            });
            return false;
        }

        return true;
    };

    const handleTestConnection = () => {
        if (!validateForm()) return;
        setTestEmailModalVisible(true);
    };

    const sendTestEmail = async () => {
        if (!testEmail) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Test e-postası için geçerli bir e-posta adresi girin.',
                life: 3000
            });
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(testEmail)) {
            toast.current?.show({
                severity: 'error',
                summary: 'Hata',
                detail: 'Geçerli bir e-posta adresi girin.',
                life: 3000
            });
            return;
        }

        setTesting(true);

        setTimeout(() => {
            setTesting(false);
            setTestEmailModalVisible(false);
            toast.current?.show({
                severity: 'success',
                summary: 'Başarılı',
                detail: `Test e-postası ${testEmail} adresine başarıyla gönderildi.`,
                life: 3000
            });
            setTestEmail('');
        }, 2000);
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            sendTestEmail();
        }
    };

    const renderTestEmailModal = () => {
        return (
            <Dialog
                header="Test E-postası Gönder"
                visible={testEmailModalVisible}
                style={{ width: '30rem' }}
                breakpoints={{ '960px': '75vw', '641px': '90vw' }}
                onHide={() => setTestEmailModalVisible(false)}
            >
                <div className="p-fluid">
                    <div className="field mb-4">
                        <label htmlFor="testEmail" className="font-medium text-900 mb-2 block">
                            Alıcı E-posta Adresi
                        </label>
                        <InputText
                            id="testEmail"
                            value={testEmail}
                            onChange={(e) => setTestEmail(e.target.value)}
                            placeholder="örnek@domain.com"
                            disabled={testing}
                            aria-label="Test E-posta Adresi"
                            onKeyDown={handleKeyDown}
                        />
                    </div>

                    <Button
                        label="Gönder"
                        icon="pi pi-send"
                        loading={testing}
                        onClick={sendTestEmail}
                        aria-label="Test E-postası Gönder"
                    />
                </div>
            </Dialog>
        );
    };

    return (
        <div className="">
            <Toast ref={toast} position="top-right" />
            {renderTestEmailModal()}
            <div className="flex align-items-center justify-content-between mb-4">
                <h2 className="text-xl font-medium m-0">SMTP Konfigürasyonu</h2>
            </div>

            <div className="grid">
                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="server" className="font-medium text-900 mb-2 block">
                        SMTP Sunucusu
                    </label>
                    <InputText
                        id="server"
                        value={config.server}
                        onChange={(e) => updateConfig('server', e.target.value)}
                        placeholder="smtp.example.com"
                        aria-label="SMTP Sunucusu"
                        className="w-full"
                    />
                </div>

                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="port" className="font-medium text-900 mb-2 block">
                        Port
                    </label>
                    <InputNumber
                        id="port"
                        value={config.port}
                        onValueChange={(e) => updateConfig('port', e.value ?? 587)}
                        placeholder="587"
                        min={0}
                        max={65535}
                        aria-label="Port"
                        className="w-full"
                        inputClassName="w-full"
                    />
                </div>

                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="encryption" className="font-medium text-900 mb-2 block">
                        Şifreleme
                    </label>
                    <Dropdown
                        id="encryption"
                        value={config.encryption}
                        options={encryptionOptions}
                        onChange={(e) => updateConfig('encryption', e.value)}
                        placeholder="Şifreleme Yöntemi Seçin"
                        aria-label="Şifreleme Yöntemi"
                        className="w-full"
                    />
                </div>

                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="timeout" className="font-medium text-900 mb-2 block">
                        Zaman Aşımı (saniye)
                    </label>
                    <InputNumber
                        id="timeout"
                        value={config.timeout}
                        onValueChange={(e) => updateConfig('timeout', e.value ?? 30)}
                        placeholder="30"
                        min={1}
                        max={300}
                        aria-label="Zaman Aşımı"
                        className="w-full"
                        inputClassName="w-full"
                    />
                </div>

                <div className="col-12 mb-4">
                    <div className="flex align-items-center">
                        <InputSwitch
                            checked={config.enableAuth}
                            onChange={(e) => updateConfig('enableAuth', e.value)}
                            aria-label="Kimlik Doğrulamayı Etkinleştir"
                        />
                        <label className="font-medium text-900 ml-2">
                            SMTP Kimlik Doğrulamayı Etkinleştir
                        </label>
                    </div>
                </div>

                {config.enableAuth && (
                    <>
                        <div className="col-12 md:col-6 mb-4">
                            <label htmlFor="username" className="font-medium text-900 mb-2 block">
                                Kullanıcı Adı
                            </label>
                            <InputText
                                id="username"
                                value={config.username}
                                onChange={(e) => updateConfig('username', e.target.value)}
                                placeholder="email@example.com"
                                aria-label="Kullanıcı Adı"
                                className="w-full"
                            />
                        </div>

                        <div className="col-12 md:col-6 mb-4">
                            <label htmlFor="password" className="font-medium text-900 mb-2 block">
                                Şifre
                            </label>
                            <Password
                                id="password"
                                value={config.password}
                                onChange={(e) => updateConfig('password', e.target.value)}
                                placeholder="********"
                                feedback={false}
                                toggleMask
                                aria-label="Şifre"
                                className="w-full"
                                inputClassName="w-full"
                            />
                        </div>
                    </>
                )}

                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="fromEmail" className="font-medium text-900 mb-2 block">
                        Gönderen E-posta Adresi
                    </label>
                    <InputText
                        id="fromEmail"
                        value={config.fromEmail}
                        onChange={(e) => updateConfig('fromEmail', e.target.value)}
                        placeholder="noreply@example.com"
                        aria-label="Gönderen E-posta Adresi"
                        className="w-full"
                    />
                </div>

                <div className="col-12 md:col-6 mb-4">
                    <label htmlFor="fromName" className="font-medium text-900 mb-2 block">
                        Gönderen Adı
                    </label>
                    <InputText
                        id="fromName"
                        value={config.fromName}
                        onChange={(e) => updateConfig('fromName', e.target.value)}
                        placeholder="Şirket Adı"
                        aria-label="Gönderen Adı"
                        className="w-full"
                    />
                </div>

                <div className="col-12 flex justify-content-end gap-2 mt-4">
                    <Button
                        label="Test Bağlantısı"
                        icon="pi pi-send"
                        className="p-button-outlined"
                        onClick={handleTestConnection}
                        aria-label="Test Bağlantısı"
                    />
                    <Button
                        label="Kaydet"
                        icon="pi pi-save"
                        loading={saving}
                        onClick={handleSave}
                        aria-label="Kaydet"
                    />
                </div>
            </div>
        </div>
    );
};

export default SMTPConfiguration;
