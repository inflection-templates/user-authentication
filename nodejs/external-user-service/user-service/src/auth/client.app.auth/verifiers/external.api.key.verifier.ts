import { injectable } from 'tsyringe';
import { CurrentClient } from "../../../domain.types/miscellaneous/current.client";
import { IApiKeyVerifier } from "../interfaces/api.key.verifier.interface";
import { CacheService } from "../../../common/cache/cache.service";
import { logger } from "../../../logger/logger";
import axios from 'axios';
import * as crypto from 'crypto';

////////////////////////////////////////////////////////////////////////////////////////////////

// Interface for external API response
interface ExternalApiKeyResponse {
    IsValid: boolean;
    Client?: {
        ClientCode: string;
        ClientName: string;
        IsPrivileged: boolean;
    };
    ExpiresAt?: string; // ISO date string
    Error?: string;
}

// Interface for cached client data
interface CachedClientData {
    Client   : CurrentClient;
    ExpiresAt: Date;
    CachedAt : Date;
}

////////////////////////////////////////////////////////////////////////////////////////////////

const CACHE_PREFIX = 'api_key_';
const CACHE_TTL_MINUTES = 60; // Cache for 60 minutes by default

////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class ExternalApiKeyVerifier implements IApiKeyVerifier {

    public verify = async (apiKey: string): Promise<CurrentClient | null> => {
        try {
            // Check cache first
            const cachedData = await this.getCachedClientData(apiKey);
            if (cachedData) {
                // Check if cached data is still valid (not expired)
                const now = new Date();
                if (cachedData.ExpiresAt > now) {
                    logger.info(`API key validation served from cache for key: ${apiKey.substring(0, 8)}...`);
                    return cachedData.Client;
                } else {
                    // Remove expired cache entry
                    await this.removeCachedClientData(apiKey);
                    logger.info(`Cached API key expired, removed from cache: ${apiKey.substring(0, 8)}...`);
                }
            }

            // Make external API call
            const externalApiResponse = await this.callExternalApiKeyValidation(apiKey);

            if (!externalApiResponse.IsValid || !externalApiResponse.Client) {
                logger.info(`External API key validation failed: ${externalApiResponse.Error || 'Invalid API key'}`);
                return null;
            }

            // Create CurrentClient object from external API response
            const currentClient: CurrentClient = {
                ClientCode   : externalApiResponse.Client.ClientCode,
                ClientName   : externalApiResponse.Client.ClientName,
                IsPrivileged : externalApiResponse.Client.IsPrivileged
            };

            // Cache the result with expiration
            const expiresAt = externalApiResponse.ExpiresAt
                ? new Date(externalApiResponse.ExpiresAt)
                : new Date(Date.now() + (CACHE_TTL_MINUTES * 60 * 1000)); // Default TTL

            await this.cacheClientData(apiKey, currentClient, expiresAt);

            logger.info(`API key validated externally and cached: ${apiKey.substring(0, 8)}...`);
            return currentClient;

        } catch (error) {
            logger.info(`Error in external API key validation: ${error.message}`);
            return null;
        }
    };

    public clearCache = async (): Promise<void> => {
        try {
            await CacheService.findAndClear([CACHE_PREFIX]);
            logger.info('All cached API key data cleared');
        } catch (error) {
            logger.info(`Error clearing cached API key data: ${error.message}`);
        }
    };

    public getVerifierType = (): string => {
        return 'External';
    };

    /**
     * Call external API to validate API key
     */
    private callExternalApiKeyValidation = async (apiKey: string): Promise<ExternalApiKeyResponse> => {
        try {
            // Get external API endpoint from environment variables
            const externalApiEndpoint = process.env.EXTERNAL_API_KEY_VALIDATION_ENDPOINT;
            // const externalApiKey = process.env.EXTERNAL_API_SERVICE_KEY;

            if (!externalApiEndpoint) {
                throw new Error('External API key validation endpoint not configured');
            }

            const headers = {
                'Content-Type' : 'application/json',
                'Accept'       : 'application/json',
                // 'x-api-key'     : externalApiKey || undefined
            };

            // Remove undefined headers
            Object.keys(headers).forEach(key => headers[key] === undefined && delete headers[key]);

            const requestBody = {
                apiKey : apiKey
            };

            logger.info(`Calling external API for key validation: ${externalApiEndpoint}`);

            const response = await axios.post(externalApiEndpoint, requestBody, {
                headers,
                timeout : 10000 // 10 second timeout
            });

            if (response.status === 200 && response.data) {
                return response.data as ExternalApiKeyResponse;
            } else {
                throw new Error(`External API returned status: ${response.status}`);
            }

        } catch (error) {
            if (axios.isAxiosError(error)) {
                const errorMessage = error.response?.data?.message || error.message;
                logger.info(`External API call failed: ${errorMessage}`);
                return {
                    IsValid : false,
                    Error   : errorMessage
                };
            } else {
                logger.info(`External API call error: ${error.message}`);
                return {
                    IsValid : false,
                    Error   : error.message
                };
            }
        }
    };

    /**
     * Get cached client data for API key
     */
    private getCachedClientData = async (apiKey: string): Promise<CachedClientData | null> => {
        try {
            const cacheKey = this.getCacheKey(apiKey);
            const cachedData = await CacheService.get(cacheKey);

            if (cachedData) {
                return {
                    Client    : cachedData.Client,
                    ExpiresAt : new Date(cachedData.ExpiresAt),
                    CachedAt  : new Date(cachedData.CachedAt)
                };
            }

            return null;
        } catch (error) {
            logger.info(`Error retrieving cached data: ${error.message}`);
            return null;
        }
    };

    /**
     * Cache client data with expiration
     */
    private cacheClientData = async (apiKey: string, client: CurrentClient, expiresAt: Date): Promise<void> => {
        try {
            const cacheKey = this.getCacheKey(apiKey);
            const cacheData: CachedClientData = {
                Client    : client,
                ExpiresAt : expiresAt,
                CachedAt  : new Date()
            };

            await CacheService.set(cacheKey, cacheData);
            logger.info(`Client data cached for key: ${apiKey.substring(0, 8)}... until ${expiresAt.toISOString()}`);
        } catch (error) {
            logger.info(`Error caching client data: ${error.message}`);
        }
    };

    /**
     * Remove cached client data for API key
     */
    private removeCachedClientData = async (apiKey: string): Promise<void> => {
        try {
            const cacheKey = this.getCacheKey(apiKey);
            await CacheService.delete(cacheKey);
        } catch (error) {
            logger.info(`Error removing cached data: ${error.message}`);
        }
    };

    /**
     * Generate cache key for API key
     */
    private getCacheKey = (apiKey: string): string => {
        // Use a hash of the API key for security (don't store the actual key)
        const hash = crypto.createHash('sha256').update(apiKey).digest('hex');
        return `${CACHE_PREFIX}${hash}`;
    };

}
