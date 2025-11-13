import React, { forwardRef } from 'react';

import classNames from 'classnames';
import { InputText } from 'primereact/inputtext';
import styled from 'styled-components';

import media from '@/src/utils/media';
import { FloatLabel } from 'primereact/floatlabel';

const Container = styled.div`
  input {
    height: 48px;

    ${media.xs`
      min-width: 100%;
    `}
  }
`;

const Input = forwardRef((props: any, ref) => {
  const { containerClassName = '', minWidth = '200px', errorMsg, label, className, disabled } = props;

  return (
    <Container className={containerClassName} style={{ minWidth }}>
      <FloatLabel>
        {props.rightLabel && <i>{props.rightLabel}</i>}
        <InputText
          ref={ref}
          {...props}
          className={classNames(className, { 'p-invalid': errorMsg })}
          disabled={disabled}
          autoComplete="new-password"
        />
        <label htmlFor={props.name} className={errorMsg ? 'p-error' : ''}>
          {errorMsg && <span className="p-error">*</span>}
          {label}
        </label>
      </FloatLabel>
      {errorMsg && <small className="p-error">{errorMsg}</small>}
    </Container>
  );
});

Input.displayName = 'Input';
export default Input;
