import 'reflect-metadata';
import express from 'express';
import { IUserAuthenticator } from '../interfaces/user.authenticator.interface';
import { injectable, inject } from "tsyringe";
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { logger } from '../../../logger/logger';
import { CurrentUser } from '../../../domain.types/miscellaneous/current.user';
import { uuid } from '../../../domain.types/miscellaneous/system.types';

///////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserAuthenticator {

    constructor(
        @inject('IUserAuthenticator') private _authenticator: IUserAuthenticator
    ) {}

    public authenticate = async (
        request: express.Request,
        response: express.Response,
        next: express.NextFunction
    ): Promise<void> => {
        try {
            const authResult = await this._authenticator.authenticate(request);
            if (authResult.Result === false){
                ResponseHandler.failure(request, response, authResult.Message, authResult.HttpErrorCode);
                return;
            }
            next();
        } catch (error) {
            logger.info(error.message);
            ResponseHandler.failure(request, response, 'User authentication error: ' + error.message, 401);
        }
    };

    public verify = async (request: express.Request): Promise<boolean> => {
        const authResult = await this._authenticator.authenticate(request);
        return authResult.Result;
    };

    public generateUserSessionToken = async (user: any): Promise<string> => {
        const token = await this._authenticator.generateUserSessionToken(user);
        return token;
    };

    public generateRefreshToken = async (user: CurrentUser): Promise<string> => {
        const token = await this._authenticator.generateRefreshToken(user);
        return token;
    };

    public rotateUserSessionToken = async (refreshToken: string): Promise<string> => {
        const token = await this._authenticator.rotateUserSessionToken(refreshToken);
        return token;
    };

    public generatePasswordResetToken = async (userId: uuid, email: string, tenantId: uuid): Promise<string> => {
        const token = await this._authenticator.generatePasswordResetToken(userId, email, tenantId);
        return token;
    };

    public verifyPasswordResetToken = async (token: string): Promise<{ UserId: uuid, TenantId: uuid }|null> => {
        return await this._authenticator.verifyPasswordResetToken(token);
    };

}
