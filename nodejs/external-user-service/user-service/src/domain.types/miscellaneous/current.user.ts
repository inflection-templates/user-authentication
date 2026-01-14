import { uuid } from "./system.types";

export interface CurrentUser {
    UserId      : string;
    TenantId    : string;
    TenantCode  : string;
    TenantName  : string;
    DisplayName : string;
    PhoneCode   : string;
    PhoneNumber : string;
    Email       : string;
    UserName    : string;
    SessionId  ?: uuid;
    IsTestUser ?: boolean;
    Roles       : any[];
}
