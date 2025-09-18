import * as speakeasy from 'speakeasy';
import * as qrcode from 'qrcode';
import { logger } from '../../logger/logger';
import { ConfigurationManager } from '../../config/configuration.manager';

export interface TotpSecret {
    secret: string;
    qrCodeUrl: string;
    qrCodeDataUrl: string;
    backupCodes?: string[];
}

export interface TotpValidationResult {
    valid: boolean;
    token?: string;
    window?: number;
}

export class TotpService {

    /**
     * Generate a new TOTP secret for a user
     * @param userIdentifier - User email or username for QR code label
     * @param issuer - Service name (e.g., "Your App Name")
     * @returns TotpSecret object with secret, QR code URL and data URL
     */
    public async generateSecret(userIdentifier: string, issuer?: string): Promise<TotpSecret> {
        try {
            const serviceName = issuer || ConfigurationManager.SystemIdentifier || 'User Service';
            
            // Generate secret
            const secret = speakeasy.generateSecret({
                name: `${serviceName} (${userIdentifier})`,
                issuer: serviceName,
                length: 32
            });

            // Generate QR code data URL
            const qrCodeDataUrl = await qrcode.toDataURL(secret.otpauth_url!);

            // Generate backup codes (optional)
            const backupCodes = this.generateBackupCodes();

            const result: TotpSecret = {
                secret: secret.base32!,
                qrCodeUrl: secret.otpauth_url!,
                qrCodeDataUrl: qrCodeDataUrl,
                backupCodes: backupCodes
            };

            logger.info(`TOTP secret generated for user: ${userIdentifier}`);
            return result;

        } catch (error) {
            logger.error(`Error generating TOTP secret for ${userIdentifier}: ${error.message}`);
            throw error;
        }
    }

    /**
     * Verify TOTP token
     * @param token - 6-digit TOTP token from user
     * @param secret - User's TOTP secret (base32 encoded)
     * @param window - Time window for validation (default: 2 = ±60 seconds)
     * @returns TotpValidationResult
     */
    public verifyToken(token: string, secret: string, window: number = 2): TotpValidationResult {
        try {
            const verified = speakeasy.totp.verify({
                secret: secret,
                encoding: 'base32',
                token: token,
                window: window,
                time: Math.floor(Date.now() / 1000)
            });

            return {
                valid: verified,
                token: verified ? token : undefined,
                window: window
            };

        } catch (error) {
            logger.error(`Error verifying TOTP token: ${error.message}`);
            return {
                valid: false,
                window: window
            };
        }
    }

    /**
     * Generate current TOTP token for testing purposes
     * @param secret - TOTP secret
     * @returns Current 6-digit token
     */
    public generateCurrentToken(secret: string): string {
        try {
            return speakeasy.totp({
                secret: secret,
                encoding: 'base32',
                time: Math.floor(Date.now() / 1000)
            });
        } catch (error) {
            logger.error(`Error generating current TOTP token: ${error.message}`);
            throw error;
        }
    }

    /**
     * Verify backup code
     * @param code - Backup code provided by user
     * @param backupCodes - Array of valid backup codes
     * @returns boolean indicating if code is valid
     */
    public verifyBackupCode(code: string, backupCodes: string[]): boolean {
        try {
            const normalizedCode = code.replace(/\s+/g, '').toLowerCase();
            return backupCodes.some(backupCode => 
                backupCode.replace(/\s+/g, '').toLowerCase() === normalizedCode
            );
        } catch (error) {
            logger.error(`Error verifying backup code: ${error.message}`);
            return false;
        }
    }

    /**
     * Generate backup codes for account recovery
     * @param count - Number of backup codes to generate
     * @returns Array of backup codes
     */
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

    /**
     * Get remaining time until next TOTP token
     * @returns Remaining seconds until next token
     */
    public getRemainingTime(): number {
        const now = Math.floor(Date.now() / 1000);
        const timeStep = 30; // TOTP time step is 30 seconds
        return timeStep - (now % timeStep);
    }

    /**
     * Validate TOTP setup by requiring user to enter current token
     * @param token - Token entered by user
     * @param secret - Generated secret
     * @returns boolean indicating successful setup
     */
    public validateSetup(token: string, secret: string): boolean {
        const result = this.verifyToken(token, secret, 1); // Smaller window for setup
        return result.valid;
    }
}
