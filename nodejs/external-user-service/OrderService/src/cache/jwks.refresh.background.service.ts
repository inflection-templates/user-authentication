/**
 * JWKS Refresh Background Service
 * TypeScript equivalent to Python JWKS refresh background service
 */

import { JwtAuthenticationService } from '../services/jwt.authentication.service';
import { logger } from '../common/logger';

export class JwksRefreshBackgroundService {
    private jwtService: JwtAuthenticationService;
    private refreshIntervalMinutes: number;
    private intervalId?: NodeJS.Timeout;
    private isRunning: boolean = false;

    constructor(
        jwtService: JwtAuthenticationService,
        refreshIntervalMinutes: number = 5
    ) {
        this.jwtService = jwtService;
        this.refreshIntervalMinutes = refreshIntervalMinutes;
    }

    async startAsync(): Promise<void> {
        if (this.isRunning) {
            logger.warn('JWKS refresh background service is already running');
            return;
        }

        try {
            logger.info(`Starting JWKS refresh background service with ${this.refreshIntervalMinutes} minute interval`);
            
            // Perform initial refresh
            await this.performRefresh();
            
            // Set up recurring refresh
            const intervalMs = this.refreshIntervalMinutes * 60 * 1000;
            this.intervalId = setInterval(async () => {
                await this.performRefresh();
            }, intervalMs);
            
            this.isRunning = true;
            logger.info('JWKS refresh background service started successfully');
            
        } catch (error) {
            logger.error(`Error starting JWKS refresh background service: ${error}`);
            throw error;
        }
    }

    async stopAsync(): Promise<void> {
        if (!this.isRunning) {
            logger.warn('JWKS refresh background service is not running');
            return;
        }

        try {
            logger.info('Stopping JWKS refresh background service');
            
            if (this.intervalId) {
                clearInterval(this.intervalId);
                this.intervalId = undefined;
            }
            
            this.isRunning = false;
            logger.info('JWKS refresh background service stopped successfully');
            
        } catch (error) {
            logger.error(`Error stopping JWKS refresh background service: ${error}`);
            throw error;
        }
    }

    private async performRefresh(): Promise<void> {
        try {
            logger.debug('Performing JWKS keys refresh');
            await this.jwtService.refreshAllKeysAsync();
        } catch (error) {
            logger.error(`Error during background JWKS refresh: ${error}`);
            // Don't throw - we want the background service to continue running
        }
    }

    public get isServiceRunning(): boolean {
        return this.isRunning;
    }

    public get currentRefreshInterval(): number {
        return this.refreshIntervalMinutes;
    }
}
