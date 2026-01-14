/**
 * JWT Authentication Configuration
 * TypeScript equivalent to Python JWT configuration with caching support
 */

import { logger } from '../common/logger';
import { JwtAuthenticationService } from '../services/jwt.authentication.service';
import { InMemoryJwksKeyCache } from '../cache/in.memory.jwks.cache';
import { RedisJwksKeyCache } from '../cache/redis.jwks.cache';
import { JwksRefreshBackgroundService } from '../cache/jwks.refresh.background.service';

export enum CacheType {
    IN_MEMORY = 'in_memory',
    REDIS = 'redis'
}

export class JwtAuthenticationConfiguration {
    private _jwtService?: JwtAuthenticationService;
    private _backgroundService?: JwksRefreshBackgroundService;
    
    public readonly jwtAuthority: string;
    public readonly jwtAudience: string;
    public readonly jwksUrl: string;
    public readonly cacheType: string;
    public readonly redisUrl: string;
    public readonly refreshIntervalMinutes: number;
    public readonly enableBackgroundRefresh: boolean;

    constructor() {
        this.jwtAuthority = process.env.JWT_AUTHORITY || 'http://localhost:5000';
        this.jwtAudience = process.env.JWT_AUDIENCE || 'shala';
        this.jwksUrl = process.env.JWT_JWKS_URL || `${this.jwtAuthority}/.well-known/jwks.json`;
        this.cacheType = process.env.JWKS_CACHE_TYPE || CacheType.IN_MEMORY;
        this.redisUrl = process.env.REDIS_URL || 'redis://localhost:6379';
        this.refreshIntervalMinutes = parseInt(process.env.JWKS_REFRESH_INTERVAL_MINUTES || '5');
        this.enableBackgroundRefresh = (process.env.JWKS_ENABLE_BACKGROUND_REFRESH || 'true').toLowerCase() === 'true';
    }

    public getJwtService(): JwtAuthenticationService {
        if (!this._jwtService) {
            this._jwtService = this.createJwtService();
        }
        return this._jwtService;
    }

    private createJwtService(): JwtAuthenticationService {
        try {
            let cache;
            
            if (this.cacheType === CacheType.REDIS) {
                logger.info('Using Redis JWKS cache');
                try {
                    cache = new RedisJwksKeyCache(this.redisUrl);
                } catch (error) {
                    logger.warn(`Redis not available, falling back to in-memory cache: ${error}`);
                    cache = new InMemoryJwksKeyCache();
                }
            } else {
                logger.info('Using in-memory JWKS cache');
                cache = new InMemoryJwksKeyCache();
            }

            const jwtService = new JwtAuthenticationService(
                cache,
                this.jwksUrl,
                this.refreshIntervalMinutes
            );

            logger.info(`JWT authentication service configured with ${this.cacheType} cache`);
            return jwtService;
        } catch (error) {
            logger.error(`Error creating JWT authentication service: ${error}`);
            // Fallback to in-memory cache
            const cache = new InMemoryJwksKeyCache();
            return new JwtAuthenticationService(
                cache,
                this.jwksUrl,
                this.refreshIntervalMinutes
            );
        }
    }

    public async startBackgroundServices(): Promise<void> {
        if (this.enableBackgroundRefresh) {
            const jwtService = this.getJwtService();
            this._backgroundService = new JwksRefreshBackgroundService(
                jwtService,
                this.refreshIntervalMinutes
            );
            await this._backgroundService.startAsync();
            logger.info('JWKS background refresh service started');
        }
    }

    public async stopBackgroundServices(): Promise<void> {
        if (this._backgroundService) {
            await this._backgroundService.stopAsync();
            logger.info('JWKS background refresh service stopped');
        }
    }
}

// Global configuration instance
let jwtConfig: JwtAuthenticationConfiguration | undefined;

export function getJwtConfiguration(): JwtAuthenticationConfiguration {
    if (!jwtConfig) {
        jwtConfig = new JwtAuthenticationConfiguration();
    }
    return jwtConfig;
}
