import React from 'react';
import { DataTable, DataTableProps } from 'primereact/datatable';
import { Column } from 'primereact/column';
import { useTranslations } from 'next-intl';
import { Paginator } from 'primereact/paginator';

interface CustomDataTableProps {
    columns: any[];
    data: any[];
    renderHeader?: any;
    globalFilter?: string;
    onGlobalFilterChange?: (value: string) => void;
    totalRecords?: number;
    onPageChange?: (e: { first: number, page: number, rows: number }) => void;
    rows?: number;
    first?: number;
}

const CustomDataTable: React.FC<CustomDataTableProps> = ({ columns, data, globalFilter, renderHeader, totalRecords, onPageChange, rows, first, ...props }) => {
    const t = useTranslations();

    return (
        <>
            <DataTable
                value={data}
                header={renderHeader}
                rows={10}
                globalFilter={globalFilter}
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                rowsPerPageOptions={[10, 25, 50]}
                emptyMessage={t('common.noData')}
                rowHover
                totalRecords={50}
                currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
                className='custom-data-table h-full'>
                {columns.map((col, i) => (
                    <Column key={col.field} {...col} />
                ))}
            </DataTable>
            <Paginator first={first} rows={rows} totalRecords={totalRecords} rowsPerPageOptions={[10, 20, 30, 50, 100]} onPageChange={onPageChange} />
        </>
    );
}

export default CustomDataTable;
