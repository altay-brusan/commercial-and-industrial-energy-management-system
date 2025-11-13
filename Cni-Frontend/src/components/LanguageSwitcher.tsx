"use client"
import { useLocale } from 'next-intl';
import { useRouter, usePathname } from 'next/navigation';
import { Button } from 'primereact/button';
import { OverlayPanel } from 'primereact/overlaypanel';
import { useRef } from 'react';
import { setUserLocale } from '@/src/services/locale';
import { Locale } from '@/i18n/config';

type LanguageOption = {
    code: string;
    label: string;
};

const LanguageSwitcher = () => {
    const locale = useLocale();
    const opPanel = useRef<OverlayPanel>(null);

    const languages: LanguageOption[] = [
        { code: 'en', label: 'English' },
        { code: 'de', label: 'Deutsch' },
        { code: 'tr', label: 'Türkçe' },
    ];

    const handleLanguageChange = (newLocale: Locale) => {
        setUserLocale(newLocale);
        opPanel.current?.hide();
    };

    const languageContent = (
        <div className="flex flex-col gap-2 p-2 min-w-[150px]">
            {languages.map((lang) => (
                <Button
                    key={lang.code}
                    className={`p-2 text-left ${locale === lang.code ? 'bg-blue-500 text-white' : 'bg-gray-100 hover:bg-gray-200'
                        }`}
                    onClick={() => handleLanguageChange(lang.code as Locale)}
                    aria-label={`Switch to ${lang.label}`}
                    text
                    severity="secondary"
                >
                    {lang.label}
                </Button>
            ))}
        </div>
    );

    return (
        <div className="relative">
            <Button
                icon="pi pi-globe"
                aria-label="Change Language"
                aria-haspopup="true"
                size='large'
                aria-controls="language-selector"
                onClick={(e) => opPanel.current?.toggle(e)}
                className="p-button-rounded p-button-text"
            />
            <OverlayPanel ref={opPanel}>
                {languageContent}
            </OverlayPanel>
        </div>
    );
};

export default LanguageSwitcher;
