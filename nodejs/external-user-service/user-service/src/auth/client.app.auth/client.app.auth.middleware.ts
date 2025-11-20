import express from "express";
import { ResponseHandler } from "../../common/handlers/response.handler";
import { logger } from "../../logger/logger";
import { ApiKeyVerifier } from "./api.key.verifier";
import { CurrentClient } from "../../domain.types/miscellaneous/current.client";
import { ConfigurationManager } from "../../config/configuration.manager";

/////////////////////////////////////////////////////////////////////////////

export default class ClientAppAuthMiddleware
{

    public static authenticateClient = async (
        request: express.Request,
        response: express.Response,
        next: express.NextFunction
    ): Promise<void> => {
        try {
            const requestUrl = request.originalUrl;
            const clientAppAuth = ConfigurationManager.ClientAppAuth;

            // If client app authentication is disabled, skip authentication
            if (clientAppAuth === false) {
                next();
                return;
            }

            // Handle certain endpoints separately
            const isHealthCheck = requestUrl === '/api/v1' && request.method === 'GET';

            //Add any other routes here that are not protected by the client app auth middleware.
            //For example, public file downloads, etc.

            const apiKey: string = request.headers['x-api-key'] as string;
            const apiKeyMissing = !apiKey || apiKey.trim() === '';

            if (isHealthCheck) {
                next();
            }
            else if (apiKeyMissing) {
                const message = 'Missing API key';
                ResponseHandler.failure(request, response, message, 401);
                return;
            }
            else {
                const currClient: CurrentClient = await ApiKeyVerifier.verify(apiKey);
                if (currClient == null){
                    ResponseHandler.failure(request, response, 'Invalid API key', 401);
                    return;
                }
                request.currentClient = currClient;
                next();
            }
        } catch (error) {
            logger.info(error.message);
            ResponseHandler.failure(request, response, 'Client app authentication error: ' + error.message, 401);
        }
    };

}
