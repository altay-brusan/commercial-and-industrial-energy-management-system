'use client';
import React from 'react';

import classNames from 'classnames';
import { Checkbox } from 'primereact/checkbox';
import { Editor } from 'primereact/editor';
import { RadioButton } from 'primereact/radiobutton';
import { SelectButton } from 'primereact/selectbutton';
import { Tooltip } from 'primereact/tooltip';
import { Controller } from 'react-hook-form';
import { Mode } from '@/src/utils/mode';
import CustomAutoComplete from './CustomAutoComplete';
import CustomCalendar from './CustomCalendar';
import CustomInputNumber from './CustomInputNumber';
import CustomMultiSelect from './CustomMultiSelect';
import CustomPasswordInput from './CustomPasswordInput';
import CustomRangeCalendar from './CustomRangeCalendar';
import Switch from './CustomSwitch';
import CustomToggleInputGroup from './CustomToggleInputGroup';
import CustomDropdown from './Dropdown';
import Input from './Input';
import Textarea from './Textarea';
import CustomFileUpload from './file-upload';

function SwitchControlField({ item, keyProp, name, control, mode, isColumn, col, errors }: any) {
  const fieldClassName = classNames(
    'flex p-0 align-items-center',
    { 'w-full': !item.fieldCol },
    { [`col-12 md:col-${item.fieldCol}`]: item.fieldCol }
  );

  const containerClassName = classNames({
    [`field mb-4 col-12 md:col-${item.col || col}`]: !isColumn,
    flex: item.component === 'checkbox'
  });

  return (
    item.hide ? null : (
      <Controller
        control={control}
        name={name || item.name}
        key={keyProp}
        render={({ field }) => {
          switch (item.component) {
            case 'input':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <Input
                      {...item}
                      name={field.name}
                      id={field.name}
                      value={field.value}
                      onChange={item.onChange || field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                      errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'calendar':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomCalendar
                      {...item}
                      inputId={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                       errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'textarea':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <Textarea
                      {...item}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      tooltipOptions="top"
                      className="w-full"
                      containerClassName="w-full"
                      errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'dropdown':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomDropdown
                      {...item}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'multiselect':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomMultiSelect
                      {...item}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'password':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomPasswordInput
                      {...item}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                      minWidth="auto"
                    />
                  </div>
                </div>
              );
            case 'checkbox':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <Checkbox
                      {...item}
                      name={field.name}
                      inputId={field.name}
                      onChange={field.onChange}
                      checked={field.value}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      tooltipOptions="top"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                    />
                    <label htmlFor={field.name} className="ml-2">
                      {item.label}
                    </label>
                  </div>
                </div>
              );
            case 'editor':
              return (
                <div className={containerClassName} key={keyProp}>
                  <Editor
                    {...item}
                    id={field.name}
                    name={field.name}
                    value={field.value}
                    onTextChange={(e) => field.onChange(e.htmlValue)}
                    style={{ height: '260px' }}
                    readOnly={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                    className={classNames(item.className, 'w-full', { 'p-invalid': item.errorMsg })}
                  />
                  {item.errorMsg && <small className="p-error">{item.errorMsg}</small>}
                </div>
              );
            case 'switch':
              return (
                <div className={`flex align-items-center ${containerClassName}`} key={keyProp}>
                  <Switch
                    {...item}
                    name={field.name}
                    id={field.name}
                    value={field.value}
                    onChange={field.onChange}
                    disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                  />
                </div>
              );
            case 'inputNumber':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomInputNumber
                      {...item}
                      name={field.name}
                      id={field.name}
                      value={field.value}
                      onChange={item.onChange || field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      tooltipOptions="top"
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                    />
                  </div>
                </div>
              );
            case 'separator':
              return (
                <div
                  className={classNames({
                    [`field mb-4 col-12 md:col-${item.col || 12}`]: !isColumn,
                    'flex align-items-center gap-2': item?.tooltip
                  })}
                  key={keyProp}>
                  {typeof item?.content === 'function' ? item?.content() : <div dangerouslySetInnerHTML={{ __html: item?.content }} className={item?.contentClassName} />}
                  {item?.tooltip && (
                    <>
                      <Tooltip target={`.${item?.target}`} />
                      <i
                        className={classNames(item?.icon, item?.target)}
                        data-pr-position={item?.position}
                        data-pr-tooltip={item?.tooltip}
                      />
                    </>
                  )}
                </div>
              );
            case 'radio-group':
              const radioButtonClassName = classNames('flex gap-3', item?.radioClassName);
              const radioButtonWrapperClassName = classNames(
                'flex align-items-start gap-2',
                { 'flex-column': !item?.radioWrapperClassName?.split(' ')?.includes('flex-row') },
                item?.radioWrapperClassName
              );
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={classNames(fieldClassName)}>
                    <div className={radioButtonWrapperClassName}>
                      {item.label && <div>{item.label}</div>}
                      <div className="flex justify-content-center align-content-center">
                        <div className={radioButtonClassName}>
                          {item?.options?.map((radio: any, idx: any) => {
                            return (
                              <div key={idx} className="flex align-content-center gap-2">
                                <RadioButton
                                  {...item}
                                  name={field.name}
                                  inputId={field.name + idx}
                                  onChange={field.onChange}
                                  value={radio.value}
                                  checked={field.value === radio.value}
                                  disabled={radio?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                                     errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                                />
                                <label className="line-height-3" htmlFor={field.name + idx}>
                                  {radio.label}
                                </label>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                      {item?.errorMsg && <small className="p-error">{item?.errorMsg}</small>}
                    </div>
                  </div>
                </div>
              );
            case 'auto-complete':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomAutoComplete
                      {...item}
                      name={field.name}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                    />
                  </div>
                </div>
              );
            case 'toggle-input':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomToggleInputGroup
                      {...item}
                      name={field.name}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      operatorValue={control._formValues[item.operatorName] === '-'}
                      operatorOnChange={control._reset}
                      percentageOrCurrencyValue={control._formValues[item.percentageOrCurrencyName]}
                      percentageOrCurrencyOnChange={control._reset}
                      formValues={control._formValues}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                    />
                  </div>
                </div>
              );
            case 'range-calendar':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomRangeCalendar
                      {...item}
                      name={field.name}
                      id={field.name}
                      value={field.value}
                      onChange={field.onChange}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                    />
                  </div>
                </div>
              );
            case 'select-button':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <SelectButton {...item} value={field.value} onChange={field.onChange} />
                  </div>
                </div>
              );
            case 'file':
              return (
                <div className={containerClassName} key={keyProp}>
                  <div className={fieldClassName}>
                    <CustomFileUpload
                      {...item}
                      name={field.name}
                      value={field.value}
                      onUpload={(e) => {
                        field.onChange(e.files[0]);
                      }}
                      disabled={item?.disabled || mode === Mode.DETAIL || mode === 'detail'}
                      className="w-full"
                      containerClassName="w-full"
                         errorMsg={item.errorMsg || errors[field.name]?.message ? item.label + " " + errors[field.name]?.message : null}
                    />
                  </div>
                </div>
              );
            default:
              return <div />;
          }
        }}
      />
    )
  );
}

export default SwitchControlField;
