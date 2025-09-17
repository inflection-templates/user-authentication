export interface GitHubAccessTokenModel {
    access_token: string;
    token_type: string;
    scope: string;
}

export interface GitHubUser {
    id: number;
    login: string;
    name?: string;
    email?: string;
    avatar_url?: string;
    html_url?: string;
    company?: string;
    location?: string;
    bio?: string;
    public_repos?: number;
    followers?: number;
    following?: number;
    created_at?: string;
    updated_at?: string;
}

export interface GitHubEmail {
    email: string;
    primary: boolean;
    verified: boolean;
    visibility?: string;
}

export interface GitHubOAuthCallbackParams {
    code: string;
    state?: string;
}
