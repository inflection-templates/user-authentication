import express from 'express';
import { UserService } from "../../../services/users/user.service";
import { ResponseHandler } from "../../../common/handlers/response.handler";
import { BaseController } from "../../base.controller";
import { Injector } from "../../../startup/injector";
import { ApiError } from '../../../common/api.error';
import { logger } from '../../../logger/logger';
import { UserAuthValidator } from './user.auth.validator';
import { UserEvents } from '../user.events';
import {
    UserLoginPasswordEmailParams,
    UserLoginPasswordPhoneParams,
    UserLoginPasswordUsernameParams,
    UserLoginResult
} from '../../../domain.types/users/user.session.types';
import {
    UserPasswordResetByOtpModel,
    UserPasswordResetByTokenModel,
    UserPasswordResetSendEmailLinkModel,
    UserPasswordResetSendEmailOtpModel,
    UserPasswordResetSendPhoneOtpModel
} from '../../../domain.types/users/user.password.reset.types';
import { UserAuthService } from '../../../services/users/user.auth.service';
import { UserOAuthService } from '../../../services/users/user.oauth.service';
import { UserPasswordChangeModel } from '../../../domain.types/users/user.password.change.types';
import { UserDeviceDetailsService } from '../../../services/users/user.device.details.service';
import { UserAccountActionResult, UserDto } from '../../../domain.types/users/user.types';
import { GitHubOAuthCallbackParams } from '../../../domain.types/users/github.oauth.types';
import { GoogleOAuthCallbackParams } from '../../../domain.types/users/google.oauth.types';
import { FacebookOAuthCallbackParams } from '../../../domain.types/users/facebook.oauth.types';
import { TwitterOAuthCallbackParams } from '../../../domain.types/users/twitter.oauth.types';

////////////////////////////////////////////////////////////////

export class UserAuthController extends BaseController {

    //#region member variables and constructors

    _service: UserAuthService = Injector.Container.resolve(UserAuthService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _userDeviceDetailsService = Injector.Container.resolve(UserDeviceDetailsService);

    _oauthService: UserOAuthService = Injector.Container.resolve(UserOAuthService);

    constructor() {
        super();
    }

    //#endregion

    loginWithEmailPassword = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const loginObject: UserLoginPasswordEmailParams = await UserAuthValidator.loginWithEmailPassword(request);
            const result = await this._service.loginWithEmailPassword(loginObject);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }
            const user: UserDto = result.User;
            const accessToken = result.AccessToken;
            const refreshToken = result.RefreshToken;

            const message = `User '${user.Metadata?.DisplayName}' logged in successfully!`;
            const data = {
                AccessToken  : accessToken,
                RefreshToken : refreshToken,
                User         : user,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            UserEvents.onUserLoginWithPassword(request, user);

            ResponseHandler.success(request, response, message, 200, data, true);

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    loginWithPhonePassword = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const loginObject: UserLoginPasswordPhoneParams = await UserAuthValidator.loginWithPhonePassword(request);
            const result = await this._service.loginWithPhonePassword(loginObject);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }
            const user: UserDto = result.User;
            const accessToken = result.AccessToken;
            const refreshToken = result.RefreshToken;

            const message = `User '${user.Metadata?.DisplayName}' logged in successfully!`;
            const data = {
                UserId       : user.id,
                AccessToken  : accessToken,
                RefreshToken : refreshToken,
                User         : user,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            UserEvents.onUserLoginWithPassword(request, user);

            ResponseHandler.success(request, response, message, 200, data, true);

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    loginWithUsernamePassword = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserLoginPasswordUsernameParams = await UserAuthValidator.loginWithUsernamePassword(request);
            if (!model.TenantId) {
                const user = await this._userService.getByUserName(model.UserName);
                if (user) {
                    model.TenantId = user.Tenant?.id;
                }
            }
            const result = await this._service.loginWithUsernamePassword(model);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }
            const user: UserDto = result.User;
            const accessToken = result.AccessToken;
            const refreshToken = result.RefreshToken;

            const message = `User '${user.Metadata?.DisplayName}' logged in successfully!`;
            const data = {
                UserId       : user.id,
                AccessToken  : accessToken,
                RefreshToken : refreshToken,
                User         : user,
                SessionId    : result.SessionId,
                ExpiresAt    : result.ExpiresAt
            };

            UserEvents.onUserLoginWithPassword(request, user);

