import { tryCatch } from '@/src/utils/tryCatch';
/* eslint-disable func-names */
import axios, {
    type AxiosInstance,
    type AxiosRequestConfig,
    type AxiosResponse,
    type AxiosError,
    type InternalAxiosRequestConfig
} from 'axios';

interface ApiError {
    message: string;
    code: string;
    status: number;
}

class HttpClient {
    private static instance: HttpClient;
    private api: AxiosInstance;
    private isRefreshing = false;
    private requestQueue: Array<{
        resolve: (value: unknown) => void;
        reject: (error: unknown) => void;
        config: AxiosRequestConfig;
    }> = [];

    private constructor() {
        this.api = axios.create({
            baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://derms-api.n2nenergy.com/api',
            timeout: 30000,
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        });

        this.setupInterceptors();
    }

    public static getInstance(): HttpClient {
        if (!HttpClient.instance) {
            HttpClient.instance = new HttpClient();
        }
        return HttpClient.instance;
    }

    private setupInterceptors(): void {
        // Request Interceptor
        this.api.interceptors.request.use(
            this.handleRequest,
            this.handleRequestError
        );

        // Response Interceptor
        this.api.interceptors.response.use(
            this.handleResponse,
            this.handleResponseError
        );
    }

    private handleRequest = (config: InternalAxiosRequestConfig): InternalAxiosRequestConfig => {
        const token = localStorage.getItem('accessToken');

        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        return config;
    };

    private handleRequestError = (error: AxiosError): Promise<AxiosError> => {
        return Promise.reject(error);
    };

    private handleResponse = (response: AxiosResponse): AxiosResponse => {
        return response;
    };

    private handleResponseError = async (error: AxiosError<ApiError>): Promise<unknown> => {
        const status = error.response?.status;
        const originalRequest = error.config;

        if (status === 401 && originalRequest) {
            if (!this.isRefreshing) {
                this.isRefreshing = true;

                try {
                    await this.handleUnauthorized();

                    this.processQueue(null);

                    const newConfig = { ...originalRequest };

                    const token = localStorage.getItem('accessToken');
                    if (token && newConfig.headers) {
                        newConfig.headers.Authorization = `Bearer ${token}`;
                    }

                    return this.api(newConfig);
                } catch (refreshError) {
                    this.processQueue(refreshError as Error);
                    this.clearAuthAndRedirect();
                    return Promise.reject(refreshError);
                } finally {
                    this.isRefreshing = false;
                }
            } else {
                return new Promise((resolve, reject) => {
                    this.requestQueue.push({
                        resolve,
                        reject,
                        config: originalRequest
                    });
                });
            }
        }

        return Promise.reject(error);
    };

    private processQueue(error: Error | null): void {
        this.requestQueue.forEach((request) => {
            if (error) {
                request.reject(error);
            } else {
                const token = localStorage.getItem('accessToken');
                if (token && request.config.headers) {
                    request.config.headers.Authorization = `Bearer ${token}`;
                }
                request.resolve(this.api(request.config));
            }
        });

        this.requestQueue = [];
    }

    public updateToken(token: string): void {
        if (token) {
            this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
            localStorage.setItem('accessToken', token);
        } else {
            delete this.api.defaults.headers.common['Authorization'];
        }
    }

    private async handleUnauthorized(): Promise<void> {
        const refreshToken = localStorage.getItem('refreshToken');

        if (!refreshToken) {
            this.clearAuthAndRedirect();
            throw new Error('No refresh token available');
        }

        const [err, res] = await tryCatch(axios.post((process.env.NEXT_PUBLIC_API_URL || 'https://derms-api.n2nenergy.com/api') + '/Users/refresh', {
            refreshToken: refreshToken
        },
            {
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        ));

        if (err || !res || res.status !== 200) {
            this.clearAuthAndRedirect();
            throw err || new Error('Failed to refresh token');
        }

        const { accessToken, refreshToken: newRefreshToken } = res.data;
        this.updateToken(accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);
    }

    private clearAuthAndRedirect(): void {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.clear();
        this.redirectTo('/auth/login');
    }

    private redirectTo(path: string): void {
        window.location.href = path;
    }

    // Public API methods
    public async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.get<T>(url, config);
        return response.data;
    }

    public async post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.post<T>(url, data, config);
        return response.data;
    }

    public async put<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.put<T>(url, data, config);
        return response.data;
    }

    public async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.delete<T>(url, config);
        return response.data;
    }

    public async patch<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
        const response = await this.api.patch<T>(url, data, config);
        return response.data;
    }
}

export const httpClient = HttpClient.getInstance();
