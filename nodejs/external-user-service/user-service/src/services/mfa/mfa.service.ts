import { injectable, inject } from 'tsyringe';
import { logger } from '../../logger/logger';
import { TotpService, TotpSecret, TotpValidationResult } from './totp.service';
import { UserDto } from '../../domain.types/users/user.types';
import { OTPScope, OTPChannel } from '../../domain.types/users/user.enums';
import { uuid } from '../../domain.types/miscellaneous/system.types';
import { ConfigurationManager } from '../../config/configuration.manager';
import { ApiError } from '../../common/api.error';

export interface MfaSetupResult {
    success: boolean;
    totpSecret?: TotpSecret;
    backupCodes?: string[];
    message: string;
}

export interface MfaValidationResult {
    success: boolean;
    method: 'totp' | 'backup' | 'sms' | 'email';
    message: string;
}

export interface MfaStatus {
    enabled: boolean;
    methods: string[];
    backupCodesRemaining?: number;
}

@injectable()
export class MfaService {

    // Temporary storage for TOTP secrets during setup (use Redis/database in production)
    private static tempSecrets = new Map<string, { secret: string, timestamp: number }>();
    private static readonly TEMP_SECRET_EXPIRY = 5 * 60 * 1000; // 5 minutes

    constructor(
        @inject('TotpService') private _totpService: TotpService
    ) {}

