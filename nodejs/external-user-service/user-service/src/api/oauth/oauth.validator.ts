import express from 'express';
import Joi from 'joi';
import { GitHubOAuthCallbackParams } from '../../domain.types/users/github.oauth.types';
import { GoogleOAuthCallbackParams } from '../../domain.types/users/google.oauth.types';
import { FacebookOAuthCallbackParams } from '../../domain.types/users/facebook.oauth.types';
import { TwitterOAuthCallbackParams } from '../../domain.types/users/twitter.oauth.types';
import { ErrorHandler } from '../../common/handlers/error.handler';

///////////////////////////////////////////////////////////////////////////////////////

export class OAuthValidator {

    //#region GitHub OAuth

    static githubOAuthCallback = async (request: express.Request)
        : Promise<GitHubOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: GitHubOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    //#endregion

    //#region Google OAuth

    static googleOAuthCallback = async (request: express.Request)
        : Promise<GoogleOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code     : Joi.string().required(),
                state    : Joi.string().optional(),
                scope    : Joi.string().optional(),
                authuser : Joi.string().optional(),  // Google sometimes includes this
                prompt   : Joi.string().optional(),  // Google sometimes includes this
                hd       : Joi.string().optional()   // Google Workspace domain
            });

            await schema.validateAsync(request.query);

            const params: GoogleOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string,
                scope : request.query.scope as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    //#endregion

    //#region Facebook OAuth

    static facebookOAuthCallback = async (request: express.Request)
        : Promise<FacebookOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: FacebookOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    //#endregion

    //#region Twitter OAuth

    static twitterOAuthCallback = async (request: express.Request)
        : Promise<TwitterOAuthCallbackParams> => {
        try {
            const schema = Joi.object({
                code  : Joi.string().required(),
                state : Joi.string().optional()
            });

            await schema.validateAsync(request.query);

            const params: TwitterOAuthCallbackParams = {
                code  : request.query.code as string,
                state : request.query.state as string
            };

            return params;
        } catch (error) {
            ErrorHandler.handleValidationError(error);
        }
    };

    //#endregion
}
