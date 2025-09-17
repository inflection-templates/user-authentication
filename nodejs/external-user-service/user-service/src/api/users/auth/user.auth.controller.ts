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
import { UserPasswordChangeModel } from '../../../domain.types/users/user.password.change.types';
import { UserDeviceDetailsService } from '../../../services/users/user.device.details.service';
import { UserAccountActionResult, UserDto } from '../../../domain.types/users/user.types';
import { GitHubOAuthCallbackParams, GitHubAccessTokenModel, GitHubUser, GitHubEmail } from '../../../domain.types/users/github.oauth.types';
import { GoogleOAuthCallbackParams, GoogleAccessTokenModel, GoogleUser } from '../../../domain.types/users/google.oauth.types';
import { FacebookOAuthCallbackParams, FacebookAccessTokenModel, FacebookUser } from '../../../domain.types/users/facebook.oauth.types';
import { ConfigurationManager } from '../../../config/configuration.manager';
import axios from 'axios';
import { Helper } from '../../../common/helper';

////////////////////////////////////////////////////////////////

export class UserAuthController extends BaseController {

    //#region member variables and constructors

    _service: UserAuthService = Injector.Container.resolve(UserAuthService);

    _userService: UserService = Injector.Container.resolve(UserService);

    _userDeviceDetailsService = Injector.Container.resolve(UserDeviceDetailsService);

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
            const clientId = process.env.GITHUB_CLIENT_ID;
            const baseUrl = process.env.BASE_URL;
            const redirectUri = `${baseUrl}/api/v1/auth/oauth/github/callback`;
            const state = Math.random().toString(36).substring(2, 15);
            
            if (!clientId) {
                ResponseHandler.failure(request, response, 'GitHub OAuth not configured. Please set GITHUB_CLIENT_ID environment variable.', 500);
                return;
            }

            if (!baseUrl) {
                ResponseHandler.failure(request, response, 'BASE_URL environment variable is required.', 500);
                return;
            }

            const authUrl = `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=user:email&state=${state}`;
            
            ResponseHandler.success(request, response, 'GitHub OAuth URL generated', 200, {
                authUrl: authUrl,
                state: state
            });

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    githubOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const { code, state }: GitHubOAuthCallbackParams = await UserAuthValidator.githubOAuthCallback(request);

            // Get GitHub OAuth configuration
            const clientId = process.env.GITHUB_CLIENT_ID;
            const clientSecret = process.env.GITHUB_CLIENT_SECRET;
            const redirectUri = process.env.GITHUB_REDIRECT_URI;

            if (!clientId || !clientSecret) {
                ResponseHandler.failure(request, response, 'GitHub OAuth configuration is missing', 500);
                return;
            }

            // Exchange code for access token
            const tokenUrl = `https://github.com/login/oauth/access_token?client_id=${clientId}&client_secret=${clientSecret}&code=${code}`;
            
            const tokenResponse = await axios.post(tokenUrl, '', {
                headers: {
                    'Accept': 'application/json'
                }
            });

            if (!tokenResponse.data || !tokenResponse.data.access_token) {
                ResponseHandler.failure(request, response, 'Unable to retrieve GitHub access token', 500);
                return;
            }

            const githubAccessToken = tokenResponse.data.access_token;

            // Get GitHub user information
            const githubUser = await this.getGithubUser(githubAccessToken);
            if (!githubUser) {
                ResponseHandler.failure(request, response, 'Unable to retrieve GitHub user information', 500);
                return;
            }

            // Get user email
            let userEmail = await this.getGithubUserEmail(githubAccessToken);
            if (!userEmail) {
                userEmail = githubUser.email;
                if (!userEmail) {
                    userEmail = `${githubUser.login}@github.oauth.user`;
                }
            }

            // Check if user exists
            const existingUser = await this._userService.getByEmail(null, userEmail);
            
