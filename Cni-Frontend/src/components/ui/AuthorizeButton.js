'use client';
import React, { useContext } from 'react';

import { Button } from 'primereact/button';

import { AppContext } from '@/src/context/AppContext';

function AuthorizeButton({ code, disabled, ...props }) {
  const { getAuthorizeCodes } = useContext(AppContext);
  if (
    code && Array.isArray(code)
      ? code.some((authorizeCode) => getAuthorizeCodes()?.includes(authorizeCode))
      : getAuthorizeCodes()?.includes(code)
  ) {
    return <Button {...props} disabled={disabled} />;
  }

  if (!code) {
    return (
      <Button
        {...props}
        disabled={disabled}
        // className="bg-red-500"
      />
    );
  }
}

export default AuthorizeButton;
