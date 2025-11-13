import React from 'react';

interface CardProps {
    title?: string;
    children: React.ReactNode;
    className?: string;
    contentClassName?: string;
    footer?: React.ReactNode;
    loading?: boolean;
    headerActions?: React.ReactNode;
    customHeader?: React.ReactNode;
}

const Card: React.FC<CardProps> = ({
    title,
    children,
    className = '',
    contentClassName = '',
    footer,
    loading,
    headerActions,
    customHeader
}) => {
    return (
        <div className={`h-full bg-white dark:bg-gray-800 border-round shadow-2 shadow-md rounded-t-large w-full ${className} p-card`}>
            {title && (
                <>
                    <div className="px-4 py-4 border-b border-gray-200 flex justify-content-between md:align-items-center flex-column md:flex-row gap-2">
                        <h5 className="font-bold m-0" style={{ fontSize: '22px' }}>
                            {title}
                        </h5>
                        {customHeader && (
                            <div className="flex items-center gap-2">
                                {customHeader}
                            </div>
                        )}
                        {headerActions && (
                            <div className="flex items-center gap-2">
                                {headerActions}
                            </div>
                        )}
                    </div>
                    <hr className='m-0' />
                </>
            )}
            <div className={contentClassName} style={{ height: 'calc(100% - 65px)' }}>
                {loading ? (
                    <div className="flex justify-center items-center h-32">
                        <i className="pi pi-spin pi-spinner text-2xl" />
                    </div>
                ) : children}
            </div>
            {footer && (
                <div className="px-6 py-4 border-t border-gray-200">
                    {footer}
                </div>
            )}
        </div>
    );
};

export default Card;
