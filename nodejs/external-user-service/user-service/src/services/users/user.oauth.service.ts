import { injectable, inject } from 'tsyringe';
import axios from 'axios';
import { ApiError } from '../../common/api.error';
import { Helper } from '../../common/helper';
import { logger } from '../../logger/logger';
import { UserService } from './user.service';
import { UserAuthService } from './user.auth.service';
import { UserEvents } from '../../api/users/user.events';
import { UserDto } from '../../domain.types/users/user.types';
import { UserLoginResult } from '../../domain.types/users/user.session.types';
import { 
    GitHubUser, 
    GitHubEmail,
    GitHubOAuthCallbackParams 
} from '../../domain.types/users/github.oauth.types';
import { 
    GoogleUser, 
    GoogleOAuthCallbackParams, 
    GoogleAccessTokenModel 
} from '../../domain.types/users/google.oauth.types';
import { 
    FacebookUser, 
    FacebookOAuthCallbackParams, 
    FacebookAccessTokenModel 
} from '../../domain.types/users/facebook.oauth.types';
import { 
    TwitterUser, 
    TwitterOAuthCallbackParams, 
    TwitterAccessTokenModel 
} from '../../domain.types/users/twitter.oauth.types';

export interface OAuthLoginResponse {
    authUrl: string;
    state: string;
}

export interface OAuthCallbackResponse {
    AccessToken: string;
    RefreshToken: string;
    User: UserDto;
    SessionId: string;
    ExpiresAt: Date;
}

@injectable()
export class UserOAuthService {

    constructor(
        private _userService: UserService,
        private _authService: UserAuthService
    ) {}

    //#region GitHub OAuth

    async githubOAuthLogin(): Promise<OAuthLoginResponse> {
        const clientId = process.env.GITHUB_CLIENT_ID;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/github/callback`;
        const state = Math.random().toString(36).substring(2, 15);
        
        if (!clientId) {
            throw new ApiError(500, 'GitHub OAuth not configured. Please set GITHUB_CLIENT_ID environment variable.');
        }

        if (!baseUrl) {
            throw new ApiError(500, 'BASE_URL environment variable is required.');
        }

        const authUrl = `https://github.com/login/oauth/authorize?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=user:email&state=${state}`;
        
        return {
            authUrl,
            state
        };
    }

