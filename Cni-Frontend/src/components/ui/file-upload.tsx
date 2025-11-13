'use client';
import React from 'react';
import { FileUpload } from 'primereact/fileupload';
import classNames from 'classnames';

interface CustomFileUploadProps {
  name: string;
  url?: string;
  accept?: string;
  maxFileSize?: number;
  onUpload?: (e: any) => void;
  className?: string;
  containerClassName?: string;
  errorMsg?: string;
  disabled?: boolean;
  label?: string;
}

const CustomFileUpload = ({
  name,
  url = '/api/upload',
  accept = 'image/*',
  maxFileSize = 1000000,
  onUpload,
  className,
  containerClassName,
  errorMsg,
  disabled,
  label
}: CustomFileUploadProps) => {
  return (
    <div className={containerClassName}>
      <span className="p-float-label">
        <FileUpload

          mode="basic"
          name={name}
          url={url}
          accept={accept}
          maxFileSize={maxFileSize}
          onUpload={onUpload}
          disabled={disabled}
          className={classNames(className, { 'p-invalid': errorMsg })}
          chooseLabel={label || 'Dosya Seç'}
        />
        {errorMsg && <small className="p-error">{errorMsg}</small>}
      </span>
    </div>
  );
};

export default CustomFileUpload;
