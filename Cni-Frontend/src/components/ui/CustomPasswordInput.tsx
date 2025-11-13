'use client';
import React, { forwardRef } from 'react';

import classNames from 'classnames';
import { Password } from 'primereact/password';
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

const CustomPasswordInput = forwardRef(
  ({ errorMsg, containerClassName, label, minWidth = '200px', ...props }: any, ref) => {
    return (
      <Container className={containerClassName} style={{ minWidth }}>
        <span className="p-float-label">
        <Password
          {...props}
          inputRef={ref}
          inputClassName="w-full"
          id={props.name}
          autoComplete="off"
          className={classNames(props.className, { 'p-invalid': errorMsg })}
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

CustomPasswordInput.displayName = 'CustomPasswordInput';
export default CustomPasswordInput;
