import { BaseSearchFilters, BaseSearchResults } from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";

///////////////////////////////////////////////////////////////////////////////////

export interface TenantCreateModel {
    Name        : string;
    Description?: string;
    TenantCode  : string;
    PhoneCode  ?: string;
    PhoneNumber?: string;
    Email      ?: string;
    UserName   ?: string;
    Password   ?: string;
}

export interface TenantUpdateModel {
    id         : uuid;
    Name       ?: string;
    Description?: string;
    TenantCode  : string;
    PhoneCode  ?: string;
    PhoneNumber?: string;
    Email      ?: string;
}

export interface TenantDto {
    id         ?: uuid;
    Name        : string;
    Description?: string;
    TenantCode  : string;
    PhoneCode  ?: string;
    PhoneNumber?: string;
    Email      ?: string;
}

export interface TenantSearchFilters extends BaseSearchFilters {
    Name        ?: string;
    TenantCode  ?: string;
    PhoneCode   ?: string;
    PhoneNumber ?: string;
    Email       ?: string;
}

export interface TenantSearchResults extends BaseSearchResults {
    Items: TenantDto[];
}
