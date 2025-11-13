'use client';
import React, { forwardRef } from 'react';

import classNames from 'classnames';
import { InputNumber } from 'primereact/inputnumber';
import styled from 'styled-components';

import media from '@/src/utils/media';

const Container = styled.div`
  input {
    height: 48px;

    ${media.xs`
      min-width: 100%;
    `}
  }
`;

const CustomInputNumber = forwardRef(
  (
    {
      containerClassName = '',
      minWidth = '200px',
      errorMsg,
      label,
      className,
      disabled,
      description,
      onChange,
      ...props
    }: any,
    ref
  ) => {
    return (
      <Container className={containerClassName} style={{ minWidth }}>
        {description && <div className="mb-3">{description}</div>}
        <span className={classNames('p-float-label', props.rightLabel && 'p-input-icon-right')}>
          {props.rightLabel && <i>{props.rightLabel}</i>}
          <InputNumber
            {...props}
            inputRef={ref}
            onValueChange={(e) => onChange(e)}
            className={classNames(className, { 'p-invalid': errorMsg, 'p-disabled': disabled })}
            disabled={disabled}
            autoComplete="new-password"
          />
          <label htmlFor={props.name} className={classNames(errorMsg ? 'p-error' : '', { 'p-disabled': disabled })}>
            {errorMsg && <span className="p-error">*</span>}
            {label}
          </label>
        </span>
        {errorMsg && <small className="p-error">{errorMsg}</small>}
      </Container>
    );
  }
);

CustomInputNumber.displayName = 'CustomInputNumber';

export default CustomInputNumber;
