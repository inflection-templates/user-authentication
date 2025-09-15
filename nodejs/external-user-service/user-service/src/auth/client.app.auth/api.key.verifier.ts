import { CurrentClient } from "../../domain.types/miscellaneous/current.client";
import { logger } from '../../logger/logger';
import { Injector } from '../../startup/injector';
import { IApiKeyVerifier } from './interfaces/api.key.verifier.interface';

////////////////////////////////////////////////////////////////////////////////////////////////

export class ApiKeyVerifier {

    public static verify = async (apiKey: string): Promise<CurrentClient> => {
        try {
            // Resolve the appropriate verifier based on configuration
            const verifier = Injector.Container.resolve<IApiKeyVerifier>('IApiKeyVerifier');

            logger.info(`Using ${verifier.getVerifierType()} API key verifier for key: ${apiKey.substring(0, 8)}...`);

            const client = await verifier.verify(apiKey);

            if (client) {
                logger.info(`API key authentication successful using ${verifier.getVerifierType()} verifier`);
            } else {
                logger.info(`API key authentication failed using ${verifier.getVerifierType()} verifier`);
            }

            return client;
        } catch (err) {
            logger.info(`API key authentication error: ${JSON.stringify(err, null, 2)}`);
            return null;
        }
    };

    /**
     * Clear cached API key data (if the verifier supports caching)
     */
    public static clearCache = async (): Promise<void> => {
        try {
            const verifier = Injector.Container.resolve<IApiKeyVerifier>('IApiKeyVerifier');

            if (verifier.clearCache) {
                await verifier.clearCache();
                logger.info(`Cache cleared for ${verifier.getVerifierType()} verifier`);
            } else {
                logger.info(`${verifier.getVerifierType()} verifier does not support caching`);
            }
        } catch (error) {
            logger.info(`Error clearing cache: ${error.message}`);
        }
    };

    /**
     * Get the current verifier type for debugging/monitoring purposes
     */
    public static getVerifierType = (): string => {
        try {
            const verifier = Injector.Container.resolve<IApiKeyVerifier>('IApiKeyVerifier');
            return verifier.getVerifierType();
        } catch (error) {
            logger.info(`Error getting verifier type: ${error.message}`);
            return 'Unknown';
        }
    };

}
