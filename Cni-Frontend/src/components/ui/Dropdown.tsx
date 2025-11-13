'use client';
import React, { forwardRef } from 'react';

import classNames from 'classnames';
import { Dropdown } from 'primereact/dropdown';
import { TreeSelect } from 'primereact/treeselect';
import styled from 'styled-components';

import media from '@/src/utils/media';
import { FloatLabel } from 'primereact/floatlabel';

const Container = styled.div`
  .p-dropdown,
  .p-treeselect {
    height: 48px;

    ${media.xs`
        min-width: 100%;
    `}

    .p-treeselect-label {
      padding: 0.675rem 0.75rem !important;
    }
  }

  .p-dropdown-label {
    display: flex;
    align-items: center;
  }
`;

interface Option {
  label: string;
  value: string;
  [key: string]: any;
}

interface CustomDropdownProps {
  containerClassName?: string;
  minWidth?: string;
  label: string;
  className?: string;
  errorMsg?: string;
  options: Option[];
  value?: string;
  optionValue?: string;
  filter?: boolean;
  disabledGetValue?: boolean;
  placeholder: string;
  onChange: (value: string) => void;
  as?: React.ElementType;
  name?: string;
}

const CustomDropdown = forwardRef<HTMLSelectElement, CustomDropdownProps>(
  (
    {
      containerClassName,
      minWidth = '200px',
      label,
      className,
      errorMsg,
      options,
      value,
      optionValue,
      filter = true,
      disabledGetValue = false,
      placeholder,
      onChange,
      as,
      name,
      ...props
    },
    ref
  ) => {
    const Component: React.ElementType = as ? TreeSelect : Dropdown;

    const getValue = () => {
      if (!value && options && options.length === 1 && !as && !disabledGetValue) {
        const currentValue = options[0];
        onChange(optionValue ? currentValue[optionValue] : currentValue);
        return optionValue ? currentValue[optionValue] : currentValue;
      }
      return value;
    };

    return (
      <Container className={containerClassName} style={{ minWidth }}>
        <FloatLabel>
          <Component
            inputRef={ref}
            className={classNames(className, { 'p-invalid': errorMsg })}
            id={name}
            autoComplete="false"
            filter={filter}
            options={options}
            optionValue={optionValue}
            value={getValue()}
            placeholder={placeholder}
            onChange={(e: any) => onChange(e.target.value)}
            {...props}
          />
          <label htmlFor={name} className={errorMsg ? 'p-error' : ''}>
            {errorMsg && <span className="p-error">*</span>}
            {label}
          </label>
        </FloatLabel>
        {errorMsg && <small className="p-error">{errorMsg}</small>}
      </Container>
    );
  }
);

CustomDropdown.displayName = 'CustomDropdown';

export default CustomDropdown;
