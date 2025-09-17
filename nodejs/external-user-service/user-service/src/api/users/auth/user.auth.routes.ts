import express from 'express';
import { UserAuthController } from './user.auth.controller';
import { auth } from '../../../auth/user.auth/user.auth.handler';
import { AuthAuth } from './auth.auth';

////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new UserAuthController();

    const router = express.Router();

    router.post('/login-with-email-password', auth(AuthAuth.loginWithEmailPassword), controller.loginWithEmailPassword);
    router.post('/login-with-phone-password', auth(AuthAuth.loginWithPhonePassword), controller.loginWithPhonePassword);
    router.post('/login-with-username-password', auth(AuthAuth.loginWithUsernamePassword), controller.loginWithUsernamePassword);
    router.post('/login-with-email-otp', auth(AuthAuth.loginWithEmailOtp), controller.loginWithEmailOtp);
    router.post('/login-with-phone-otp', auth(AuthAuth.loginWithPhoneOtp), controller.loginWithPhoneOtp);

    router.post('/generate-email-otp', auth(AuthAuth.generateEmailOtp), controller.generateEmailOtp);
    router.post('/generate-phone-otp', auth(AuthAuth.generatePhoneOtp), controller.generatePhoneOtp);

    router.post('/change-password', auth(AuthAuth.changePassword), controller.changePassword);
    router.post('/reset-password-by-token', auth(AuthAuth.resetPasswordByToken), controller.resetPasswordByToken);
    router.post('/reset-password-by-otp', auth(AuthAuth.resetPasswordByOtp), controller.resetPasswordByOtp);
    router.post('/send-reset-password-email-link', auth(AuthAuth.sendResetPasswordByEmailLink), controller.sendResetPasswordByEmailLink);
    router.post('/send-reset-password-phone-otp', auth(AuthAuth.sendResetPasswordByPhoneOtp), controller.sendResetPasswordByPhoneOtp);
    router.post('/send-reset-password-email-otp', auth(AuthAuth.sendResetPasswordByEmailOtp), controller.sendResetPasswordByEmailOtp);

    router.post('/logout', auth(AuthAuth.logout), controller.logout);
    router.post('/access-token/:refreshToken', auth(AuthAuth.rotateUserAccessToken), controller.rotateUserAccessToken);

    router.get('/oauth/github/login', auth(AuthAuth.githubOAuthLogin), controller.githubOAuthLogin);
    router.get('/oauth/github/callback', auth(AuthAuth.githubOAuthCallback), controller.githubOAuthCallback);
    
    router.get('/oauth/google/login', auth(AuthAuth.googleOAuthLogin), controller.googleOAuthLogin);
    router.get('/oauth/google/callback', auth(AuthAuth.googleOAuthCallback), controller.googleOAuthCallback);

    app.use('/api/v1/auth', router);

};
