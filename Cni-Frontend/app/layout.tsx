import 'primereact/resources/primereact.css';
import 'primeflex/primeflex.css';
import 'primeicons/primeicons.css';
import '@/src/styles/layout/layout.scss';
import '@xyflow/react/dist/style.css';
import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';
import { getLocale } from 'next-intl/server';
import Providers from './providers';
import "primereact/resources/themes/lara-light-indigo/theme.css";
interface RootLayoutProps {
    children: React.ReactNode;
}

export default async function RootLayout({ children }: RootLayoutProps) {
    const locale = await getLocale();
    const messages = await getMessages();

    return (
        <html lang="en" suppressHydrationWarning>
            <head>
            <link id="theme-css" href={`/themes/lara-light-indigo/theme.css`} rel="stylesheet"></link>
            </head>
            <body>
                <NextIntlClientProvider messages={messages} locale={locale}>
                    <Providers>{children}</Providers>
                </NextIntlClientProvider>
            </body>
        </html>
    );
}
