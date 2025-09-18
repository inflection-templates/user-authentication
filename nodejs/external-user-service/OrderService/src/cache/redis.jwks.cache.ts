/**
 * Redis JWKS Key Cache
 * TypeScript equivalent to Python Redis JWKS cache
 */

import Redis from 'redis';
import { IJwksKeyCache, JwksKey } from './jwks.key.cache.interface';
import { logger } from '../common/logger';

export class RedisJwksKeyCache implements IJwksKeyCache {
    private client: Redis.RedisClientType;
    private keyPrefix = 'jwks:';

    constructor(redisUrl: string) {
        this.client = Redis.createClient({ url: redisUrl });
        
        this.client.on('error', (error) => {
            logger.error(`Redis client error: ${error}`);
        });
        
        this.client.on('connect', () => {
            logger.info('Connected to Redis server');
        });
        
        this.client.on('disconnect', () => {
            logger.warn('Disconnected from Redis server');
        });
    }

    async connect(): Promise<void> {
        try {
            await this.client.connect();
        } catch (error) {
            logger.error(`Failed to connect to Redis: ${error}`);
            throw error;
        }
    }

    async disconnect(): Promise<void> {
        try {
            await this.client.disconnect();
        } catch (error) {
            logger.error(`Failed to disconnect from Redis: ${error}`);
        }
    }

    async tryGetAsync(kid: string): Promise<JwksKey | null> {
        try {
            if (!this.client.isOpen) {
                await this.connect();
            }

            const key = this.keyPrefix + kid;
            const cached = await this.client.get(key);
            
            if (!cached) {
                logger.debug(`Redis cache miss for key ID: ${kid}`);
                return null;
            }
            
            const jwk = JSON.parse(cached) as JwksKey;
            logger.debug(`Redis cache hit for key ID: ${kid}`);
            return jwk;
        } catch (error) {
            logger.error(`Error getting key from Redis cache: ${error}`);
            return null;
        }
    }

    async setAsync(kid: string, jwk: JwksKey, ttlMinutes: number): Promise<void> {
        try {
            if (!this.client.isOpen) {
                await this.connect();
            }

            const key = this.keyPrefix + kid;
            const value = JSON.stringify(jwk);
            const ttlSeconds = ttlMinutes * 60;
            
            await this.client.setEx(key, ttlSeconds, value);
            
            logger.debug(`Key cached in Redis for ID: ${kid}, TTL: ${ttlMinutes} minutes`);
        } catch (error) {
            logger.error(`Error setting key in Redis cache: ${error}`);
        }
    }

    async removeAsync(kid: string): Promise<void> {
        try {
            if (!this.client.isOpen) {
                await this.connect();
            }

            const key = this.keyPrefix + kid;
            const deleted = await this.client.del(key);
            
            if (deleted > 0) {
                logger.debug(`Key removed from Redis cache: ${kid}`);
            }
        } catch (error) {
            logger.error(`Error removing key from Redis cache: ${error}`);
        }
    }

    async clearAsync(): Promise<void> {
        try {
            if (!this.client.isOpen) {
                await this.connect();
            }

            const keys = await this.client.keys(this.keyPrefix + '*');
            
            if (keys.length > 0) {
                await this.client.del(keys);
                logger.debug(`Cleared ${keys.length} keys from Redis cache`);
            }
        } catch (error) {
            logger.error(`Error clearing Redis cache: ${error}`);
        }
    }
}
