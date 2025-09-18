import {
    BaseSearchFilters,
    BaseSearchResults
} from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";
import { PermissionDto } from "./permission.types";

/////////////////////////////////////////////////////////////////

export interface UserPermissionCreateModel {
    UserId      : uuid;
    PermissionId: uuid;
    Scope       : string;
    Granted     : boolean;
}

export interface UserPermissionDto {
    id           : uuid,
    UserId       : uuid;
    PermissionId : uuid;
    Permission  ?: PermissionDto;
    Scope        : string;
    Granted      : boolean;
}

export interface UserPermissionSearchFilters extends BaseSearchFilters {
    UserId : uuid;
    Scope  : string;
    Granted: boolean;
}

export interface UserPermissionSearchResults extends BaseSearchResults {
    Items: UserPermissionDto[];
}
