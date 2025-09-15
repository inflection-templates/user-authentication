import { uuid } from "../../../domain.types/miscellaneous/system.types";
import {
    RolePermissionDto,
    RolePermissionCreateModel,
    RolePermissionSearchFilters,
    RolePermissionSearchResults,
} from "../../../domain.types/authorization/role.permission.types";

/////////////////////////////////////////////////////////////////////////////////////////////

export interface IRolePermissionRepo {

    create(entity: RolePermissionCreateModel): Promise<RolePermissionDto>;

    getById(id: uuid): Promise<RolePermissionDto>;

    search(filters: RolePermissionSearchFilters): Promise<RolePermissionSearchResults>;

    getPermissionsForRole (roleId: uuid): Promise<RolePermissionDto[]>;

    hasPermissionForRole (roleId: uuid, PermissionId: uuid): Promise<boolean>;

    getRolePermission(roleId: uuid, permissionId: uuid): Promise<RolePermissionDto|null>;

    enable(id: uuid, enable: boolean): Promise<RolePermissionDto>;

    delete(id: uuid): Promise<boolean>;

    getForTenant(tenantId: uuid): Promise<RolePermissionDto[]>;

    getSystemRolePermissions(): Promise<RolePermissionDto[]>;

}