    /**
     * Setup MFA for a user (TOTP)
     * @param userId - User ID
     * @param userIdentifier - User email or username
     * @returns MfaSetupResult with TOTP secret and QR code
     */
    public async setupTotp(userId: uuid, userIdentifier: string): Promise<MfaSetupResult> {
        try {
            // Check if user exists (placeholder - implement based on your user service)
            // const user = await this._getUserById(userId);
            // if (!user) {
            //     throw new ApiError(404, 'User not found');
            // }

            // Check if TOTP is already enabled
            const mfaStatus = await this.getMfaStatus(userId);
            if (mfaStatus.enabled && mfaStatus.methods.includes('totp')) {
                return {
                    success: false,
                    message: 'TOTP is already enabled for this user'
                };
            }

            // Generate TOTP secret
            const totpSecret = await this._totpService.generateSecret(
                userIdentifier, 
                ConfigurationManager.SystemIdentifier
            );

            // Store the secret temporarily (user needs to verify before enabling)
            await this.storeTempTotpSecret(userId, totpSecret.secret);
            
            logger.info(`TOTP secret generated for user: ${userIdentifier}`);

            logger.info(`TOTP setup initiated for user: ${userId}`);

            return {
                success: true,
                totpSecret: totpSecret,
                backupCodes: totpSecret.backupCodes,
                message: 'TOTP setup initiated. Please verify with your authenticator app.'
            };

        } catch (error) {
            logger.error(`Error setting up TOTP for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Verify and enable TOTP for a user
     * @param userId - User ID
     * @param token - TOTP token from user's authenticator app
     * @returns MfaSetupResult
     */
    public async verifyAndEnableTotp(userId: uuid, token: string): Promise<MfaSetupResult> {
        try {
            logger.info(`Attempting TOTP verification for user: ${userId} with token: ${token}`);
            
            // Get temporary TOTP secret
            const tempSecret = await this.getTempTotpSecret(userId);
            if (!tempSecret) {
                logger.warn(`TOTP verification failed - no temp secret found for user: ${userId}`);
                return {
                    success: false,
                    message: 'No active TOTP setup found. Please initiate setup first or your setup may have expired (5 minutes).'
                };
            }
            
            logger.info(`Found temporary TOTP secret for user: ${userId}, attempting verification`);

            // Verify the token
            const validation = this._totpService.verifyToken(token, tempSecret);
            if (!validation.valid) {
                return {
                    success: false,
                    message: 'Invalid TOTP token. Please try again.'
                };
            }

            // Enable TOTP for the user
            await this.enableTotpForUser(userId, tempSecret);

            // Clean up temporary secret
            await this.clearTempTotpSecret(userId);

            logger.info(`TOTP enabled successfully for user: ${userId}`);

            return {
                success: true,
                message: 'TOTP enabled successfully. Your account is now secured with two-factor authentication.'
            };

        } catch (error) {
            logger.error(`Error verifying and enabling TOTP for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Validate MFA token (TOTP or backup code)
     * @param userId - User ID
     * @param token - TOTP token or backup code
     * @param method - Authentication method ('totp' or 'backup')
     * @returns MfaValidationResult
     */
    public async validateMfa(userId: uuid, token: string, method: 'totp' | 'backup' = 'totp'): Promise<MfaValidationResult> {
        try {
            // Check if user exists (placeholder - implement based on your user service)
            // const user = await this._getUserById(userId);
            // if (!user) {
            //     throw new ApiError(404, 'User not found');
            // }

            const mfaStatus = await this.getMfaStatus(userId);
            if (!mfaStatus.enabled) {
                throw new ApiError(400, 'MFA is not enabled for this user');
            }

            if (method === 'totp') {
                return await this.validateTotpToken(userId, token);
            } else if (method === 'backup') {
                return await this.validateBackupCode(userId, token);
            }

            return {
                success: false,
                method: method,
                message: 'Invalid authentication method'
            };

        } catch (error) {
            logger.error(`Error validating MFA for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Disable MFA for a user
     * @param userId - User ID
     * @param currentToken - Current TOTP token for verification
     * @returns boolean
     */
    public async disableMfa(userId: uuid, currentToken: string): Promise<boolean> {
        try {
            // Verify current token before disabling
            const validation = await this.validateMfa(userId, currentToken, 'totp');
            if (!validation.success) {
                throw new ApiError(400, 'Invalid TOTP token. Cannot disable MFA.');
            }

            // Disable MFA
            await this.disableMfaForUser(userId);

            logger.info(`MFA disabled for user: ${userId}`);
            return true;

        } catch (error) {
            logger.error(`Error disabling MFA for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Get MFA status for a user
     * @param userId - User ID
     * @returns MfaStatus
     */
    public async getMfaStatus(userId: uuid): Promise<MfaStatus> {
        try {
            // This would typically query your user metadata or auth profile
            // For now, we'll implement a basic version
            // const user = await this._getUserById(userId);
            // if (!user) {
            //     throw new ApiError(404, 'User not found');
            // }

            // Check if user has TOTP enabled (you'll need to implement this based on your schema)
            const totpEnabled = await this.isTotpEnabledForUser(userId);
            
            const methods: string[] = [];
            if (totpEnabled) {
                methods.push('totp');
            }

            return {
                enabled: methods.length > 0,
                methods: methods,
                backupCodesRemaining: totpEnabled ? await this.getBackupCodesCount(userId) : undefined
            };

        } catch (error) {
            logger.error(`Error getting MFA status for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Generate new backup codes
     * @param userId - User ID
     * @param currentToken - Current TOTP token for verification
     * @returns string[] - New backup codes
     */
    public async generateNewBackupCodes(userId: uuid, currentToken: string): Promise<string[]> {
        try {
            // Verify current token
            const validation = await this.validateMfa(userId, currentToken, 'totp');
            if (!validation.success) {
                throw new ApiError(400, 'Invalid TOTP token. Cannot generate backup codes.');
            }

            // Generate new backup codes
            const backupCodes = this.generateBackupCodes();
            
            // Store backup codes (encrypted)
            await this.storeBackupCodes(userId, backupCodes);

            logger.info(`New backup codes generated for user: ${userId}`);
            return backupCodes;

        } catch (error) {
            logger.error(`Error generating backup codes for user ${userId}: ${error.message}`);
            throw error;
        }
    }

    // Private helper methods

    private async validateTotpToken(userId: uuid, token: string): Promise<MfaValidationResult> {
        try {
            const totpSecret = await this.getTotpSecretForUser(userId);
            if (!totpSecret) {
                return {
                    success: false,
                    method: 'totp',
                    message: 'TOTP not configured for this user'
                };
            }

            const validation = this._totpService.verifyToken(token, totpSecret);
            
            return {
                success: validation.valid,
                method: 'totp',
                message: validation.valid ? 'TOTP token validated successfully' : 'Invalid TOTP token'
            };

        } catch (error) {
            logger.error(`Error validating TOTP token: ${error.message}`);
            return {
                success: false,
                method: 'totp',
                message: 'Error validating TOTP token'
            };
        }
    }

    private async validateBackupCode(userId: uuid, code: string): Promise<MfaValidationResult> {
        try {
            const backupCodes = await this.getBackupCodesForUser(userId);
            if (!backupCodes || backupCodes.length === 0) {
                return {
                    success: false,
                    method: 'backup',
                    message: 'No backup codes available'
                };
            }

            const isValid = this._totpService.verifyBackupCode(code, backupCodes);
            
            if (isValid) {
                // Remove used backup code
                await this.removeUsedBackupCode(userId, code);
            }

            return {
                success: isValid,
                method: 'backup',
                message: isValid ? 'Backup code validated successfully' : 'Invalid backup code'
            };

        } catch (error) {
            logger.error(`Error validating backup code: ${error.message}`);
            return {
                success: false,
                method: 'backup',
                message: 'Error validating backup code'
            };
        }
    }

    private generateBackupCodes(count: number = 10): string[] {
        const codes: string[] = [];
        
        for (let i = 0; i < count; i++) {
            // Generate 8-character alphanumeric code
            const code = Math.random().toString(36).substr(2, 4).toUpperCase() + 
                        Math.random().toString(36).substr(2, 4).toUpperCase();
            codes.push(code);
        }

        return codes;
    }

    // These methods need to be implemented based on your database schema
    private async storeTempTotpSecret(userId: uuid, secret: string): Promise<void> {
        // Store with timestamp for expiry
        MfaService.tempSecrets.set(userId, {
            secret: secret,
            timestamp: Date.now()
        });
        
        // Clean up expired secrets
        this.cleanupExpiredSecrets();
        
        logger.info(`Storing temporary TOTP secret for user: ${userId}`);
    }

    private async getTempTotpSecret(userId: uuid): Promise<string | null> {
        logger.info(`Getting temporary TOTP secret for user: ${userId}`);
        
        const stored = MfaService.tempSecrets.get(userId);
        if (!stored) {
            logger.warn(`No temporary TOTP secret found for user: ${userId}`);
            return null;
        }

        // Check if expired
        if (Date.now() - stored.timestamp > MfaService.TEMP_SECRET_EXPIRY) {
            logger.warn(`Temporary TOTP secret expired for user: ${userId}`);
            MfaService.tempSecrets.delete(userId);
            return null;
        }

        return stored.secret;
    }

    private async clearTempTotpSecret(userId: uuid): Promise<void> {
        MfaService.tempSecrets.delete(userId);
        logger.info(`Clearing temporary TOTP secret for user: ${userId}`);
    }

    private cleanupExpiredSecrets(): void {
        const now = Date.now();
        for (const [userId, data] of MfaService.tempSecrets.entries()) {
            if (now - data.timestamp > MfaService.TEMP_SECRET_EXPIRY) {
                MfaService.tempSecrets.delete(userId);
                logger.info(`Cleaned up expired TOTP secret for user: ${userId}`);
            }
        }
    }

    private async enableTotpForUser(userId: uuid, secret: string): Promise<void> {
        // Implementation depends on your database schema
        // Update user's auth profile to enable TOTP
        logger.info(`Enabling TOTP for user: ${userId}`);
    }

    private async disableMfaForUser(userId: uuid): Promise<void> {
        // Implementation depends on your database schema
        logger.info(`Disabling MFA for user: ${userId}`);
    }

    private async isTotpEnabledForUser(userId: uuid): Promise<boolean> {
        // Implementation depends on your database schema
        return false; // Placeholder
    }

    private async getTotpSecretForUser(userId: uuid): Promise<string | null> {
        // Implementation depends on your database schema
        return null; // Placeholder
    }

    private async getBackupCodesForUser(userId: uuid): Promise<string[]> {
        // Implementation depends on your database schema
        return []; // Placeholder
    }

    private async getBackupCodesCount(userId: uuid): Promise<number> {
        const codes = await this.getBackupCodesForUser(userId);
        return codes.length;
    }

    private async storeBackupCodes(userId: uuid, codes: string[]): Promise<void> {
        // Implementation depends on your database schema
        logger.info(`Storing backup codes for user: ${userId}`);
    }

    private async removeUsedBackupCode(userId: uuid, code: string): Promise<void> {
        // Implementation depends on your database schema
        logger.info(`Removing used backup code for user: ${userId}`);
    }
}
