'use client';
import React, { forwardRef } from 'react';

import classNames from 'classnames';
import { Calendar } from 'primereact/calendar';
import styled from 'styled-components';
import media from '@/src/utils/media';

const Container = styled.div`
  .p-calendar {
    height: 48px;

    ${media.xs`
      min-width: 100%
    `}
  }
`;

const CustomCalendar = forwardRef(
  (
    {
      minWidth = '200px',
      label,
      selectionMode = 'single',
      className,
      errorMsg,
      containerClassName,
      dateFormat = 'dd/mm/yy',
      ...props
    }: any,
    ref
  ) => {
    return (
      <Container className={containerClassName} style={{ minWidth }}>
        <span className="p-float-label">
          <Calendar
            {...props}
            dateFormat={dateFormat}
            inputRef={ref}
            inputId={props.name}
            selectionMode={selectionMode}
            showIcon
            className={classNames(className, { 'p-invalid': errorMsg })}
          />
          <label htmlFor={props.name} className={errorMsg ? 'p-error' : ''}>
            {errorMsg && <span className="p-error">*</span>}
            {label}
          </label>
        </span>
        {errorMsg && <small className="p-error">{errorMsg}</small>}
      </Container>
    );
  }
);


CustomCalendar.displayName = 'CustomCalendar';

export default CustomCalendar;
