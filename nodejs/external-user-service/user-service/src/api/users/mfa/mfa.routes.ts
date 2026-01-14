import express from 'express';
import { MfaController } from './mfa.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { MfaAuth } from './mfa.auth';

////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new MfaController();
    const router = express.Router();

    // Setup TOTP for the current user
    router.post('/setup-totp', auth(MfaAuth.setupTotp), controller.setupTotp);

    // Verify and enable TOTP setup
    router.post('/verify-totp-setup', auth(MfaAuth.verifyTotpSetup), controller.verifyTotpSetup);

    // Get QR code for TOTP setup (returns image)
    router.get('/qr-code', auth(MfaAuth.getQrCode), controller.getQrCode);

    // Validate MFA token (TOTP or backup code)
    router.post('/validate', auth(MfaAuth.validateMfa), controller.validateMfa);

    // Get MFA status for current user
    router.get('/status', auth(MfaAuth.getMfaStatus), controller.getMfaStatus);

    // Disable MFA for current user
    router.post('/disable', auth(MfaAuth.disableMfa), controller.disableMfa);

    // Generate new backup codes
    router.post('/generate-backup-codes', auth(MfaAuth.generateBackupCodes), controller.generateBackupCodes);

    app.use('/api/v1/users/mfa', router);
};
