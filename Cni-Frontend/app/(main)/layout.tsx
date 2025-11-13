import { Metadata } from 'next';
import Layout from '../../layout/layout';
import ProtectedRoute from '@/src/components/auth/ProtectedRoute';

interface AppLayoutProps {
    children: React.ReactNode;
}

export const metadata: Metadata = {
    title: 'Oklidis Panel',
    description: 'Oklidis Panel',
    robots: { index: false, follow: false },
    viewport: { initialScale: 1, width: 'device-width' },
    openGraph: {
        type: 'website',
        title: 'Oklidis Panel',
        url: '',
        description: 'Oklidis Panel',
        images: [''],
        ttl: 604800
    },
    icons: {
        icon: '/favicon.ico'
    }
};

export default function AppLayout({ children }: AppLayoutProps) {
    return (
        <ProtectedRoute>
            <Layout>{children}</Layout>
        </ProtectedRoute>
    );
}
