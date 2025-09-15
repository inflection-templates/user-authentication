import { uuid } from "../../../domain.types/miscellaneous/system.types";
import { UserPermissionDto } from "../../../domain.types/authorization/user.permission.types";

/////////////////////////////////////////////////////////////////////////////////////////////

export interface IUserPermissionRepo {

    addPermission(userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto>;

    getPermissions(userId: uuid): Promise<UserPermissionDto[]>;

    removePermission(userId: uuid, permissionId: uuid): Promise<boolean>;

    grantPermission(userId: uuid, permissionId: uuid): Promise<UserPermissionDto>;

    revokePermission(userId: uuid, permissionId: uuid): Promise<UserPermissionDto>;

    updatePermissionScope(userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto>;

}
