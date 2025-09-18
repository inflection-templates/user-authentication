/**
 * In-Memory JWKS Key Cache
 * TypeScript equivalent to Python in-memory JWKS cache
 */

import { IJwksKeyCache, JwksKey } from './jwks.key.cache.interface';
import { logger } from '../common/logger';

interface CachedKey {
    key: JwksKey;
    expiresAt: Date;
}

export class InMemoryJwksKeyCache implements IJwksKeyCache {
    private cache: Map<string, CachedKey> = new Map();

    async tryGetAsync(kid: string): Promise<JwksKey | null> {
        try {
            const cached = this.cache.get(kid);
            
            if (!cached) {
                logger.debug(`Cache miss for key ID: ${kid}`);
                return null;
            }
            
            // Check if expired
            if (cached.expiresAt <= new Date()) {
                logger.debug(`Cache expired for key ID: ${kid}`);
                this.cache.delete(kid);
                return null;
            }
            
            logger.debug(`Cache hit for key ID: ${kid}`);
            return cached.key;
        } catch (error) {
            logger.error(`Error getting key from in-memory cache: ${error}`);
            return null;
        }
    }

    async setAsync(kid: string, jwk: JwksKey, ttlMinutes: number): Promise<void> {
        try {
            const expiresAt = new Date();
            expiresAt.setMinutes(expiresAt.getMinutes() + ttlMinutes);
            
            this.cache.set(kid, {
                key: jwk,
                expiresAt
            });
            
            logger.debug(`Key cached in-memory for ID: ${kid}, expires at: ${expiresAt.toISOString()}`);
        } catch (error) {
            logger.error(`Error setting key in in-memory cache: ${error}`);
        }
    }

    async removeAsync(kid: string): Promise<void> {
        try {
            const deleted = this.cache.delete(kid);
            if (deleted) {
                logger.debug(`Key removed from in-memory cache: ${kid}`);
            }
        } catch (error) {
            logger.error(`Error removing key from in-memory cache: ${error}`);
        }
    }

    async clearAsync(): Promise<void> {
        try {
            this.cache.clear();
            logger.debug('In-memory cache cleared');
        } catch (error) {
            logger.error(`Error clearing in-memory cache: ${error}`);
        }
    }

    // Cleanup expired keys periodically
    public startCleanupTimer(): void {
        setInterval(() => {
            this.cleanupExpiredKeys();
        }, 5 * 60 * 1000); // Every 5 minutes
    }

    private cleanupExpiredKeys(): void {
        const now = new Date();
        let cleanedCount = 0;
        
        for (const [kid, cached] of this.cache.entries()) {
            if (cached.expiresAt <= now) {
                this.cache.delete(kid);
                cleanedCount++;
            }
        }
        
        if (cleanedCount > 0) {
            logger.debug(`Cleaned up ${cleanedCount} expired keys from in-memory cache`);
        }
    }
}
