'use client';
import React from 'react';
import TwoFactorAuthFlow from '../../../src/components/auth/TwoFactorAuthFlow';

export default function AuthExamplePage() {
    return (
        <div className="grid">
            <div className="col-12">
                <div className="card">
                    <TwoFactorAuthFlow />
                </div>
            </div>
        </div>
    );
}
