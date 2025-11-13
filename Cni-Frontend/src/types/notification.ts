export interface Notification {
    id: string;
    title: string;
    message: string;
    deviceId?: string;
    deviceName?: string;
    severity: 'info' | 'warning' | 'error' | 'success';
    status: 'read' | 'unread' | 'archived';
    createdAt: string;
    createdBy?: string;
    updatedAt?: string;
    updatedBy?: string;
}

export interface NotificationFilter {
    severity?: 'info' | 'warning' | 'error' | 'success';
    status?: 'read' | 'unread' | 'archived';
    deviceId?: string;
    fromDate?: string;
    toDate?: string;
}
