import {
    BaseSearchFilters,
    BaseSearchResults
} from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";
import { PermissionDto } from "./permission.types";
import { RoleDto } from "./role.types";

/////////////////////////////////////////////////////////////////

export interface RolePermissionCreateModel {
    RoleId    : uuid;
    PermissionId: uuid;
    Scope     : string;
    Enabled   : boolean;
}

export interface RolePermissionDto {
    id             : uuid,
    RoleId         : uuid;
    PermissionId   : uuid;
    Role           ?: RoleDto;
    Permission     ?: PermissionDto;
    Scope          : string;
    Enabled        : boolean;
}

export interface RolePermissionSearchFilters extends BaseSearchFilters {
    RoleId  : uuid;
    Scope   : string;
    Enabled : boolean;
}

export interface RolePermissionSearchResults extends BaseSearchResults {
    Items: RolePermissionDto[];
}
