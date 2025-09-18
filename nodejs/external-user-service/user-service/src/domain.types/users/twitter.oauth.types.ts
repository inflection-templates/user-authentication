export interface TwitterAccessTokenModel {
    access_token: string;
    token_type: string;
    expires_in?: number;
    refresh_token?: string;
    scope?: string;
}

export interface TwitterUser {
    id: string;
    name: string;
    username: string;
    email?: string;
    profile_image_url?: string;
    verified?: boolean;
    description?: string;
    location?: string;
    url?: string;
    created_at?: string;
    public_metrics?: {
        followers_count: number;
        following_count: number;
        tweet_count: number;
        listed_count: number;
    };
}

export interface TwitterOAuthCallbackParams {
    code: string;
    state?: string;
}
