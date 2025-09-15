import { OAuthProvider } from './user.enums';
import { uuid } from '../miscellaneous/system.types';
import { BaseSearchFilters, BaseSearchResults } from '../miscellaneous/base.search.types';

///////////////////////////////////////////////////////////////////////////////////////

export interface UserOAuthDomainModel {
    id          ?: uuid;
    UserId      : uuid;
    Provider    : OAuthProvider;
    AccessToken ?: string;
    SessionId   ?: string;
    Claims      ?: string;
    Metadata    ?: string;
    ValidFrom   : Date;
    ValidTill   : Date;
    IsExpired   : boolean;
    RefreshToken?: string;
    CreatedAt   ?: Date;
    UpdatedAt   ?: Date;
}

export interface UserOAuthDto {
    id          ?: uuid;
    UserId      : uuid;
    Provider    : OAuthProvider;
    AccessToken ?: string;
    SessionId   ?: string;
    Claims      ?: any;
    Metadata    ?: any;
    ValidFrom   : Date;
    ValidTill   : Date;
    IsExpired   : boolean;
    RefreshToken?: string;
    CreatedAt   ?: Date;
    UpdatedAt   ?: Date;
}

export interface UserOAuthCreateModel {
    UserId      : uuid;
    Provider    : OAuthProvider;
    AccessToken ?: string;
    SessionId   ?: string;
    Claims      ?: string | object;
    Metadata    ?: string | object;
    ValidFrom   : Date;
    ValidTill   : Date;
    RefreshToken?: string;
}

export interface UserOAuthUpdateModel {
    AccessToken ?: string;
    SessionId   ?: string;
    Claims      ?: string | object;
    Metadata    ?: string | object;
    ValidFrom   ?: Date;
    ValidTill   ?: Date;
    IsExpired   ?: boolean;
    RefreshToken?: string;
}

export interface UserOAuthSearchFilters extends BaseSearchFilters {
    UserId   ?: uuid;
    Provider ?: OAuthProvider;
    IsExpired?: boolean;
    FromDate ?: Date;
    ToDate   ?: Date;
}

export interface UserOAuthSearchResults extends BaseSearchResults {
    Items: UserOAuthDto[];
}
