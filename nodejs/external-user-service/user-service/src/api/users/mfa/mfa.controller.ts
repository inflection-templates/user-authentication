import express from 'express';
import { BaseController } from '../../base.controller';
import { MfaService, MfaSetupResult, MfaValidationResult, MfaStatus } from '../../../services/mfa/mfa.service';
import { ResponseHandler } from '../../../common/handlers/response.handler';
import { logger } from '../../../logger/logger';
import { Injector } from '../../../startup/injector';
import { uuid } from '../../../domain.types/miscellaneous/system.types';
import { ApiError } from '../../../common/api.error';

export interface MfaSetupRequest {
    userIdentifier?: string;
}

export interface MfaVerifySetupRequest {
    token: string;
}

export interface MfaValidateRequest {
    token: string;
    method?: 'totp' | 'backup';
}

export interface MfaDisableRequest {
    token: string;
}

export interface GenerateBackupCodesRequest {
    token: string;
}

export class MfaController extends BaseController {

    //#region member variables and constructors

    _mfaService: MfaService = Injector.Container.resolve(MfaService);

    constructor() {
        super();
    }

    //#endregion

    //#region Setup MFA

    /**
     * Setup TOTP for the current user
     * POST /api/v1/users/mfa/setup-totp
     */
    public setupTotp = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const requestBody = request.body as MfaSetupRequest;
            const userIdentifier = requestBody.userIdentifier || currentUser.Email || currentUser.UserName || 'User';

            const result: MfaSetupResult = await this._mfaService.setupTotp(currentUser.UserId, userIdentifier);

            if (result.success) {
                ResponseHandler.success(request, response, 'TOTP setup initiated successfully', 200, {
                    qrCodeDataUrl: result.totpSecret?.qrCodeDataUrl,
                    qrCodeUrl: result.totpSecret?.qrCodeUrl,
                    backupCodes: result.backupCodes,
                    message: result.message
                });
            } else {
                ResponseHandler.failure(request, response, result.message, 400);
            }

        } catch (error) {
            logger.error(`Error in setupTotp: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    /**
     * Verify and enable TOTP for the current user
     * POST /api/v1/users/mfa/verify-totp-setup
     */
    public verifyTotpSetup = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const requestBody = request.body as MfaVerifySetupRequest;
            if (!requestBody.token || requestBody.token.length !== 6) {
                ResponseHandler.failure(request, response, 'Invalid TOTP token format', 400);
                return;
            }

            const result: MfaSetupResult = await this._mfaService.verifyAndEnableTotp(currentUser.UserId, requestBody.token);

            if (result.success) {
                ResponseHandler.success(request, response, 'TOTP enabled successfully', 200, {
                    message: result.message,
                    enabled: true
                });
            } else {
                ResponseHandler.failure(request, response, result.message, 400);
            }

        } catch (error) {
            logger.error(`Error in verifyTotpSetup: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    //#endregion

    //#region Validate MFA

    /**
     * Validate MFA token (TOTP or backup code)
     * POST /api/v1/users/mfa/validate
     */
    public validateMfa = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const requestBody = request.body as MfaValidateRequest;
            if (!requestBody.token) {
                ResponseHandler.failure(request, response, 'Token is required', 400);
                return;
            }

            const method = requestBody.method || 'totp';
            const result: MfaValidationResult = await this._mfaService.validateMfa(
                currentUser.UserId, 
                requestBody.token, 
                method
            );

            if (result.success) {
                ResponseHandler.success(request, response, 'MFA validation successful', 200, {
                    method: result.method,
                    message: result.message,
                    valid: true
                });
            } else {
                ResponseHandler.failure(request, response, result.message, 400);
            }

        } catch (error) {
            logger.error(`Error in validateMfa: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    //#endregion

    //#region Manage MFA

    /**
     * Get MFA status for the current user
     * GET /api/v1/users/mfa/status
     */
    public getMfaStatus = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const status: MfaStatus = await this._mfaService.getMfaStatus(currentUser.UserId);

            ResponseHandler.success(request, response, 'MFA status retrieved successfully', 200, {
                enabled: status.enabled,
                methods: status.methods,
                backupCodesRemaining: status.backupCodesRemaining
            });

        } catch (error) {
            logger.error(`Error in getMfaStatus: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    /**
     * Disable MFA for the current user
     * POST /api/v1/users/mfa/disable
     */
    public disableMfa = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const requestBody = request.body as MfaDisableRequest;
            if (!requestBody.token) {
                ResponseHandler.failure(request, response, 'TOTP token is required to disable MFA', 400);
                return;
            }

            const success = await this._mfaService.disableMfa(currentUser.UserId, requestBody.token);

            if (success) {
                ResponseHandler.success(request, response, 'MFA disabled successfully', 200, {
                    enabled: false,
                    message: 'Multi-factor authentication has been disabled for your account'
                });
            } else {
                ResponseHandler.failure(request, response, 'Failed to disable MFA', 400);
            }

        } catch (error) {
            logger.error(`Error in disableMfa: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    /**
     * Generate new backup codes
     * POST /api/v1/users/mfa/generate-backup-codes
     */
    public generateBackupCodes = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const requestBody = request.body as GenerateBackupCodesRequest;
            if (!requestBody.token) {
                ResponseHandler.failure(request, response, 'TOTP token is required to generate backup codes', 400);
                return;
            }

            const backupCodes = await this._mfaService.generateNewBackupCodes(currentUser.UserId, requestBody.token);

            ResponseHandler.success(request, response, 'Backup codes generated successfully', 200, {
                backupCodes: backupCodes,
                message: 'Please save these backup codes in a secure location. They can be used to access your account if you lose your authenticator device.'
            });

        } catch (error) {
            logger.error(`Error in generateBackupCodes: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    //#endregion

    //#region Utility Methods

    /**
     * Get QR code for TOTP setup (alternative endpoint)
     * GET /api/v1/users/mfa/qr-code
     */
    public getQrCode = async (request: express.Request, response: express.Response): Promise<void> => {
        try {
            const currentUser = request.currentUser;
            if (!currentUser) {
                ResponseHandler.failure(request, response, 'Unauthorized', 401);
                return;
            }

            const userIdentifier = currentUser.Email || currentUser.UserName || 'User';
            const result: MfaSetupResult = await this._mfaService.setupTotp(currentUser.UserId, userIdentifier);

            if (result.success && result.totpSecret) {
                // Return QR code as image
                const qrCodeBuffer = Buffer.from(result.totpSecret.qrCodeDataUrl.split(',')[1], 'base64');
                response.set({
                    'Content-Type': 'image/png',
                    'Content-Length': qrCodeBuffer.length
                });
                response.send(qrCodeBuffer);
            } else {
                ResponseHandler.failure(request, response, result.message, 400);
            }

        } catch (error) {
            logger.error(`Error in getQrCode: ${error.message}`);
            ResponseHandler.failure(request, response, 'Internal server error', 500);
        }
    };

    //#endregion
}
