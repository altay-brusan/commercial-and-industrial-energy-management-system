'use client';
import React from 'react';

import classNames from 'classnames';
import { MultiSelect } from 'primereact/multiselect';
import styled from 'styled-components';

import media from '@/src/utils/media';

const Container = styled.div`
  .p-multiselect {
    height: 48px;

    ${media.xs`
      min-width: 200px
    `}
  }
`;

function CustomMultiSelect({ minWidth = '200px', name, label, className, errorMsg, containerClassName, ...props }: any) {
  return (
    <Container className={containerClassName} style={{ minWidth }}>
      <span className="p-float-label">
        <MultiSelect
          display="chip"
          className={classNames(className, { 'p-invalid': errorMsg })}
          maxSelectedLabels={3}
          {...props}
        />
        <label htmlFor={name} className={errorMsg ? 'p-error' : ''}>
          {errorMsg && <span className="p-error">*</span>}
          {label}
        </label>
      </span>
      {errorMsg && <small className="p-error">{errorMsg}</small>}
    </Container>
  );
}

CustomMultiSelect.displayName = 'CustomMultiSelect';

export default CustomMultiSelect;
