import { BaseSearchFilters, BaseSearchResults } from '../miscellaneous/base.search.types';
import { uuid, Gender } from '../miscellaneous/system.types';
import { SupportedLanguage } from './user.enums';

///////////////////////////////////////////////////////////////////////////////////////

export interface UserCreateModel {
    TenantId          : uuid;
    UserName         ?: string;
    Prefix           ?: string;
    FirstName        ?: string;
    MiddleName       ?: string;
    LastName         ?: string;
    PhoneCode        ?: string;
    PhoneNumber      ?: string;
    Email            ?: string;
    Gender           ?: Gender;
    DateOfBirth      ?: Date;
    ProfileImageUrl  ?: string;
    Password         ?: string;
    CurrentTimeZone  ?: string;
    DefaultTimeZone  ?: string;
    IsTestUser       ?: boolean;
    PreferredLanguage?: SupportedLanguage;
    UserSettings     ?: any;
    RoleIds          ?: uuid[];
}

export interface UserUpdateModel {
    id             ?: uuid;
    UserName       ?: string;
    Prefix         ?: string;
    FirstName      ?: string;
    MiddleName     ?: string;
    LastName       ?: string;
    PhoneCode      ?: string;
    PhoneNumber    ?: string;
    Email          ?: string;
    Gender         ?: Gender;
    DateOfBirth    ?: Date;
    ProfileImageUrl?: string;
}

export interface UserMetadataModel {
    id               ?: uuid;
    DisplayName      ?: string;
    DefaultTimeZone  ?: string;
    CurrentTimeZone  ?: string;
    IsTestUser       ?: boolean;
    PreferredLanguage?: SupportedLanguage;
    LastLogin        ?: Date;
    UserSettings     ?: any;
}

export interface UserDto {
    id                : uuid;
    Tenant            : {
        id             : uuid;
        TenantCode     : string;
        TenantName     : string;
    };
    UserName         ?: string;
    Prefix           ?: string;
    FirstName        ?: string;
    MiddleName       ?: string;
    LastName         ?: string;
    PhoneCode        ?: string;
    PhoneNumber      ?: string;
    Email            ?: string;
    Gender           ?: Gender;
    DateOfBirth      ?: Date;
    ProfileImageUrl  ?: string;
    Metadata         ?: UserMetadataModel;
    Roles            ?: {
        id             : uuid;
        Name           : string;
    }[];
    CreatedAt        ?: Date;
    UpdatedAt        ?: Date;
}

export interface UserSearchFilters extends BaseSearchFilters {
    TenantId   ?: uuid;
    PhoneNumber?: string;
    Email      ?: string;
    UserId     ?: uuid;
    Name       ?: string;
    Gender     ?: Gender;
    UserName   ?: string;
}

export interface UserSearchResults extends BaseSearchResults {
    Items: UserDto[];
}

///////////////////////////////////////////////////////////////////////////////////////

export interface UserUniqueIdentifiers {
    UserId     ?: uuid;
    PhoneNumber?: string,
    PhoneCode  ?: string,
    Email      ?: string,
    UserName   ?: string;
    TenantId   ?: uuid,
}
////////////////////////////////////////////////////////////////

export interface UserAccountActionResult {
    Success: boolean;
    User: UserDto;
}
