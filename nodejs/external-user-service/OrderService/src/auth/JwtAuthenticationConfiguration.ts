/**
 * JWT Authentication Configuration
 * TypeScript equivalent to .NET JwtAuthenticationConfiguration
 */

import { InMemoryJwksKeyCache } from '../cache/InMemoryJwksKeyCache';
import { RedisJwksKeyCache } from '../cache/RedisJwksKeyCache';
import { IJwksKeyCache } from '../cache/IJwksKeyCache';
import { JwtAuthenticationService } from './JwtAuthenticationService';
import { JwksRefreshBackgroundService } from './JwksRefreshBackgroundService';

export class JwtAuthenticationConfiguration {
    private jwtService?: JwtAuthenticationService;
    private backgroundService?: JwksRefreshBackgroundService;
    private jwksCache?: IJwksKeyCache;
    private lastRefresh: Date = new Date(0);

    // Configuration properties
    public readonly authority: string;
    public readonly audience: string;
    public readonly jwksUrl: string;
    public readonly cacheProvider: string;
    public readonly redisConnectionString: string;

    constructor() {
        // Load configuration from environment variables
        this.authority = process.env.JWT_AUTHORITY || 'http://localhost:5000';
        this.audience = process.env.JWT_AUDIENCE || 'shala';
        this.jwksUrl = process.env.JWT_JWKS_URL || `${this.authority}/.well-known/jwks.json`;
        this.cacheProvider = process.env.JWKS_CACHE_PROVIDER || 'local';
        this.redisConnectionString = process.env.REDIS_CONNECTION_STRING || 'redis://localhost:6379';

        console.log('🔐 JWT Authentication Configuration:');
        console.log(`   • Authority: ${this.authority}`);
        console.log(`   • Audience: ${this.audience}`);
        console.log(`   • JWKS URL: ${this.jwksUrl}`);
        console.log(`   • Cache Provider: ${this.cacheProvider}`);
    }

    /**
     * Configure JWKS caching (equivalent to .NET AddJwtAuthenticationAndAuthorization)
     */
    public configureJwksCaching(): IJwksKeyCache {
        // Pluggable cache registration (local or redis)
        if (this.cacheProvider.toLowerCase() === 'redis') {
            try {
                this.jwksCache = new RedisJwksKeyCache(this.redisConnectionString);
                console.log('🧠 JWKS Cache Provider: Redis');
            } catch (error) {
                console.warn('⚠️ Redis cache initialization failed, falling back to in-memory cache:', error);
                this.jwksCache = new InMemoryJwksKeyCache();
                console.log('🧠 JWKS Cache Provider: InMemory (Fallback)');
            }
        } else {
            this.jwksCache = new InMemoryJwksKeyCache();
            console.log('🧠 JWKS Cache Provider: InMemory');
        }

        // Initialize JWT service
        this.jwtService = new JwtAuthenticationService(this.jwksUrl, this.jwksCache);

        // Start cleanup timer for in-memory cache
        if (this.jwksCache instanceof InMemoryJwksKeyCache) {
            this.jwksCache.startCleanupTimer();
        }

        return this.jwksCache;
    }

    /**
     * Get the JWT authentication service instance
     */
    public getJwtService(): JwtAuthenticationService {
        if (!this.jwtService) {
            throw new Error('JWT service not initialized. Call configureJwksCaching() first.');
        }
        return this.jwtService;
    }

    /**
     * Start background services (equivalent to .NET hosted service)
     */
    public async startBackgroundServices(): Promise<void> {
        try {
            if (!this.jwtService) {
                throw new Error('JWT service not initialized. Call configureJwksCaching() first.');
            }

            const refreshIntervalMinutes = parseInt(process.env.JWKS_REFRESH_INTERVAL_MINUTES || '5');
            this.backgroundService = new JwksRefreshBackgroundService(this.jwtService, refreshIntervalMinutes);
            
            await this.backgroundService.startAsync();
            console.log('✅ JWKS background refresh service started');
        } catch (error: any) {
            console.error('❌ Error starting background services:', error.message);
            throw error;
        }
    }

    /**
     * Stop background services
     */
    public async stopBackgroundServices(): Promise<void> {
        try {
            if (this.backgroundService) {
                await this.backgroundService.stopAsync();
                console.log('✅ JWKS background refresh service stopped');
            }

            // Cleanup Redis connection if using Redis cache
            if (this.jwksCache instanceof RedisJwksKeyCache) {
                await this.jwksCache.disconnect();
                console.log('✅ Redis connection closed');
            }
        } catch (error: any) {
            console.error('❌ Error stopping background services:', error.message);
        }
    }

    /**
     * Get cache statistics
     */
    public getCacheStats(): any {
        const stats: any = {
            provider: this.cacheProvider,
            lastRefresh: this.lastRefresh,
            backgroundServiceStatus: this.backgroundService?.getStatus() || { isRunning: false }
        };

        if (this.jwksCache instanceof InMemoryJwksKeyCache) {
            stats.cacheSize = this.jwksCache.getCacheSize();
        }

        return stats;
    }
}

// Global configuration instance (singleton pattern like .NET)
let jwtConfig: JwtAuthenticationConfiguration | undefined;

export function getJwtConfiguration(): JwtAuthenticationConfiguration {
    if (!jwtConfig) {
        jwtConfig = new JwtAuthenticationConfiguration();
    }
    return jwtConfig;
}
