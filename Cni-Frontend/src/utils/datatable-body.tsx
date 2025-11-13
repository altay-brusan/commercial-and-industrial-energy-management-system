import { Button } from "primereact/button";
import { InputText } from "primereact/inputtext";
import { Tag } from "primereact/tag";

const imageBodyTemplate = (rowData:any) => {
    return (
        <>{rowData.logoUrl?.startsWith('http://') && <img src={rowData.logoUrl} alt={rowData.image} width="100" />}</>
    );
};

const actionBodyTemplate = (rowData: any, editClick: any, deleteClick: any) => {
    return (
        <>
            <Button icon="pi pi-pencil" rounded severity="success" className="mr-2" onClick={() => editClick(rowData)} />
            <Button icon="pi pi-trash" rounded severity="warning" onClick={() => deleteClick(rowData)} />
        </>
    );
};

const statusBodyTemplate = (rowData: any) => {
    return (
        <>
            <span className="p-column-title">Status</span>
            <Tag value={rowData.status} severity={rowData.status === 'Active' ? 'success' : 'danger'} />
        </>
    );
};

const header = (t:any, setGlobalFilter:any) => (
    <div className="flex flex-column md:flex-row md:justify-content-between md:align-items-center">
        <Button icon="pi pi-file-excel"  label={t('common.export')} className="p-button-outlined p-button-secondary" />
        <span className="block mt-2 md:mt-0 p-input-icon-left">
            <i className="pi pi-search" />
            <InputText className="w-full md:w-auto" type="search" onInput={(e: any) => setGlobalFilter(e.currentTarget.value)} placeholder="Search..." />
        </span>
    </div>
);

export { imageBodyTemplate, actionBodyTemplate, statusBodyTemplate, header };
