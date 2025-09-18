export interface FacebookAccessTokenModel {
    access_token: string;
    token_type: string;
    expires_in?: number;
}

export interface FacebookUser {
    id: string;
    name: string;
    first_name: string;
    last_name: string;
    email?: string;
    picture?: {
        data: {
            height: number;
            is_silhouette: boolean;
            url: string;
            width: number;
        };
    };
    locale?: string;
    timezone?: number;
    verified?: boolean;
}

export interface FacebookOAuthCallbackParams {
    code: string;
    state?: string;
}
