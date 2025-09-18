import express from 'express';
import { MfaController } from './mfa.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { ActionScope, ResourceOwnership } from '../../../auth/user.auth/auth.types';

////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new MfaController();
    const router = express.Router();

    const authOptions = {
        Context: 'Users.MFA',
        ActionScope: ActionScope.Owner,
        Ownership: ResourceOwnership.Owner,
        ClientAppAuth: false
    };

    // Setup TOTP for the current user
    router.post('/setup-totp', auth(authOptions), controller.setupTotp);

    // Verify and enable TOTP setup
    router.post('/verify-totp-setup', auth(authOptions), controller.verifyTotpSetup);

    // Get QR code for TOTP setup (returns image)
    router.get('/qr-code', auth(authOptions), controller.getQrCode);

    // Validate MFA token (TOTP or backup code)
    router.post('/validate', auth(authOptions), controller.validateMfa);

    // Get MFA status for current user
    router.get('/status', auth(authOptions), controller.getMfaStatus);

    // Disable MFA for current user
    router.post('/disable', auth(authOptions), controller.disableMfa);

    // Generate new backup codes
    router.post('/generate-backup-codes', auth(authOptions), controller.generateBackupCodes);

    app.use('/api/v1/users/mfa', router);
};
