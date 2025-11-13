export const getTokens = () => {
    try {
        if (typeof window === 'undefined') return null;

        const accessToken = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');

        return { accessToken, refreshToken };
    } catch (error) {
        return null;
    }
};

export const isAuthenticated = () => {
    try {
        const tokens = getTokens();
        return !!tokens?.accessToken;
    } catch (error) {
        return false;
    }
};