    async githubOAuthCallback(params: GitHubOAuthCallbackParams, request: any): Promise<OAuthCallbackResponse> {
        const { code } = params;

        // Get GitHub OAuth configuration
        const clientId = process.env.GITHUB_CLIENT_ID;
        const clientSecret = process.env.GITHUB_CLIENT_SECRET;

        if (!clientId || !clientSecret) {
            throw new ApiError(500, 'GitHub OAuth configuration is missing');
        }

        // Exchange code for access token
        const tokenUrl = `https://github.com/login/oauth/access_token?client_id=${clientId}&client_secret=${clientSecret}&code=${code}`;
        
        const tokenResponse = await axios.post(tokenUrl, '', {
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!tokenResponse.data || !tokenResponse.data.access_token) {
            throw new ApiError(500, 'Unable to retrieve GitHub access token');
        }

        const githubAccessToken = tokenResponse.data.access_token;

        // Get GitHub user information
        const githubUser = await this.getGithubUser(githubAccessToken);
        if (!githubUser) {
            throw new ApiError(500, 'Unable to retrieve GitHub user information');
        }

        // Get user email
        let userEmail = await this.getGithubUserEmail(githubAccessToken);
        if (!userEmail) {
            userEmail = githubUser.email;
            if (!userEmail) {
                userEmail = `${githubUser.login}@github.oauth.user`;
            }
        }

        return await this.handleOAuthUser(userEmail, githubUser.name, githubUser.login, request, 'GitHub');
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

    //#endregion

    //#region Google OAuth

    async googleOAuthLogin(): Promise<OAuthLoginResponse> {
        const clientId = process.env.GOOGLE_CLIENT_ID;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/google/callback`;
        const state = Math.random().toString(36).substring(2, 15);
        
        if (!clientId) {
            throw new ApiError(500, 'Google OAuth not configured. Please set GOOGLE_CLIENT_ID environment variable.');
        }

        if (!baseUrl) {
            throw new ApiError(500, 'BASE_URL environment variable is required.');
        }

        const authUrl = `https://accounts.google.com/o/oauth2/auth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=openid%20email%20profile&response_type=code&state=${state}`;

        return {
            authUrl,
            state
        };
    }

    async googleOAuthCallback(params: GoogleOAuthCallbackParams, request: any): Promise<OAuthCallbackResponse> {
        const { code } = params;

        const clientId = process.env.GOOGLE_CLIENT_ID;
        const clientSecret = process.env.GOOGLE_CLIENT_SECRET;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/google/callback`;

        if (!clientId || !clientSecret) {
            throw new ApiError(500, 'Google OAuth not configured');
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
            throw new ApiError(500, 'Unable to retrieve Google access token');
        }

        const tokenData: GoogleAccessTokenModel = tokenResponse.data;
        const googleAccessToken = tokenData.access_token;

        // Get user information from Google
        const googleUser = await this.getGoogleUser(googleAccessToken);
        if (!googleUser) {
            throw new ApiError(500, 'Unable to retrieve Google user information');
        }

        const userEmail = googleUser.email;
        if (!userEmail) {
            throw new ApiError(400, 'Google user email is required');
        }

        const fullName = `${googleUser.given_name || ''} ${googleUser.family_name || ''}`.trim();
        const userName = googleUser.email.split('@')[0]; // Use email prefix as username

        return await this.handleOAuthUser(userEmail, fullName, userName, request, 'Google');
    }

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

    //#endregion

    //#region Facebook OAuth

    async facebookOAuthLogin(): Promise<OAuthLoginResponse> {
        const clientId = process.env.FACEBOOK_CLIENT_ID;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/facebook/callback`;
        const state = Math.random().toString(36).substring(2, 15);
        
        if (!clientId) {
            throw new ApiError(500, 'Facebook OAuth not configured. Please set FACEBOOK_CLIENT_ID environment variable.');
        }

        if (!baseUrl) {
            throw new ApiError(500, 'BASE_URL environment variable is required.');
        }

        const authUrl = `https://www.facebook.com/v18.0/dialog/oauth?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=email,public_profile&response_type=code&state=${state}`;

        return {
            authUrl,
            state
        };
    }

    async facebookOAuthCallback(params: FacebookOAuthCallbackParams, request: any): Promise<OAuthCallbackResponse> {
        const { code } = params;

        const clientId = process.env.FACEBOOK_CLIENT_ID;
        const clientSecret = process.env.FACEBOOK_CLIENT_SECRET;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/facebook/callback`;

        if (!clientId || !clientSecret) {
            throw new ApiError(500, 'Facebook OAuth not configured');
        }

        // Exchange code for access token
        const tokenUrl = `https://graph.facebook.com/v18.0/oauth/access_token?client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&client_secret=${clientSecret}&code=${code}`;
        
        const tokenResponse = await axios.get(tokenUrl);

        if (tokenResponse.status !== 200 || !tokenResponse.data.access_token) {
            throw new ApiError(500, 'Unable to retrieve Facebook access token');
        }

        const tokenData: FacebookAccessTokenModel = tokenResponse.data;
        const facebookAccessToken = tokenData.access_token;

        // Get user information from Facebook
        const facebookUser = await this.getFacebookUser(facebookAccessToken);
        if (!facebookUser) {
            throw new ApiError(500, 'Unable to retrieve Facebook user information');
        }

        const userEmail = facebookUser.email;
        if (!userEmail) {
            throw new ApiError(400, 'Facebook user email is required. Please ensure email permission is granted.');
        }

        const fullName = `${facebookUser.first_name || ''} ${facebookUser.last_name || ''}`.trim();
        const userName = userEmail.split('@')[0]; // Use email prefix as username

        return await this.handleOAuthUser(userEmail, fullName, userName, request, 'Facebook');
    }

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

    //#endregion

    //#region Twitter OAuth

    async twitterOAuthLogin(): Promise<OAuthLoginResponse> {
        const clientId = process.env.TWITTER_CLIENT_ID;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/twitter/callback`;
        const state = Math.random().toString(36).substring(2, 15);
        
        if (!clientId) {
            throw new ApiError(500, 'Twitter OAuth not configured. Please set TWITTER_CLIENT_ID environment variable.');
        }

        if (!baseUrl) {
            throw new ApiError(500, 'BASE_URL environment variable is required.');
        }

        const authUrl = `https://twitter.com/i/oauth2/authorize?response_type=code&client_id=${clientId}&redirect_uri=${encodeURIComponent(redirectUri)}&scope=users.read%20tweet.read%20offline.access&state=${state}&code_challenge=challenge&code_challenge_method=plain`;

        return {
            authUrl,
            state
        };
    }

    async twitterOAuthCallback(params: TwitterOAuthCallbackParams, request: any): Promise<OAuthCallbackResponse> {
        const { code } = params;

        const clientId = process.env.TWITTER_CLIENT_ID;
        const clientSecret = process.env.TWITTER_CLIENT_SECRET;
        const baseUrl = process.env.BASE_URL;
        const redirectUri = `${baseUrl}/api/v1/auth/oauth/twitter/callback`;

        if (!clientId || !clientSecret) {
            throw new ApiError(500, 'Twitter OAuth not configured');
        }

        // Exchange code for access token
        const tokenUrl = 'https://api.twitter.com/2/oauth2/token';
        const tokenResponse = await axios.post(tokenUrl, new URLSearchParams({
            code: code,
            grant_type: 'authorization_code',
            client_id: clientId,
            redirect_uri: redirectUri,
            code_verifier: 'challenge'
        }), {
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'Authorization': `Basic ${Buffer.from(`${clientId}:${clientSecret}`).toString('base64')}`
            }
        });

        if (tokenResponse.status !== 200 || !tokenResponse.data.access_token) {
            throw new ApiError(500, 'Unable to retrieve Twitter access token');
        }

        const tokenData: TwitterAccessTokenModel = tokenResponse.data;
        const twitterAccessToken = tokenData.access_token;

        // Get user information from Twitter
        const twitterUser = await this.getTwitterUser(twitterAccessToken);
        if (!twitterUser) {
            throw new ApiError(500, 'Unable to retrieve Twitter user information');
        }

        let userEmail = twitterUser.email;
        if (!userEmail) {
            // Twitter doesn't always provide email, create a placeholder
            userEmail = `${twitterUser.username}@twitter.oauth.user`;
        }

        const fullName = twitterUser.name || '';
        const userName = twitterUser.username;

        return await this.handleOAuthUser(userEmail, fullName, userName, request, 'Twitter');
    }

    private async getTwitterUser(accessToken: string): Promise<TwitterUser | null> {
        try {
            // Try to get user info with email first
            let response = await axios.get('https://api.twitter.com/2/users/me?user.fields=id,name,username,email,profile_image_url,verified,description,location,url,created_at,public_metrics', {
                headers: {
                    'Authorization': `Bearer ${accessToken}`,
                    'User-Agent': 'User-Service-OAuth'
                }
            }).catch(async (emailError) => {
                console.log('Email access not available, trying without email:', emailError.response?.status);
                // If email access fails, try without email field
                return await axios.get('https://api.twitter.com/2/users/me?user.fields=id,name,username,profile_image_url,verified,description,location,url,created_at,public_metrics', {
                    headers: {
                        'Authorization': `Bearer ${accessToken}`,
                        'User-Agent': 'User-Service-OAuth'
                    }
                });
            });

            if (response.status === 200 && response.data.data) {
                console.log('Twitter User Response:', JSON.stringify(response.data, null, 2));
                return response.data.data as TwitterUser;
            }
            
            console.log('Twitter API Error - Status:', response.status);
            console.log('Twitter API Error - Data:', response.data);
            return null;
        } catch (error) {
            console.error('Error fetching Twitter user:', error.response?.data || error.message);
            console.error('Twitter API Status:', error.response?.status);
            return null;
        }
    }

    //#endregion

    //#region Common OAuth Helper Methods

    private async handleOAuthUser(
        email: string, 
        fullName: string, 
        userName: string, 
        request: any, 
        provider: string
    ): Promise<OAuthCallbackResponse> {
        // Check if user exists
        const existingUser = await this._userService.getByEmail(null, email);
        
        if (existingUser) {
            // User exists, log them in
            const loginResult = await this._authService.loginWithOAuth(existingUser.id);
            if (!loginResult) {
                throw new ApiError(500, 'Session cannot be created');
            }

            UserEvents.onUserLoginWithOAuth(request, existingUser);
            
            return {
                AccessToken: loginResult.AccessToken,
                RefreshToken: loginResult.RefreshToken,
                User: existingUser,
                SessionId: loginResult.SessionId,
                ExpiresAt: loginResult.ExpiresAt
            };
        } else {
            // Create new user
            const nameTokens = fullName?.split(' ') || [];
            const firstName = nameTokens[0] || '';
            const lastName = nameTokens.length > 1 ? nameTokens.slice(1).join(' ') : '';

            // Get default tenant for user creation
            const tenant = await this._authService['_tenantRepo'].getTenantWithCode('default');
            if (!tenant) {
                throw new ApiError(500, 'Default tenant not found. Please contact system administrator.');
            }
            const tenantId = tenant.id;

            const createModel = {
                TenantId: tenantId,
                FirstName: firstName,
                LastName: lastName,
                Email: email,
                UserName: userName,
                Password: Helper.generatePassword(),
            };

            const createdUser = await this._userService.create(createModel);
            if (!createdUser) {
                throw new ApiError(500, 'Unable to create a new user');
            }

            const loginResult = await this._authService.loginWithOAuth(createdUser.id);
            if (!loginResult) {
                throw new ApiError(500, 'Session cannot be created');
            }

            UserEvents.onUserLoginWithOAuth(request, createdUser);

            return {
                AccessToken: loginResult.AccessToken,
                RefreshToken: loginResult.RefreshToken,
                User: createdUser,
                SessionId: loginResult.SessionId,
                ExpiresAt: loginResult.ExpiresAt
            };
        }
    }

    //#endregion
}
