import { injectable, inject } from 'tsyringe';
import { CurrentClient } from "../../../domain.types/miscellaneous/current.client";
import { IApiKeyVerifier } from "../interfaces/api.key.verifier.interface";
import { ClientAppService } from "../../../services/client.apps/client.app.service";
import { logger } from "../../../logger/logger";

////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class InternalApiKeyVerifier implements IApiKeyVerifier {

    constructor(
        @inject(ClientAppService) private _clientAppService: ClientAppService
    ) {}

    public verify = async (apiKey: string): Promise<CurrentClient | null> => {
        try {
            logger.info(`Verifying API key internally: ${apiKey.substring(0, 8)}...`);

            const client: CurrentClient = await this._clientAppService.isApiKeyValid(apiKey);
            if (!client) {
                logger.info(`Internal API key validation failed: ${apiKey.substring(0, 8)}...`);
                return null;
            }

            logger.info(`Internal API key validation successful: ${apiKey.substring(0, 8)}...`);
            return client;
        } catch (error) {
            logger.info(`Error in internal API key verification: ${error.message}`);
            return null;
        }
    };

    public getVerifierType = (): string => {
        return 'Internal';
    };

}
