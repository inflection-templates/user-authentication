import express from 'express';
import { ResponseHandler } from "../../common/handlers/response.handler";
import { BaseController } from "../base.controller";
import { Injector } from "../../startup/injector";
import { UserOAuthService } from '../../services/users/user.oauth.service';
import { OAuthValidator } from './oauth.validator';
import { GitHubOAuthCallbackParams } from '../../domain.types/users/github.oauth.types';
import { GoogleOAuthCallbackParams } from '../../domain.types/users/google.oauth.types';
import { FacebookOAuthCallbackParams } from '../../domain.types/users/facebook.oauth.types';
import { TwitterOAuthCallbackParams } from '../../domain.types/users/twitter.oauth.types';

////////////////////////////////////////////////////////////////

export class OAuthController extends BaseController {

    //#region member variables and constructors

    _oauthService: UserOAuthService = Injector.Container.resolve(UserOAuthService);

    constructor() {
        super();
    }

    //#endregion

    //#region GitHub OAuth

    githubOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const result = await this._oauthService.githubOAuthLogin();
            ResponseHandler.success(request, response, 'GitHub OAuth URL generated', 200, result);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    githubOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const params: GitHubOAuthCallbackParams = await OAuthValidator.githubOAuthCallback(request);
            const result = await this._oauthService.githubOAuthCallback(params, request);
            
            const user = result.User;
            const message = `User ${user.UserName} logged in successfully!`;
            const data = {
                AccessToken  : result.AccessToken,
                RefreshToken : result.RefreshToken,
                User         : result.User,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            ResponseHandler.success(request, response, message, 200, data, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    //#endregion

    //#region Google OAuth

    googleOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const result = await this._oauthService.googleOAuthLogin();
            ResponseHandler.success(request, response, 'Google OAuth URL generated', 200, result);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    googleOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const params: GoogleOAuthCallbackParams = await OAuthValidator.googleOAuthCallback(request);
            const result = await this._oauthService.googleOAuthCallback(params, request);
            
            const user = result.User;
            const message = `User ${user.UserName} logged in successfully!`;
            const data = {
                AccessToken  : result.AccessToken,
                RefreshToken : result.RefreshToken,
                User         : result.User,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            ResponseHandler.success(request, response, message, 200, data, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    //#endregion

    //#region Facebook OAuth

    facebookOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const result = await this._oauthService.facebookOAuthLogin();
            ResponseHandler.success(request, response, 'Facebook OAuth URL generated', 200, result);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    facebookOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const params: FacebookOAuthCallbackParams = await OAuthValidator.facebookOAuthCallback(request);
            const result = await this._oauthService.facebookOAuthCallback(params, request);
            
            const user = result.User;
            const message = `User ${user.UserName} logged in successfully!`;
            const data = {
                AccessToken  : result.AccessToken,
                RefreshToken : result.RefreshToken,
                User         : result.User,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            ResponseHandler.success(request, response, message, 200, data, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    //#endregion

    //#region Twitter OAuth

    twitterOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const result = await this._oauthService.twitterOAuthLogin();
            ResponseHandler.success(request, response, 'Twitter OAuth URL generated', 200, result);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    twitterOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const params: TwitterOAuthCallbackParams = await OAuthValidator.twitterOAuthCallback(request);
            const result = await this._oauthService.twitterOAuthCallback(params, request);
            
            const user = result.User;
            const message = `User ${user.UserName} logged in successfully!`;
            const data = {
                AccessToken  : result.AccessToken,
                RefreshToken : result.RefreshToken,
                User         : result.User,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            ResponseHandler.success(request, response, message, 200, data, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    //#endregion
}
