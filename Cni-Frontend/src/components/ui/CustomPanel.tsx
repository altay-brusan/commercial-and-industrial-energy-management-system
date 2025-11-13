'use client';
import React from 'react';

import { Panel } from 'primereact/panel';
import { Ripple } from 'primereact/ripple';

export default function CustomPanel({ children, header, ...props }: { children: React.ReactNode; header: string; props: any }) {
  const template = (options: any) => {
    const toggleIcon = options.collapsed ? 'pi pi-chevron-down' : 'pi pi-chevron-up';
    const className = `${options.className} justify-content-start text-primary`;
    const titleClassName = `${options.titleClassName} ml-2`;
    const style = { fontSize: '1.25rem', color: 'black' };

    return (
      <div className={className}>
        <button type="button" className={options.togglerClassName} onClick={options.onTogglerClick}>
          <span className={toggleIcon} />
          <Ripple />
        </button>
        <span className={titleClassName} style={style}>
          {header}
        </span>
      </div>
    );
  };

  return (
    <Panel {...props} headerTemplate={template} toggleable>
      {children}
    </Panel>
  );
}
