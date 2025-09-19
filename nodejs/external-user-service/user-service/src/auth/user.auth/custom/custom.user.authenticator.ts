import express from 'express';
import jwt, { JwtPayload } from 'jsonwebtoken';
import { logger } from '../../../logger/logger';
import { IUserAuthenticator } from '../interfaces/user.authenticator.interface';
import { ActionScope, AuthResult } from '../auth.types';
import { CurrentUser } from '../../../domain.types/miscellaneous/current.user';
import { ConfigurationManager } from '../../../config/configuration.manager';
import { UserService } from '../../../services/users/user.service';
import { TenantService } from '../../../services/tenant/tenant.service';
import { Injector } from '../../../startup/injector';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { RoleService } from '../../../services/authorization/role.service';
import { UserAuthService } from '../../../services/users/user.auth.service';
import { JwtRsaTokenService } from '../../../services/jwt.rsa.token.service';

/////////////////////////////////////////////////////////////////////////////////

export class CustomUserAuthenticator implements IUserAuthenticator {

    _userService: UserService = null;
    _jwtRsaService: JwtRsaTokenService = null;

    _userAuthService: UserAuthService = null;

    _tenantService: TenantService = null;

    _roleService: RoleService = null;

    constructor() {
        this._userService = Injector.Container.resolve(UserService);
        this._userAuthService = Injector.Container.resolve(UserAuthService);
        this._tenantService = Injector.Container.resolve(TenantService);
        this._roleService = Injector.Container.resolve(RoleService);
                this._jwtRsaService = JwtRsaTokenService.getInstance();
    }

    public authenticate = async (
        request: express.Request
    ): Promise<AuthResult> => {

        let res: AuthResult = {
            Result        : true,
            Message       : 'Authenticated',
            HttpErrorCode : 200,
        };

        try {

            //////////////////////////////////////////////////////////////////////////////////////////
            // Already taken care of in the auth.handler
            // if (!request.clientAppAuth && request.alternateAuth) {
            //     // Cuurently, this check is applicable only for the specific endpoints, where
            //     // there is a need to allow alternate authentication mechanism.
            //     // For example, client-app specific endpoints like renew and get API keys.
            //     // Here we are using basic authentication (username and password) instead of JWT token.
            //     // For all other endpoints, this check is not applicable.
            //     return res;
            // }
            //////////////////////////////////////////////////////////////////////////////////////////

            const publicAccess = request.actionScope === ActionScope.Public;
            const optionalUserAuth = request.optionalUserAuth;
            const privilegedClient = request.currentClient?.IsPrivileged as boolean;

            const authHeader = request.headers['authorization'];
            const token = authHeader && authHeader.split(' ')[1];

            const missingToken = token == null || token === 'null' || token === undefined;

            const allowWithoutToken = publicAccess || optionalUserAuth || privilegedClient;

            if (missingToken) {
                if (allowWithoutToken) {
                    logger.info('Access is allowed without token.');
                    return res;
                }
                res = {
                    Result        : false,
                    Message       : 'Unauthorized user access',
                    HttpErrorCode : 401,
                };
                logger.info('Unauthorized user access: Missing token');
                return res;
            }

            // RSA JWT verification
            const claims = this._jwtRsaService.verifyToken(token);
            if (!claims) {
                res = {
                    Result        : false,
                    Message       : 'Invalid or expired user login session.',
                    HttpErrorCode : 403,
                };
                logger.info('Invalid or expired user login session.');
                return res;
            }
            // Convert JWT claims to CurrentUser format
            const currentUser: CurrentUser = {
                UserId: claims.userId || claims.sub,
                TenantId: claims.tenantId || '',
                TenantCode: claims.tenantId || 'default',
                TenantName: claims.tenantName || 'Default Tenant',
                DisplayName: claims.displayName || claims.username || claims.email || 'Unknown',
                PhoneCode: claims.phoneCode || '',
                PhoneNumber: '', // Not available in JWT claims
                Email: claims.email || '',
                UserName: claims.username || '',
                SessionId: claims.sessionId,
                IsTestUser: false, // Default value
                Roles: claims.role ? [{ id: 'role-id', Name: claims.role }] : []
            };

            if (!currentUser.SessionId) {
                const isPrivilegedAccess = request.currentClient.IsPrivileged as boolean;
                if (isPrivilegedAccess) {
                    request.currentUser = currentUser;
                    logger.info('Privileged access granted without session Id.');
                    return res;
                }
                res = {
                    Result        : false,
                    Message       : 'Your session has expired. Please login to the app again.',
                    HttpErrorCode : 403,
                };
                logger.info('Unauthorized user access: Missing session Id.');

                return res;
            }

            var isValidUserLoginSession = await this._userAuthService.isValidUserLoginSession(currentUser.SessionId);

            if (!isValidUserLoginSession) {
                res = {
                    Result        : false,
                    Message       : 'Invalid or expired user login session.',
                    HttpErrorCode : 403,
                };
                logger.info('Invalid or expired user login session.');
                return res;
            }

            request.currentUser = currentUser;
            request.currentUserTenantId = request.currentUser?.TenantId;
            res = {
                Result        : true,
                Message       : 'Authenticated',
                HttpErrorCode : 200,
            };

            return res;
        } catch (err) {
            logger.info(JSON.stringify(err, null, 2));
            logger.info(err.message);
            res = {
                Result        : false,
                Message       : 'Forbidden user access: ' + err.message, // Please do not change this and if needed to change then check with app developer
                HttpErrorCode : 403,
            };
            return res;
        }
    };

