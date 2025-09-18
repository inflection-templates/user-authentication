import { BaseSearchFilters, BaseSearchResults } from "../miscellaneous/base.search.types";

///////////////////////////////////////////////////////////////////////

export interface ClientAppCreateModel {
    id?: string;
    ClientName: string;
    ClientCode?: string;
    IsPrivileged?: boolean;
    PhoneCode?: string;
    PhoneNumber?: string;
    Email?: string;
    Password?: string;
    ApiKey?: string;
    ValidFrom?: Date;
    ValidTill?: Date;
}

export interface ClientAppVerificationModel {
    ClientCode: string;
    Password: string;
    ValidFrom: Date;
    ValidTill: Date;
}

export interface ClientAppUpdateModel {
    ClientName?: string;
    ClientCode?: string;
    IsPrivileged?: boolean;
    PhoneCode?: string;
    PhoneNumber?: string;
    Email?: string;
    ApiKey?: string;
    ValidFrom?: Date;
    ValidTill?: Date;
}

export interface ClientAppDto {
    id          : string;
    ClientName  : string;
    ClientCode  : string;
    IsPrivileged: boolean;
    PhoneCode   : string;
    PhoneNumber : string;
    Email       : string;
    Active      : boolean;
}

export interface ClientAppSearchFilters extends BaseSearchFilters {
    ClientName?: string;
    ClientCode?: string;
    IsPrivileged?: boolean;
}

export interface ClientAppSearchResults extends BaseSearchResults {
    Items: ClientAppDto[];
}

export interface ApiKeyDto {
    id: string;
    ClientName: string;
    ClientCode: string;
    ApiKey: string;
    ValidFrom: Date;
    ValidTill: Date;
}