            if (existingUser) {
                // User exists, log them in
                const loginResult = await this._service.loginWithOAuth(existingUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${existingUser.UserName} logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : existingUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, existingUser);
                ResponseHandler.success(request, response, message, 200, data, true);
            } else {
                // Create new user
                const nameTokens = githubUser.name?.split(' ') || [];
                const firstName = nameTokens[0] || '';
                const lastName = nameTokens.length > 1 ? nameTokens[1] : '';

                // Get default tenant for user creation
                const tenant = await this._service['_tenantRepo'].getTenantWithCode('default');
                const tenantId = tenant?.id || null;

                const createModel = {
                    TenantId  : tenantId,
                    FirstName : firstName,
                    LastName  : lastName,
                    Email     : userEmail,
                    UserName  : githubUser.login,
                    Password  : Helper.generatePassword(),
                };

                const createdUser = await this._userService.create(createModel);
                if (!createdUser) {
                    ResponseHandler.failure(request, response, 'Unable to create a new user', 500);
                    return;
                }

                const loginResult = await this._service.loginWithOAuth(createdUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${createdUser.UserName} logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : createdUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, createdUser);
                ResponseHandler.success(request, response, message, 200, data, true);
            }

        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    googleOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const clientId = process.env.GOOGLE_CLIENT_ID;
            const baseUrl = process.env.BASE_URL;
            const redirectUri = `${baseUrl}/api/v1/auth/oauth/google/callback`;
            const state = Math.random().toString(36).substring(2, 15);
            
            if (!clientId) {
                ResponseHandler.failure(request, response, 'Google OAuth not configured. Please set GOOGLE_CLIENT_ID environment variable.', 500);
                return;
            }

            if (!baseUrl) {
                ResponseHandler.failure(request, response, 'BASE_URL environment variable is required.', 500);
                return;
            }

            const authUrl = `https://accounts.google.com/o/oauth2/auth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=openid%20email%20profile&response_type=code&state=${state}`;

