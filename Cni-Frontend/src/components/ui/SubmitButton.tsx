'use client';
import React from 'react';
import { Button, ButtonProps } from 'primereact/button';
import classNames from 'classnames';

interface SubmitButtonProps extends Omit<ButtonProps, 'type' | 'text'> {
  loading?: boolean;
  text?: string;
}

const SubmitButton: React.FC<SubmitButtonProps> = ({
  loading,
  text = 'Kaydet',
  className,
  ...props
}) => {
  return (
    <Button
      {...props}
      type="submit"
      loading={loading}
      loadingIcon="pi pi-spin pi-spinner"
      icon="pi pi-check"
      className={classNames(
        'w-auto min-w-[120px]',
        className
      )}
    >
      {text}
    </Button>
  );
};

export default SubmitButton;
