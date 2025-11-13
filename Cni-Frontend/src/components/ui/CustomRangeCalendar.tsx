/* eslint-disable react-hooks/exhaustive-deps */
'use client';

import React, { forwardRef, useEffect, useState } from 'react';

import { Button } from 'primereact/button';
import { Calendar } from 'primereact/calendar';

interface CustomRangeCalendarProps {
  onChange: (dates: Date[]) => void;
  disabled?: boolean;
  range?: number;
  dateFormat?: string;
  value?: Date[];
}

const CustomRangeCalendar = forwardRef<HTMLInputElement, CustomRangeCalendarProps>(
  ({ onChange, disabled, range = 14, dateFormat = 'dd/mm/yy', ...props }, ref) => {
    const [value, setValue] = useState<Date[]>(props.value || []);

    useEffect(() => {
      handleDateChange(new Date());
    }, []);

    const handlePrevClick = (e: React.MouseEvent) => {
      e.preventDefault();

      const startDate = new Date(value[0].getTime() - range * 24 * 60 * 60 * 1000);
      const endDate = new Date(startDate.getTime() + range * 24 * 60 * 60 * 1000);
      setValue([startDate, endDate]);
      onChange([startDate, endDate]);
    };

    const handleNextClick = (e: React.MouseEvent) => {
      e.preventDefault();

      const startDate = new Date(value[0].getTime() + range * 24 * 60 * 60 * 1000);
      const endDate = new Date(startDate.getTime() + range * 24 * 60 * 60 * 1000);
      setValue([startDate, endDate]);
      onChange([startDate, endDate]);
    };

    const handleDateChange = (firstDate: Date) => {
      const startDate = firstDate;
      const endDate = new Date(startDate.getTime() + range * 24 * 60 * 60 * 1000);
      setValue([startDate, endDate]);
      onChange([startDate, endDate]);
    };

    return (
      <div className="flex justify-content-center p-2 justify-content-between">
        <Button
          icon="pi pi-chevron-left"
          onClick={handlePrevClick}
          className="border-noround-right bg-blue-300 border-blue-300"
        />
        <Calendar
          {...props}
          inputRef={ref}
          value={value}
          onChange={(e: any) => handleDateChange(e.value[0])}
          selectionMode="range"
          readOnlyInput
          disabled={disabled}
          className="custom-calendar w-full"
          dateFormat={dateFormat}
        />
        <Button
          icon="pi pi-chevron-right"
          onClick={handleNextClick}
          className="border-noround-left bg-blue-300 border-blue-300"
        />
      </div>
    );
  }
);

CustomRangeCalendar.displayName = 'CustomRangeCalendar';

export default CustomRangeCalendar;