            ResponseHandler.success(request, response, 'Google OAuth URL generated', 200, {
                authUrl: authUrl,
                state: state
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    googleOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const { code, state }: GoogleOAuthCallbackParams = await UserAuthValidator.googleOAuthCallback(request);

            const clientId = process.env.GOOGLE_CLIENT_ID;
            const clientSecret = process.env.GOOGLE_CLIENT_SECRET;
            const baseUrl = process.env.BASE_URL;
            const redirectUri = `${baseUrl}/api/v1/auth/oauth/google/callback`;

            if (!clientId || !clientSecret) {
                ResponseHandler.failure(request, response, 'Google OAuth not configured', 500);
                return;
            }

            // Exchange code for access token
            const tokenUrl = 'https://oauth2.googleapis.com/token';
            const tokenResponse = await axios.post(tokenUrl, {
                client_id: clientId,
                client_secret: clientSecret,
                code: code,
                grant_type: 'authorization_code',
                redirect_uri: redirectUri
            }, {
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                }
            });

            if (tokenResponse.status !== 200) {
                ResponseHandler.failure(request, response, 'Unable to retrieve Google access token', 500);
                return;
            }

            const tokenData: GoogleAccessTokenModel = tokenResponse.data;
            const googleAccessToken = tokenData.access_token;

            // Get user information from Google
            const googleUser = await this.getGoogleUser(googleAccessToken);
            if (!googleUser) {
                ResponseHandler.failure(request, response, 'Unable to retrieve Google user information', 500);
                return;
            }

            const userEmail = googleUser.email;
            if (!userEmail) {
                ResponseHandler.failure(request, response, 'Google user email is required', 400);
                return;
            }

            // Check if user already exists
            const existingUser = await this._userService.getByEmail(null, userEmail);
            
            if (existingUser) {
                // User exists, log them in
                const loginResult = await this._service.loginWithOAuth(existingUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${existingUser.UserName} logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : existingUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, existingUser);
                ResponseHandler.success(request, response, message, 200, data, true);
            } else {
                // Create new user
                const firstName = googleUser.given_name || '';
                const lastName = googleUser.family_name || '';

                // Get default tenant for user creation
                const tenant = await this._service['_tenantRepo'].getTenantWithCode('default');
                const tenantId = tenant?.id || null;

                const createModel = {
                    FirstName : firstName,
                    LastName  : lastName,
                    Email     : userEmail,
                    UserName  : googleUser.email.split('@')[0], // Use email prefix as username
                    Password  : Helper.generatePassword(),
                    TenantId  : tenantId,
                };

                const newUser = await this._userService.create(createModel);
                if (!newUser) {
                    ResponseHandler.failure(request, response, 'Unable to create user account', 500);
                    return;
                }

                // Log in the new user
                const loginResult = await this._service.loginWithOAuth(newUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${newUser.UserName} created and logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : newUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, newUser);
                ResponseHandler.success(request, response, message, 201, data, true);
            }
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    private async getGoogleUser(accessToken: string): Promise<GoogleUser | null> {
        try {
            const response = await axios.get('https://www.googleapis.com/oauth2/v2/userinfo', {
                headers: {
                    'Authorization': `Bearer ${accessToken}`
                }
            });

            if (response.status === 200) {
                return response.data as GoogleUser;
            }
            return null;
        } catch (error) {
            console.error('Error fetching Google user:', error);
            return null;
        }
    }

    facebookOAuthLogin = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const clientId = process.env.FACEBOOK_CLIENT_ID;
            const baseUrl = process.env.BASE_URL;
            const redirectUri = `${baseUrl}/api/v1/auth/oauth/facebook/callback`;
            const state = Math.random().toString(36).substring(2, 15);
            
            if (!clientId) {
                ResponseHandler.failure(request, response, 'Facebook OAuth not configured. Please set FACEBOOK_CLIENT_ID environment variable.', 500);
                return;
            }

            if (!baseUrl) {
                ResponseHandler.failure(request, response, 'BASE_URL environment variable is required.', 500);
                return;
            }

            const authUrl = `https://www.facebook.com/v18.0/dialog/oauth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=email,public_profile&response_type=code&state=${state}`;

            ResponseHandler.success(request, response, 'Facebook OAuth URL generated', 200, {
                authUrl: authUrl,
                state: state
            });
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    facebookOAuthCallback = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const { code, state }: FacebookOAuthCallbackParams = await UserAuthValidator.facebookOAuthCallback(request);

            const clientId = process.env.FACEBOOK_CLIENT_ID;
            const clientSecret = process.env.FACEBOOK_CLIENT_SECRET;
            const baseUrl = process.env.BASE_URL;
            const redirectUri = `${baseUrl}/api/v1/auth/oauth/facebook/callback`;

            if (!clientId || !clientSecret) {
                ResponseHandler.failure(request, response, 'Facebook OAuth not configured', 500);
                return;
            }

            // Exchange code for access token
            const tokenUrl = `https://graph.facebook.com/v18.0/oauth/access_token?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&client_secret=${clientSecret}&code=${code}`;
            
            const tokenResponse = await axios.get(tokenUrl);

            if (tokenResponse.status !== 200 || !tokenResponse.data.access_token) {
                ResponseHandler.failure(request, response, 'Unable to retrieve Facebook access token', 500);
                return;
            }

            const tokenData: FacebookAccessTokenModel = tokenResponse.data;
            const facebookAccessToken = tokenData.access_token;

            // Get user information from Facebook
            const facebookUser = await this.getFacebookUser(facebookAccessToken);
            if (!facebookUser) {
                ResponseHandler.failure(request, response, 'Unable to retrieve Facebook user information', 500);
                return;
            }

            const userEmail = facebookUser.email;
            if (!userEmail) {
                ResponseHandler.failure(request, response, 'Facebook user email is required. Please ensure email permission is granted.', 400);
                return;
            }

            // Check if user already exists
            const existingUser = await this._userService.getByEmail(null, userEmail);
            
            if (existingUser) {
                // User exists, log them in
                const loginResult = await this._service.loginWithOAuth(existingUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${existingUser.UserName} logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : existingUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, existingUser);
                ResponseHandler.success(request, response, message, 200, data, true);
            } else {
                // Create new user
                const firstName = facebookUser.first_name || '';
                const lastName = facebookUser.last_name || '';

                // Get default tenant for user creation
                const tenant = await this._service['_tenantRepo'].getTenantWithCode('default');
                const tenantId = tenant?.id || null;

                const createModel = {
                    FirstName : firstName,
                    LastName  : lastName,
                    Email     : userEmail,
                    UserName  : userEmail.split('@')[0], // Use email prefix as username
                    Password  : Helper.generatePassword(),
                    TenantId  : tenantId,
                };

                const newUser = await this._userService.create(createModel);
                if (!newUser) {
                    ResponseHandler.failure(request, response, 'Unable to create user account', 500);
                    return;
                }

                // Log in the new user
                const loginResult = await this._service.loginWithOAuth(newUser.id);
                if (!loginResult) {
                    ResponseHandler.failure(request, response, 'Session cannot be created', 500);
                    return;
                }

                const message = `User ${newUser.UserName} created and logged in successfully!`;
                const data = {
                    AccessToken  : loginResult.AccessToken,
                    RefreshToken : loginResult.RefreshToken,
                    User         : newUser,
                    SessionId    : loginResult.SessionId,
                    ExpiresAt    : loginResult.ExpiresAt
                };

                UserEvents.onUserLoginWithOAuth(request, newUser);
                ResponseHandler.success(request, response, message, 201, data, true);
            }
        } catch (error) {
            ResponseHandler.handleError(request, response, error);
        }
    };

    private async getFacebookUser(accessToken: string): Promise<FacebookUser | null> {
        try {
            const response = await axios.get(`https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,picture&access_token=${accessToken}`);

            if (response.status === 200) {
                return response.data as FacebookUser;
            }
            return null;
        } catch (error) {
            console.error('Error fetching Facebook user:', error);
            return null;
        }
    }

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
            Roles             : user.Roles,
            IsProfileComplete : isProfileComplete,
            SessionId         : result.SessionId,
            ExpiresAt         : result.ExpiresAt
        };
        return data;
    }

    private async getGithubUser(token: string): Promise<GitHubUser | null> {
        try {
            const apiUrl = 'https://api.github.com/user';
            const response = await axios.get(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/vnd.github.v3+json',
                    'User-Agent': 'User-Service-OAuth'
                }
            });

            if (response.status === 200 && response.data) {
                logger.info('GitHub User Response:');
                logger.info(JSON.stringify(response.data, null, 2));
                return response.data as GitHubUser;
            }
            
            logger.info('GitHub User API Error:');
            logger.info(`Status: ${response.status}`);
            return null;

        } catch (error) {
            logger.info('Error fetching GitHub user:');
            logger.info(error.message);
            return null;
        }
    }

    private async getGithubUserEmail(token: string): Promise<string | null> {
        try {
            const apiUrl = 'https://api.github.com/user/emails';
            const response = await axios.get(apiUrl, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Accept': 'application/vnd.github.v3+json',
                    'User-Agent': 'User-Service-OAuth'
                }
            });

            if (response.status === 200 && response.data) {
                logger.info('GitHub Emails Response:');
                logger.info(JSON.stringify(response.data, null, 2));
                
                const emails = response.data as GitHubEmail[];
                
                // Get the primary email or the first verified email
                const primaryEmail = emails.find(e => e.primary)?.email;
                const verifiedEmail = emails.find(e => e.verified)?.email;
                
                return primaryEmail || verifiedEmail || emails[0]?.email || null;
            }
            
            logger.info('GitHub Emails API Error:');
            logger.info(`Status: ${response.status}`);
            return null;

        } catch (error) {
            logger.info('Error fetching GitHub user emails:');
            logger.info(error.message);
            return null;
        }
    }

}
