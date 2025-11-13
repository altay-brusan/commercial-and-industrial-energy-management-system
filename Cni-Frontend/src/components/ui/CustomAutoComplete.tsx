'use client';

import React, { forwardRef, useState } from 'react';
import { AutoComplete, AutoCompleteProps } from 'primereact/autocomplete';
import classNames from 'classnames';
import styled from 'styled-components';
import { tryCatch } from '@/src/utils/tryCatch';
import media from '@/src/utils/media';

const Container = styled.div`
  input {
    height: 48px;

    ${media.xs`
      min-width: 100%;
    `}
  }
`;

interface CustomAutoCompleteProps extends Omit<AutoCompleteProps, 'ref'> {
  containerClassName?: string;
  minWidth?: string;
  errorMsg?: string;
  label?: string;
  rightLabel?: React.ReactNode;
  endpoint?: (query: string) => Promise<any>;
  data?: () => Array<{ value: string }>;
  name?: string;
  optionValue?: string;
}

const CustomAutoComplete = forwardRef<HTMLInputElement, CustomAutoCompleteProps>(({
  containerClassName = '',
  minWidth = '200px',
  errorMsg,
  label,
  className,
  disabled,
  endpoint,
  data = () => [],
  ...props
}, ref) => {
  const [items, setItems] = useState<any[]>();

  const search = async (event: { query: string }) => {
    let result = [];

    if (endpoint) {
      const [err, res] = await tryCatch(endpoint(event.query));
      if (!err) {
        result = res;
      }
    }

    const dataResult = data();
    if (dataResult) {
      result = dataResult.filter((item) =>
        item.value.toLowerCase().includes(event.query.toLowerCase())
      );
    }

    setItems(result);
  };

  return (
    <Container className={containerClassName} style={{ minWidth }}>
      <span className={classNames('p-float-label', props.rightLabel && 'p-input-icon-right')}>
        {props.rightLabel && <i>{props.rightLabel}</i>}
        <AutoComplete
          {...props}
          inputId={props.name}
          inputRef={ref}
          suggestions={items}
          field={props.optionValue}
          completeMethod={search}
          disabled={disabled}
          className={className}
          inputClassName={classNames(className, { 'p-invalid': errorMsg })}
          dropdown
        />
        <label htmlFor={props.name} className={errorMsg ? 'p-error' : ''}>
          {errorMsg && <span className="p-error">*</span>}
          {label}
        </label>
      </span>
      {errorMsg && <small className="p-error">{errorMsg}</small>}
    </Container>
  );
});

CustomAutoComplete.displayName = 'CustomAutoComplete';

export default CustomAutoComplete;