            ResponseHandler.success(request, response, message, 200, data, true);

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    loginWithEmailOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const loginObject = await UserAuthValidator.loginWithEmailOtp(request);
            const result = await this._service.loginWithEmailOtp(loginObject);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }

            const accessToken = result.AccessToken;
            const refreshToken = result.RefreshToken;
            const user = result.User;
            const data = this.extract(user, accessToken, refreshToken, result);
            const message = `User '${user.Metadata?.DisplayName}' logged in successfully!`;
            UserEvents.onUserLoginWithOtp(request, user);
            ResponseHandler.success(request, response, message, 200, data, true);

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    loginWithPhoneOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const loginObject = await UserAuthValidator.loginWithPhoneOtp(request);
            const result = await this._service.loginWithPhoneOtp(loginObject);
            if (result == null) {
                ResponseHandler.failure(request, response, 'User not found!', 404);
                return;
            }

            const user: UserDto = result.User;
            const accessToken = result.AccessToken;
            const refreshToken = result.RefreshToken;

            const data = this.extract(user, accessToken, refreshToken, result);

            const message = `User '${user.Metadata?.DisplayName}' logged in successfully!`;
            UserEvents.onUserLoginWithOtp(request, user);

            ResponseHandler.success(request, response, message, 200, data, true);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    logout = async (request: express.Request, response: express.Response): Promise<any> => {
        try {
            const sesssionId = request.currentUser.SessionId;
            const userId = request.currentUser.UserId;

            const user = await this._userService.getById(userId);
            if (user == null) {
                throw new ApiError(404, 'User not found.');
            }
            if (!sesssionId) {
                return true;
            }
            var invalidated = await this._service.logout(sesssionId);
            if (invalidated) {
                logger.info(`Session invalidated successfully!`);
            }
            UserEvents.onUserLogout(request, user);
            ResponseHandler.success(request, response, 'User logged out successfully!', 200);
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    changePassword = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const userId = request.currentUser.UserId;
            const model: UserPasswordChangeModel = await UserAuthValidator.changePassword(request);
            const result: UserAccountActionResult = await this._service.changePassword(
                userId, model.OldPassword, model.NewPassword);

            const user = result.User;
            UserEvents.onUserPasswordChanged(request, user);

            ResponseHandler.success(request, response, 'Password changed successfully!', 200, {
                PasswordReset : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    resetPasswordByToken = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserPasswordResetByTokenModel   = await UserAuthValidator.resetPasswordByToken(request);
            const result: UserAccountActionResult = await this._service.resetPasswordByToken(model);

            const user = result.User;
            UserEvents.onUserPasswordReset(request, user);

            ResponseHandler.success(request, response, 'Password reset successfully!', 200, {
                PasswordReset : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    resetPasswordByOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserPasswordResetByOtpModel = await UserAuthValidator.resetPasswordByOtp(request);
            const result: UserAccountActionResult = await this._service.resetPasswordByOtp(model);

            const user = result.User;
            UserEvents.onUserPasswordReset(request, user);

            ResponseHandler.success(request, response, 'Password reset successfully!', 200, {
                PasswordReset : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    sendResetPasswordByEmailLink = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserPasswordResetSendEmailLinkModel = await UserAuthValidator.sendResetPasswordByEmailLink(request);
            const result: UserAccountActionResult = await this._service.sendResetPasswordByEmailLink(model);

            const user = result.User;
            UserEvents.sendResetPasswordByEmailLink(request, user);

            ResponseHandler.success(request, response, 'Password reset code sent successfully!', 200, {
                Success : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    sendResetPasswordByEmailOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserPasswordResetSendEmailOtpModel = await UserAuthValidator.sendResetPasswordByEmailLink(request);
            const result: UserAccountActionResult = await this._service.sendResetPasswordByEmailOtp(model);

            const user = result.User;
            UserEvents.sendResetPasswordByEmailOtp(request, user);

            ResponseHandler.success(request, response, 'Password reset code sent successfully!', 200, {
                Success : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    sendResetPasswordByPhoneOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const model: UserPasswordResetSendPhoneOtpModel = await UserAuthValidator.sendResetPasswordByPhoneOtp(request);
            const result: UserAccountActionResult = await this._service.sendResetPasswordByPhoneOtp(model);

            const user = result.User;
            UserEvents.sendResetPasswordByPhoneOtp(request, user);

            ResponseHandler.success(request, response, 'Password reset code sent successfully!', 200, {
                Success : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    generateEmailOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const obj = await UserAuthValidator.generateEmailOtp(request);
            const result: UserAccountActionResult = await this._service.generateEmailOtp(obj);

            const user = result.User;
            UserEvents.onGenerateOtp(request, user);

            ResponseHandler.success(request, response, 'OTP has been successfully generated!', 200, {
                GenerateOTPResult : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    generatePhoneOtp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const obj = await UserAuthValidator.generatePhoneOtp(request);
            const result: UserAccountActionResult = await this._service.generatePhoneOtp(obj);

            const user = result.User;
            UserEvents.onGenerateOtp(request, user);

            ResponseHandler.success(request, response, 'OTP has been successfully generated!', 200, {
                GenerateOTPResult : result.Success,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    rotateUserAccessToken = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const refreshToken = request.params.refreshToken;
            const accessToken = await this._service.rotateUserAccessToken(refreshToken);

            ResponseHandler.success(request, response, 'User access token rotated successfully!', 200, {
                AccessToken : accessToken,
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

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
            const params: GitHubOAuthCallbackParams = await UserAuthValidator.githubOAuthCallback(request);
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
            const params: GoogleOAuthCallbackParams = await UserAuthValidator.googleOAuthCallback(request);
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
            const params: FacebookOAuthCallbackParams = await UserAuthValidator.facebookOAuthCallback(request);
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
            const params: TwitterOAuthCallbackParams = await UserAuthValidator.twitterOAuthCallback(request);
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


    private extract(user: UserDto, accessToken: string, refreshToken: string, result: UserLoginResult) {
        const isProfileComplete = user.FirstName &&
            user.LastName &&
            user.Gender &&
            user.DateOfBirth ? true : false;

        const data = {
            UserId            : user.id,
            AccessToken       : accessToken,
            RefreshToken      : refreshToken,
            User              : user,
            Roles             : [],
            IsProfileComplete : isProfileComplete,
            SessionId         : result.SessionId,
            ExpiresAt         : result.ExpiresAt
        };
        return data;
    }


}
