'use client';

import React, { useRef } from 'react';

import classNames from 'classnames';
import { InputTextarea } from 'primereact/inputtextarea';
import styled from 'styled-components';

import { useAutosizeTextArea } from '@/src/hooks/use-autosize-textarea';

const Wrapper = styled.div.attrs({
  className: 'flex'
})``;

const Textarea = ({ label, className, errorMsg, rows, cols, containerClassName, height, ...props }) => {
  return (
    <div className={containerClassName}>
      <Wrapper className={className}>
        <span className="p-float-label w-full">
          <InputTextarea
            style={{ height: height ? height : '48px', paddingTop: '14px' }}
            className={classNames(className, { 'p-invalid': errorMsg })}
            rows={rows}
            cols={cols}
            {...props}
          />
          {label && (
            <label htmlFor={props.name} className={errorMsg ? 'p-error' : ''}>
              {errorMsg && <span className="p-error">*</span>} {label}
            </label>
          )}
        </span>
      </Wrapper>
      {props.maxLength && (
        <div className="text-xs absolute right-0">
          <span>{props.value?.length}</span>
          <span>/ {props.maxLength}</span>
        </div>
      )}
      {errorMsg && <small className="p-error">{errorMsg}</small>}
    </div>
  );
};

Textarea.displayName = 'Textarea';
export default Textarea;
