'use client';
import React from 'react';
import classNames from 'classnames';
import SwitchControlField from './SwitchControlField';
import FormActionButton from './FormActionButton';

// Type tanımlamaları
interface FormField {
  order?: number;
  name: string;
  [key: string]: any;
  rentingTypeId?: string;
  label?: string;
  component?: string;
  options?: any[];
  optionLabel?: string;
  optionValue?: string;
}

interface FormProps extends React.FormHTMLAttributes<HTMLFormElement> {
  control: any; // React Hook Form control tipi
  fields: FormField[];
  mode?: 'add' | 'edit' | 'detail';
  col?: number;
  isColumn?: boolean;
  close?: () => void;
  buttonIgnore?: boolean;
  className?: string;
  errors?: any;
}

const Form: React.FC<FormProps> = ({
  control,
  fields,
  mode,
  col = 6,
  isColumn,
  close,
  children,
  buttonIgnore,
  className,
  errors,
  ...props
}) => {

  const sortFields = (): FormField[] => {
    return fields?.sort((a, b) => (b.order || 0) - (a.order || 0));
  };

  return (
    <form
      {...props}
      className={classNames(className, {
        'flex flex-column gap-5': isColumn,
        'formgrid grid': !isColumn
      })}>
      {sortFields()?.map((item, idx) => {
        return (
          <SwitchControlField
            key={idx}
            control={control}
            col={col}
            item={item}
            keyProp={idx}
            isColumn={isColumn}
            mode={mode}
            name={item.name}
            errors={errors}
          />
        );
      })}
      {children}
      {!buttonIgnore && <FormActionButton mode={mode || ''} close={close || (() => {})  } />}
    </form>
  );
};

export default Form;
