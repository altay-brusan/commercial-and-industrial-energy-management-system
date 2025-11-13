import { httpClient } from './http';
import { Notification, NotificationFilter } from '../types/notification';
import { PaginatedList } from '../types/common';

const generateMockNotifications = (count: number, status?: string, severity?: string): Notification[] => {
    const deviceNames = ['Solar Panel A1', 'Inverter B22', 'Battery Storage C3', 'Grid Connection', 'Generator X5', 'Transformator T1'];
    const titles = [
        'Device connection error',
        'High temperature warning',
        'Capacity decrease',
        'Maintenance time',
        'High voltage warning',
        'Low efficiency',
        'System rebooted',
        'Energy production stopped',
        'Grid connection lost',
        'Battery charge level low'
    ];
    const severities = ['info', 'warning', 'error', 'success'];
    const statuses = ['read', 'unread', 'archived'];
    const messages = [
        'Device connection was temporarily lost. Automatic reconnection is being attempted.',
        'Device temperature has reached critical level. Please check the cooling system.',
        'Energy production capacity has decreased. Maintenance may be required.',
        'Routine maintenance time has arrived. Please inform the maintenance team.',
        'High voltage has been detected. System has entered protection mode.',
        'Efficiency rate has decreased by 20% in the last 24 hours. Review is recommended.',
        'System has been automatically rebooted. No action is required.',
        'Energy production has unexpectedly stopped. Urgent review is required.',
        'Grid connection has been lost. Backup power system has been activated.',
        'Battery charge level has decreased to 10%. Energy saving mode has been activated.'
    ];

    const result: Notification[] = [];

    for (let i = 0; i < count; i++) {
        const currentSeverity = severity || severities[Math.floor(Math.random() * severities.length)];
        const currentStatus = status || statuses[Math.floor(Math.random() * statuses.length)];
        const titleIndex = Math.floor(Math.random() * titles.length);
        const deviceIndex = Math.floor(Math.random() * deviceNames.length);

        const date = new Date();
        date.setDate(date.getDate() - Math.floor(Math.random() * 30));
        date.setHours(date.getHours() - Math.floor(Math.random() * 24));

        result.push({
            id: `not-${i+1}`,
            title: titles[titleIndex],
            message: messages[titleIndex],
            severity: currentSeverity as any,
            status: currentStatus as any,
            deviceId: `device-${deviceIndex+1}`,
            deviceName: deviceNames[deviceIndex],
            createdAt: date.toISOString(),
            createdBy: 'system'
        });
    }

    return result;
};

const applyFilters = (notifications: Notification[], filter?: NotificationFilter): Notification[] => {
    if (!filter) return notifications;

    return notifications.filter(notification => {
        if (filter.severity && notification.severity !== filter.severity) return false;
        if (filter.status && notification.status !== filter.status) return false;
        if (filter.deviceId && notification.deviceId !== filter.deviceId) return false;

        if (filter.fromDate) {
            const fromDate = new Date(filter.fromDate);
            const notificationDate = new Date(notification.createdAt);
            if (notificationDate < fromDate) return false;
        }

        if (filter.toDate) {
            const toDate = new Date(filter.toDate);
            const notificationDate = new Date(notification.createdAt);
            if (notificationDate > toDate) return false;
        }

        return true;
    });
};

export const notificationService = {
    list: async (params: { pageNumber: number; pageSize: number; filter?: NotificationFilter }): Promise<PaginatedList<Notification>> => {
        const { pageNumber, pageSize, filter } = params;

        const allNotifications = generateMockNotifications(50);
        const filteredNotifications = applyFilters(allNotifications, filter);

        const startIndex = (pageNumber - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedItems = filteredNotifications.slice(startIndex, endIndex);

        return {
            items: paginatedItems,
            pageNumber,
            totalPages: Math.ceil(filteredNotifications.length / pageSize),
            totalCount: filteredNotifications.length,
            hasPreviousPage: pageNumber > 1,
            hasNextPage: endIndex < filteredNotifications.length
        };
    },

    getById: async (id: string): Promise<Notification> => {
        const mockNotifications = generateMockNotifications(50);
        const notification = mockNotifications.find(n => n.id === id);

        if (!notification) {
            throw new Error('Notification not found');
        }

        return notification;
    },

    updateStatus: async (id: string, status: 'read' | 'unread' | 'archived'): Promise<void> => {
        return new Promise((resolve) => {
            setTimeout(() => {
                console.log(`Notification ${id} status updated to ${status}`);
                resolve();
            }, 500);
        });
    },

    markAllAsRead: async (): Promise<void> => {
        return new Promise((resolve) => {
            setTimeout(() => {
                console.log('All notifications marked as read');
                resolve();
            }, 500);
        });
    },

    deleteNotification: async (id: string): Promise<void> => {
        return new Promise((resolve) => {
            setTimeout(() => {
                console.log(`Notification ${id} deleted`);
                resolve();
            }, 500);
        });
    }
};
