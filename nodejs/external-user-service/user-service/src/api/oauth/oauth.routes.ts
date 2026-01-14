import express from 'express';
import { OAuthController } from './oauth.controller';
import { auth } from '../../auth/user.auth/user.auth.handler';
import { OAuthAuth } from './oauth.auth';

////////////////////////////////////////////////////////////////////////////////

export const register = (app: express.Application): void => {

    const controller = new OAuthController();

    const router = express.Router();

    //#region GitHub OAuth Routes

    router.get('/github/login', auth(OAuthAuth.githubOAuthLogin), controller.githubOAuthLogin);
    router.get('/github/callback', auth(OAuthAuth.githubOAuthCallback), controller.githubOAuthCallback);

    //#endregion

    //#region Google OAuth Routes

    router.get('/google/login', auth(OAuthAuth.googleOAuthLogin), controller.googleOAuthLogin);
    router.get('/google/callback', auth(OAuthAuth.googleOAuthCallback), controller.googleOAuthCallback);

    //#endregion

    //#region Facebook OAuth Routes

    router.get('/facebook/login', auth(OAuthAuth.facebookOAuthLogin), controller.facebookOAuthLogin);
    router.get('/facebook/callback', auth(OAuthAuth.facebookOAuthCallback), controller.facebookOAuthCallback);

    //#endregion

    //#region Twitter OAuth Routes

    router.get('/twitter/login', auth(OAuthAuth.twitterOAuthLogin), controller.twitterOAuthLogin);
    router.get('/twitter/callback', auth(OAuthAuth.twitterOAuthCallback), controller.twitterOAuthCallback);

    //#endregion

    app.use('/api/v1/oauth', router);

};