    public rotateUserSessionToken = async (refreshToken: string): Promise<string> => {
        if (!refreshToken) {
            throw ('Invalid refresh token');
        }
        const payload = await this.verifyJwtToken(refreshToken, process.env.USER_REFRESH_TOKEN_SECRET);
        if (!payload) {
            throw ('Invalid or expired user login session.');
        }
        const userId = payload.UserId;
        const sessionId = payload.SessionId;
        var isValidUserLoginSession = await this._userAuthService.isValidUserLoginSession(sessionId);
        if (!isValidUserLoginSession) {
            throw ('Invalid or expired user login session.');
        }
        const user = await this._userService.getById(userId);
        if (!user) {
            throw ('Invalid user');
        }
        const roles = user.Roles.map(r => ({ id: r.id, Name: r.Name }));
        const tenant = await this._tenantService.getById(user.Tenant?.id);
        if (!tenant) {
            throw ('Invalid tenant');
        }
        var currentUser: CurrentUser = {
            UserId      : user.id,
            TenantId    : tenant.id,
            TenantCode  : tenant.TenantCode,
            TenantName  : tenant.Name,
            DisplayName : user.Metadata?.DisplayName,
            PhoneCode   : user.PhoneCode,
            PhoneNumber : user.PhoneNumber,
            Email       : user.Email,
            UserName    : user.UserName,
            Roles       : roles,
            SessionId   : sessionId,
            IsTestUser  : user.Metadata?.IsTestUser,
        };
        const accessToken = await this.generateUserSessionToken(currentUser);
        return accessToken;
    };

    public generateRefreshToken = async (currentUser: CurrentUser): Promise<string> => {

        const expiresIn: number = ConfigurationManager.RefreshTokenExpiresInSeconds;
        var seconds = expiresIn.toString() + 's';
        const user = await this._userService.getById(currentUser.UserId);
        if (!user) {
            throw ('Invalid user');
        }
        const tenant = await this._tenantService.getById(currentUser.TenantId);
        if (!tenant) {
            throw ('Invalid tenant');
        }

        var payload = {
            UserId     : currentUser.UserId,
            TenantId   : currentUser.TenantId,
            Roles      : currentUser.Roles,
            SessionId  : currentUser.SessionId,
            IsTestUser : currentUser.IsTestUser,
        };
        const token = jwt.sign(payload, process.env.USER_REFRESH_TOKEN_SECRET, { expiresIn: seconds });
        return token;
    };

    public generateUserSessionToken = async (user: CurrentUser): Promise<string> => {
        return new Promise((resolve, reject) => {
            try {
                // Use RSA JWT service instead of HMAC (like Python/C# implementations)
                const sessionId = user.SessionId || 'default-session';
                const role = user.Roles && user.Roles.length > 0 ? user.Roles[0].Name : undefined;
                const token = this._jwtRsaService.generateToken(user, sessionId, role);
                resolve(token);
            } catch (error) {
                logger.error(`Error generating RSA JWT token: ${error.message}`);
                reject(error);
            }
        });
    };

    public generatePasswordResetToken = async (userId: uuid, email: string, tenantId: uuid): Promise<string> => {
        const token = jwt.sign(
            { UserId: userId, Email: email, TenantId: tenantId },
            process.env.PASSWORD_RESET_TOKEN_SECRET,
            { expiresIn: '3h' });
        return token;
    };

    public verifyPasswordResetToken = async (token: string): Promise<{ UserId: uuid, TenantId: uuid }|null> => {
        const payload = await this.verifyJwtToken(token, process.env.PASSWORD_RESET_TOKEN_SECRET);
        if (!payload) {
            return null;
        }
        return { UserId: payload.UserId, TenantId: payload.TenantId };
    };

    public verifyJwtToken = async (token: string, secret: string): Promise<JwtPayload> => {
        return new Promise((resolve, reject) => {
            jwt.verify(token, secret, (err, decoded) => {
                if (err) {
                    logger.info('Token invalid or expired');
                    return reject(new Error('Token invalid or expired'));
                }
                // Ensure payload is an object (JwtPayload)
                if (typeof decoded === 'string' || !decoded) {
                    logger.info('Invalid token payload');
                    return reject(new Error('Invalid token payload'));
                }
                resolve(decoded as JwtPayload);
            });
        });
    };

}
