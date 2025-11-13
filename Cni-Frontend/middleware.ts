import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export async function middleware(request: NextRequest) {
    const response = await fetch(request.nextUrl.toString(), { method: 'HEAD' });

    if (response.status === 404) {
        return NextResponse.redirect(new URL('/', request.url));
    }

    return NextResponse.next();
}

export const config = {
    matcher: '/:path*',
};
