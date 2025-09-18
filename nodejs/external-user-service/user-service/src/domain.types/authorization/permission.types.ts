import { BaseSearchFilters, BaseSearchResults } from "../miscellaneous/base.search.types";
import { uuid } from "../miscellaneous/system.types";

/////////////////////////////////////////////////////////////////

export interface PermissionCreateModel {
  Name         : string;
  Description  : string;
  TenantId     : uuid;
  Module      ?: string; // Optional field for module association
}

export interface PermissionUpdateModel {
  id          ?: uuid;
  Name        ?: string;
  Description ?: string;
  TenantId    ?: uuid;
  Module      ?: string;
}

export interface PermissionSearchFilters {
  Name        ?: string;
  TenantId    ?: uuid;
  Module      ?: string;
}

export interface PermissionDto {
  id          : uuid;
  Name        : string;
  Description : string;
  TenantId    : uuid;
  Module      ?: string;
}

export interface PermissionSearchFilters extends BaseSearchFilters {
  Name        ?: string;
  TenantId    ?: uuid;
  Module      ?: string;
}

export interface PermissionSearchResults extends BaseSearchResults {
  Items: PermissionDto[];
}
