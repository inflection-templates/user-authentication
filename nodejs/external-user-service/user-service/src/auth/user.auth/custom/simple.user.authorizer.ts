import express from 'express';
import { logger } from '../../../logger/logger';
import { IUserAuthorizer } from '../interfaces/user.authorizer.interface';
import { ActionScope } from '../auth.types';

//////////////////////////////////////////////////////////////

export class SimpleUserAuthorizer implements IUserAuthorizer {

    public authorize = async (request: express.Request): Promise<boolean> => {
        try {
            const context = request.context;
            if (context == null || context === 'undefined') {
                logger.info('Authorizer: Request context is not set.');
                return false;
            }

            // If the client is privileged, then allow access to all resources
            if (request.currentClient?.IsPrivileged) {
                logger.info('Authorizer: Privileged client access granted.');
                return true;
            }

            const publicAccess = request.actionScope === ActionScope.Public;
            const optionalUserAuth = request.optionalUserAuth;
            const allowUnauthorized = publicAccess || optionalUserAuth;

            const currentUser = request.currentUser ?? null;
            if (!currentUser) {
                //If the user is not authenticated, then check if the resource access is public
                if (allowUnauthorized) {
                    // To check whether a particular resource is available for public access, e.g. a profile image download
                    logger.info('Authorizer: Public access granted.');
                    return true;
                }
                // If the resource is not public, then the user must be authenticated
                return false;
            }

            // User is authenticated, allow access
            logger.info('Authorizer: User authenticated successfully.');
            return true;

        } catch (error) {
            logger.info(error.message);
        }
        return false;
    };

}
