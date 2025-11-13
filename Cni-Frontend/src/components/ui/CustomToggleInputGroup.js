/* eslint-disable react-hooks/exhaustive-deps */
'use client';
import { forwardRef, useEffect } from 'react';

import { ToggleButton } from 'primereact/togglebutton';

import Input from './Input';

const CustomToggleInputGroup = forwardRef(
  (
    {
      operatorName,
      operatorValue,
      operatorOnChange,
      percentageOrCurrencyName,
      percentageOrCurrencyValue,
      percentageOrCurrencyOnChange,
      formValues,
      disabledCurrency,
      disabledOperator,
      defaultPercentageOrCurrencyValue,
      defaultOperatorValue,
      onLabel = '%',
      offLabel = 'TRY',
      ...props
    },
    ref
  ) => {
    useEffect(() => {
      if (typeof defaultOperatorValue !== 'undefined' && typeof defaultPercentageOrCurrencyValue !== 'undefined')
        operatorOnChange({
          ...formValues,
          [operatorName]: defaultOperatorValue ? '-' : '+',
          [percentageOrCurrencyName]: defaultPercentageOrCurrencyValue
        });
    }, []);

    return (
      <div className="p-inputgroup flex-1">
        <ToggleButton
          id={operatorName}
          name={operatorName}
          className="border-round-left w-2 bg-orange-300 text-black-alpha-70 border-none p-0"
          onLabel=""
          offLabel=""
          onIcon="pi pi-minus"
          offIcon="pi pi-plus"
          checked={operatorValue}
          onChange={(e) =>
            operatorOnChange({
              ...formValues,
              [operatorName]: e.value ? '-' : '+'
            })
          }
          tooltip="Switch munis or plus"
          tooltipOptions={{ position: 'left' }}
          pt={{
            icon: {
              className: 'text-black-alpha-70'
            }
          }}
          disabled={disabledOperator}
        />
        <Input ref={ref} {...props} className="w-full border-noround" containerClassName="w-full" />
        <ToggleButton
          id={percentageOrCurrencyName}
          name={percentageOrCurrencyName}
          className="border-round-right w-2 bg-orange-300 text-black-alpha-70 p-0 "
          onLabel={onLabel}
          offLabel={offLabel}
          checked={percentageOrCurrencyValue}
          onChange={(e) => percentageOrCurrencyOnChange({ ...formValues, [percentageOrCurrencyName]: e.value })}
          tooltip="Switch % or currency"
          disabled={disabledCurrency}
        />
      </div>
    );
  }
);

CustomToggleInputGroup.displayName = 'CustomToggleInputGroup';

export default CustomToggleInputGroup;
