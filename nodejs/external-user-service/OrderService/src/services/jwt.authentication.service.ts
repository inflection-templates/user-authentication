/**
 * JWT Authentication Service
 * TypeScript equivalent to Python JWT authentication service with caching
 */

import axios from 'axios';
import { IJwksKeyCache, JwksKey } from '../cache/jwks.key.cache.interface';
import { logger } from '../common/logger';

export interface JwksResponse {
    keys: JwksKey[];
}

export class JwtAuthenticationService {
    private jwksCache: IJwksKeyCache;
    private jwksUrl: string;
    private refreshIntervalMinutes: number;
    private defaultTtlMinutes: number = 10;

    constructor(
        jwksCache: IJwksKeyCache,
        jwksUrl: string,
        refreshIntervalMinutes: number = 5
    ) {
        this.jwksCache = jwksCache;
        this.jwksUrl = jwksUrl;
        this.refreshIntervalMinutes = refreshIntervalMinutes;
    }

    async getJwksKeyAsync(kid: string): Promise<JwksKey | null> {
        try {
            // Try to get from cache first
            let jwksKey = await this.jwksCache.tryGetAsync(kid);
            
            if (jwksKey) {
                logger.debug(`JWKS key found in cache for kid: ${kid}`);
                return jwksKey;
            }
            
            // Cache miss - fetch from JWKS endpoint
            logger.debug(`JWKS key not in cache, fetching from endpoint for kid: ${kid}`);
            jwksKey = await this.fetchJwksKeyFromEndpoint(kid);
            
            if (jwksKey) {
                // Cache the key
                await this.jwksCache.setAsync(kid, jwksKey, this.defaultTtlMinutes);
                logger.debug(`JWKS key cached for kid: ${kid}`);
            }
            
            return jwksKey;
        } catch (error) {
            logger.error(`Error getting JWKS key for kid ${kid}: ${error}`);
            return null;
        }
    }

    private async fetchJwksKeyFromEndpoint(kid: string): Promise<JwksKey | null> {
        try {
            logger.debug(`Fetching JWKS from endpoint: ${this.jwksUrl}`);
            
            const response = await axios.get<JwksResponse>(this.jwksUrl, {
                timeout: 10000,
                headers: {
                    'Accept': 'application/json',
                    'User-Agent': 'OrderService-JWT-Client/1.0'
                }
            });
            
            if (response.status !== 200) {
                logger.error(`JWKS endpoint returned status: ${response.status}`);
                return null;
            }
            
            const jwksResponse = response.data;
            
            if (!jwksResponse.keys || !Array.isArray(jwksResponse.keys)) {
                logger.error('Invalid JWKS response format');
                return null;
            }
            
            // Find the key with matching kid
            const key = jwksResponse.keys.find(k => k.kid === kid);
            
            if (!key) {
                logger.warn(`Key with kid ${kid} not found in JWKS response`);
                return null;
            }
            
            // Validate required fields
            if (!key.kty || !key.use || !key.n || !key.e) {
                logger.error(`Invalid key format for kid ${kid}`);
                return null;
            }
            
            logger.debug(`Successfully fetched JWKS key for kid: ${kid}`);
            return key;
            
        } catch (error) {
            if (axios.isAxiosError(error)) {
                logger.error(`HTTP error fetching JWKS: ${error.message}, Status: ${error.response?.status}`);
            } else {
                logger.error(`Error fetching JWKS key from endpoint: ${error}`);
            }
            return null;
        }
    }

    async refreshAllKeysAsync(): Promise<void> {
        try {
            logger.debug('Starting JWKS keys refresh');
            
            const response = await axios.get<JwksResponse>(this.jwksUrl, {
                timeout: 10000,
                headers: {
                    'Accept': 'application/json',
                    'User-Agent': 'OrderService-JWT-Client/1.0'
                }
            });
            
            if (response.status !== 200) {
                logger.error(`JWKS endpoint returned status: ${response.status}`);
                return;
            }
            
            const jwksResponse = response.data;
            
            if (!jwksResponse.keys || !Array.isArray(jwksResponse.keys)) {
                logger.error('Invalid JWKS response format during refresh');
                return;
            }
            
            // Cache all keys
            let cachedCount = 0;
            for (const key of jwksResponse.keys) {
                if (key.kid && key.kty && key.use && key.n && key.e) {
                    await this.jwksCache.setAsync(key.kid, key, this.defaultTtlMinutes);
                    cachedCount++;
                }
            }
            
            logger.info(`JWKS keys refresh completed. Cached ${cachedCount} keys`);
            
        } catch (error) {
            logger.error(`Error during JWKS keys refresh: ${error}`);
        }
    }

    async clearCacheAsync(): Promise<void> {
        try {
            await this.jwksCache.clearAsync();
            logger.info('JWKS cache cleared');
        } catch (error) {
            logger.error(`Error clearing JWKS cache: ${error}`);
        }
    }
}
