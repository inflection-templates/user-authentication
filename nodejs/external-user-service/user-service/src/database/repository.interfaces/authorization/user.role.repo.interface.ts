import { UserRolesDto } from "../../../domain.types/authorization/user.role.types";
import { uuid } from "../../../domain.types/miscellaneous/system.types";

////////////////////////////////////////////////////////////////////////////////////

export interface IUserRoleRepo {

    getUserRoles(userId: uuid): Promise<UserRolesDto|null>;

    hasUserRole(userId: uuid, roleId: uuid): Promise<boolean>;

    addUserRole(userId: uuid, roleId: uuid, tenantId: uuid): Promise<boolean>;

    removeUserRole(userId: uuid, roleId: uuid): Promise<boolean>;

    removeAllUserRoles(userId: uuid): Promise<boolean>;

}
