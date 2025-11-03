import { BaseSearchFilters, BaseSearchResults } from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";
import { DefaultRoleTypes, PermissionScope } from "./enums";

/////////////////////////////////////////////////////////////////

export const DefaultRoles = [
    {
        Role         : DefaultRoleTypes.SystemAdmin,
        Description  : 'Admin of the system having elevated super-admin privileges.',
        IsSystemRole : true,
        IsUserRole   : true,
        SeederFile   : 'permissions.system.admin.json',
        Scope        : PermissionScope.System,
    },
    {
        Role         : DefaultRoleTypes.SystemUser,
        Description  : 'User of the system having elevated privileges in select areas. Can manage users and resources within the system.',
        IsSystemRole : true,
        IsUserRole   : true,
        SeederFile   : 'permissions.system.user.json',
        Scope        : PermissionScope.System,
    },
    {
        Role         : DefaultRoleTypes.User,
        Description  : 'A basic user role. Can edit their own profile and view basic information.',
        IsSystemRole : false,
        IsUserRole   : true,
        SeederFile   : 'permissions.base.user.json',
        Scope        : PermissionScope.User,
    }

];

/////////////////////////////////////////////////////////////////

export interface RoleCreateModel {
  Name         : string;
  Description  : string;
  TenantId     : uuid;
  IsSystemRole : boolean;
  ParentRoleId?: uuid;
}

export interface RoleUpdateModel {
  id          ?: uuid;
  Name        ?: string;
  Description ?: string;
  TenantId    ?: uuid;
  ParentRoleId?: uuid;
  IsSystemRole?: boolean;
}

export interface RoleSearchFilters {
  Name        ?: string;
  TenantId    ?: uuid;
  ParentRoleId?: uuid;
  IsSystemRole?: boolean;
}

export interface RoleDto {
  id          : uuid;
  Name        : string;
  Description : string;
  TenantId    : uuid;
  ParentRoleId: uuid;
  IsSystemRole: boolean;
}

export interface RoleSearchFilters extends BaseSearchFilters {
  Name        ?: string;
  TenantId    ?: uuid;
  ParentRoleId?: uuid;
  IsSystemRole?: boolean;
}

export interface RoleSearchResults extends BaseSearchResults {
  Items: RoleDto[];
}
