'use client';
import React from 'react';
import { Avatar } from 'primereact/avatar';
import { useTranslations } from 'next-intl';

import CustomFileUpload from './file-upload';

function CustomAvatar({
  imagePath,
  setValue,
  fileUploadProps,
  disabled,
  attachmentName,
  localPath = 'localImagePath',
  mode = 'basic',
  accept = '.jpg,.png,.jpeg,.gif,.svg,.webp'
}: {
  imagePath: string;
  setValue: (name: string, value: string) => void;
  fileUploadProps: any;
  disabled: boolean;
  attachmentName: string;
  localPath: string;
  mode: string;
  accept: string;
}) {
  const t = useTranslations();

  const onUpload = (data: any) => {
    const { files } = data;
    setValue(localPath, files[0]?.objectURL);
  };

  return (
    <div className="flex flex-column gap-4 align-items-center w-full">
      {imagePath ? (
        <Avatar
          image={imagePath}
          className="flex align-items-center justify-content-center mr-2"
          size="xlarge"
          shape="circle"
          style={{ borderRadius: '50%', height: '175px', width: '175px' }}
        />
      ) : (
        <Avatar icon="pi pi-user" size="xlarge" style={{ borderRadius: '50%', height: '175px', width: '175px' }} />
      )}

      <CustomFileUpload
        chooseOptions={{ className: 'p-button-info', icon: 'pi pi-cloud-upload' }}
        mode={mode}
        chooseLabel={t('general.imageUpload')}
        accept={accept}
        name={attachmentName}
        setValue={setValue}
        disabled={disabled}
        uploadHandler={onUpload}
        {...fileUploadProps}
      />
    </div>
  );
}

export default CustomAvatar;
