'use client';

import classNames from 'classnames';
import { useRouter } from 'next/navigation';

import { Button } from 'primereact/button';
import { useTranslations } from 'next-intl';

import { Mode } from '@/src/utils/mode';

import SubmitButton from './SubmitButton';

function FormActionButton({ close, mode }: { close: () => void; mode: string }) {
  const t = useTranslations();
  const { back } = useRouter();
  return (
    <div className={classNames('col md:col-12 text-center flex justify-content-end')}>
      <Button
        className="mr-3"
        type="button"
        icon="pi pi-arrow-left"
        label={t('general.goBack')}
        outlined
        onClick={() => (close ? close() : back())}
      />
      {mode !== Mode.DETAIL && <SubmitButton label={mode === Mode.CREATE ? t('general.save') : t('common.update')} />}
    </div>
  );
}


export default FormActionButton;
