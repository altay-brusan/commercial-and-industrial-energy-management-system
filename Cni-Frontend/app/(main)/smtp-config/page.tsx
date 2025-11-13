'use client';
import React, { useState } from 'react';
import SMTPConfiguration from '../../../src/components/config/SMTPConfiguration';
import { Message } from 'primereact/message';

export default function SMTPConfigPage() {
    const [savedConfig, setSavedConfig] = useState(null);

    const handleSave = async (config: any): Promise<void> => {
        return new Promise<void>((resolve) => {
            setTimeout(() => {
                setSavedConfig(config);
                resolve();
            }, 1000);
        });
    };

    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <h1 className="text-2xl font-bold mb-4">SMTP Yapılandırması</h1>
                    <p className="mb-5 text-color-secondary">
                        E-posta gönderimi için SMTP sunucu ayarlarını yapılandırın. Bu ayarlar, sistem bildirimleri, kullanıcı bildirimleri ve diğer e-posta iletişimleri için kullanılacaktır.
                    </p>

                    {savedConfig && (
                        <div className="mb-4">
                            <Message
                                severity="success"
                                text="SMTP ayarları başarıyla kaydedildi ve şu anda etkin."
                                className="w-full"
                            />
                        </div>
                    )}

                    <SMTPConfiguration
                        onSave={handleSave}
                        initialConfig={{
                            server: 'smtp.example.com',
                            port: 587,
                            encryption: 'tls'
                        }}
                    />
                </div>
            </div>
        </div>
    );
}
