/**
 * In-Memory JWKS Key Cache
 * TypeScript equivalent to .NET InMemoryJwksKeyCache
 */

import { IJwksKeyCache, JwksKey, CachedSecurityKey } from './IJwksKeyCache';

interface CacheEntry {
    jwk: JwksKey;
    expiresAt: Date;
}

export class InMemoryJwksKeyCache implements IJwksKeyCache {
    private readonly cache: Map<string, CacheEntry> = new Map();

    async tryGetAsync(kid: string): Promise<CachedSecurityKey | null> {
        const entry = this.cache.get(kid);
        
        if (!entry) {
            return null;
        }

        const now = new Date();
        if (now > entry.expiresAt) {
            // Entry expired, remove it
            this.cache.delete(kid);
            return null;
        }

        return {
            jwksKey: entry.jwk,
            cachedAt: new Date(entry.expiresAt.getTime() - (10 * 60 * 1000)), // Assuming 10 min TTL
            expiresAt: entry.expiresAt,
            isExpired: false
        };
    }

    async setAsync(kid: string, jwk: JwksKey, ttlMs: number): Promise<void> {
        const expiresAt = new Date(Date.now() + ttlMs);
        
        this.cache.set(kid, {
            jwk,
            expiresAt
        });
    }

    async removeAsync(kid: string): Promise<void> {
        this.cache.delete(kid);
    }

    // Additional utility methods
    public getCacheSize(): number {
        return this.cache.size;
    }

    public cleanupExpired(): number {
        const now = new Date();
        let cleanedCount = 0;

        for (const [kid, entry] of this.cache.entries()) {
            if (now > entry.expiresAt) {
                this.cache.delete(kid);
                cleanedCount++;
            }
        }

        return cleanedCount;
    }

    // Start periodic cleanup
    public startCleanupTimer(): void {
        setInterval(() => {
            const cleaned = this.cleanupExpired();
            if (cleaned > 0) {
                console.log(`Cleaned up ${cleaned} expired JWKS keys from in-memory cache`);
            }
        }, 5 * 60 * 1000); // Every 5 minutes
    }
}
