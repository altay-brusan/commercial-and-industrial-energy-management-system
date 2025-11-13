import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

class WebSocketService {
    private connection: HubConnection | null = null;
    private subscribers: Map<string, ((data: any) => void)[]> = new Map();
    private reconnectAttempts = 0;
    private maxReconnectAttempts = 5;

    public async connect(url: string): Promise<void> {
        if (this.connection?.state === HubConnectionState.Connected) {
            return;
        }

        this.connection = new HubConnectionBuilder()
            .withUrl(url, {
                withCredentials: true,
                skipNegotiation: true,
                transport: HttpTransportType.WebSockets,
                headers: {
                    'Access-Control-Allow-Origin': window.location.origin
                }
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(LogLevel.Information)
            .build();

        this.connection.onclose(async (error) => {
            console.log('Bağlantı kapandı:', error);
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                await this.reconnect();
            }
        });

        this.connection.onreconnecting((error) => {
            console.log('Yeniden bağlanmaya çalışılıyor:', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('Yeniden bağlantı başarılı. ConnectionId:', connectionId);
            this.reconnectAttempts = 0;
        });

        try {
            await this.connection.start();
            this.reconnectAttempts = 0;
            console.log('SignalR bağlantısı başarılı');
        } catch (error) {
            console.error('SignalR bağlantı hatası:', error);
            throw error;
        }
    }

    public async disconnect(): Promise<void> {
        if (this.connection) {
            try {
                await this.connection.stop();
                this.subscribers.clear();
                this.connection = null;
                this.reconnectAttempts = 0;
            } catch (error) {
                console.error('Bağlantı kapatma hatası:', error);
            }
        }
    }

    public isConnected(): boolean {
        return this.connection?.state === HubConnectionState.Connected;
    }

    public async subscribe<T>(methodName: string, callback: (data: T) => void): Promise<() => void> {
        if (!this.connection) {
            throw new Error('SignalR bağlantısı yok');
        }

        if (!this.subscribers.has(methodName)) {
            this.subscribers.set(methodName, []);
            this.connection.on(methodName, (data: T) => {
                const callbacks = this.subscribers.get(methodName);
                if (callbacks) {
                    callbacks.forEach(cb => cb(data));
                }
            });
        }

        const callbacks = this.subscribers.get(methodName);
        if (callbacks) {
            callbacks.push(callback);
        }

        return () => {
            const callbacks = this.subscribers.get(methodName);
            if (callbacks) {
                const index = callbacks.indexOf(callback);
                if (index !== -1) {
                    callbacks.splice(index, 1);
                }
            }
        };
    }

    public async invoke<T>(methodName: string, ...args: any[]): Promise<T> {
        if (!this.connection) {
            throw new Error('SignalR bağlantısı yok');
        }

        return await this.connection.invoke(methodName, ...args);
    }

    private async reconnect(): Promise<void> {
        if (this.connection) {
            try {
                await this.connection.start();
                console.log('Yeniden bağlantı başarılı');
            } catch (error) {
                console.error('Yeniden bağlantı hatası:', error);
            }
        }
    }
}

const webSocketService = new WebSocketService();
export default webSocketService;
