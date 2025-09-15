import { inject, injectable } from "tsyringe";
import { IUserPermissionRepo } from "../../database/repository.interfaces/authorization/user.permission.repo.interface";
import { uuid } from "../../domain.types/miscellaneous/system.types";
import { UserPermissionDto } from '../../domain.types/authorization/user.permission.types';
import { IUserRepo } from "../../database/repository.interfaces/users/user.repo.interface";
import { IUserRoleRepo } from "../../database/repository.interfaces/authorization/user.role.repo.interface";
import { IRolePermissionRepo } from "../../database/repository.interfaces/authorization/role.permission.repo.interface";
import { PermissionDto } from "../../domain.types/authorization/permission.types";

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserPermissionService {

    constructor(
        @inject('IUserPermissionRepo') private _userPermissionRepo: IUserPermissionRepo,
        @inject('IUserRepo') private _userRepo: IUserRepo,
        @inject('IUserRoleRepo') private _userRoleRepo: IUserRoleRepo,
        @inject('IRolePermissionRepo') private _rolePermissionRepo: IRolePermissionRepo,
    ) { }

    addAddonPermission = async (userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto> => {
        return await this._userPermissionRepo.addPermission(userId, permissionId, scope);
    };

    removeAddonPermission = async (userId: uuid, permissionId: uuid): Promise<boolean> => {
        return await this._userPermissionRepo.removePermission(userId, permissionId);
    };

    getAddonPermissions = async (userId: uuid): Promise<UserPermissionDto[]> => {
        return await this._userPermissionRepo.getPermissions(userId);
    };

    grant = async (userId: uuid, permissionId: uuid): Promise<UserPermissionDto> => {
        return await this._userPermissionRepo.grantPermission(userId, permissionId);
    };

    revoke = async (userId: uuid, permissionId: uuid): Promise<UserPermissionDto> => {
        return await this._userPermissionRepo.revokePermission(userId, permissionId);
    };

    updatePermissionScope = async (userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto> => {
        return await this._userPermissionRepo.updatePermissionScope(userId, permissionId, scope);
    };

    getGranted = async (userId: uuid): Promise<UserPermissionDto[]> => {
        const permissions = await this.getAddonPermissions(userId);
        return permissions.filter(permission => permission.Granted === true);
    };

    getRevoked = async (userId: uuid): Promise<UserPermissionDto[]> => {
        const permissions = await this.getAddonPermissions(userId);
        return permissions.filter(permission => permission.Granted === false);
    };

    getRoleBased = async (userId: uuid): Promise<PermissionDto[]> => {
        const userRoles = await this._userRoleRepo.getUserRoles(userId);
        if (!userRoles || !userRoles.Roles) {
            return [];
        }
        const roleBasedPermissions: PermissionDto[] = [];
        for (const role of userRoles.Roles) {
            const rolePermissions = await this._rolePermissionRepo.getPermissionsForRole(role.id);
            const enabledPermissions = rolePermissions.filter(rp => rp.Enabled);
            const permissions = enabledPermissions.map(rp => rp.Permission);
            const uniquePermissions = permissions.filter((permission, index, self) =>
                index === self.findIndex((t) => t.id === permission.id)
            );
            roleBasedPermissions.push(...uniquePermissions);
        }
        return roleBasedPermissions;
    };

    getAll = async (userId: uuid): Promise<any> => {
        const addons = await this._userPermissionRepo.getPermissions(userId);
        const revoked = addons.filter(permission => permission.Granted === false);
        const granted = addons.filter(permission => permission.Granted === true);
        const roleBased = await this.getRoleBased(userId);

        //Filter out revoked permissions from role based permissions
        const refinedRoleBased = roleBased.filter(permission => !revoked.some(rp => rp.PermissionId === permission.id));
        const grantedPermission = granted.map(permission => permission.Permission);
        const combined = [...refinedRoleBased, ...grantedPermission];
        const uniqueCombined = combined.filter((permission, index, self) =>
            index === self.findIndex((t) => t.id === permission.id)
        );

        return {
            AddonGranted     : granted,
            AddonRevoked     : revoked,
            RoleBased        : roleBased,
            Consolidated     : uniqueCombined,
        };
    };

}
