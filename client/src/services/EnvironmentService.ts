declare var process: {
    env: {
        NODE_ENV: string;
    };
};

export default class EnvironmentService {

    /**
     * @returns Whether we are in development.
     */
    public static get isDevelopment(): boolean {
        return process.env.NODE_ENV === 'development';
    }

    /**
     * @returns Whether we are in production.
     */
    public static get isProduction(): boolean {
        return process.env.NODE_ENV === 'production';
    }

    /**
     * @returns The current environment string.
     */
    public static get getEnv(): string {
        return process.env.NODE_ENV;
    }
}
