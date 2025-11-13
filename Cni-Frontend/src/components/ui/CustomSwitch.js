'use client';
import React, { forwardRef } from 'react';

import { InputSwitch } from 'primereact/inputswitch';
import styled from 'styled-components';

const Container = styled.div`
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
`;

const Switch = forwardRef(({ containerClassName = '', label, className, disabled, value, ...props }, ref) => {
  return (
    <>
      <Container className={containerClassName}>
        <label htmlFor={props.name}>{label}</label>
        <InputSwitch ref={ref} {...props} className={className} disabled={disabled} checked={value} />
      </Container>
    </>
  );
});

Switch.displayName = 'Switch';
export default Switch;
