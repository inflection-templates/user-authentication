import { inject, injectable } from "tsyringe";
import { IUserRoleRepo } from "../../database/repository.interfaces/authorization/user.role.repo.interface";
import { uuid } from "../../domain.types/miscellaneous/system.types";
import { IRoleRepo } from "../../database/repository.interfaces/authorization/role.repo.interface";
import { logger } from "../../logger/logger";

////////////////////////////////////////////////////////////////////////////////////////////////////////

@injectable()
export class UserRoleService {

    constructor(
        @inject('IUserRoleRepo') private _userRoleRepo: IUserRoleRepo,
        @inject('IRoleRepo') private _roleRepo: IRoleRepo,
    ) {}

    getUserRoles = async (userId: uuid): Promise<any> => {
        return await this._userRoleRepo.getUserRoles(userId);
    };

    addUserRole = async (userId: uuid, roleId: uuid, tenantId: uuid): Promise<any> => {
        return await this._userRoleRepo.addUserRole(userId, roleId, tenantId);
    };

    removeUserRole = async (userId: uuid, roleId: uuid): Promise<boolean> => {
        return await this._userRoleRepo.removeUserRole(userId, roleId);
    };

    hasUserRole = async (userId: uuid, roleId: uuid): Promise<boolean> => {
        return await this._userRoleRepo.hasUserRole(userId, roleId);
    };

    addUserRoles = async (userId: uuid, roleIds: uuid[], tenantId: uuid): Promise<boolean> => {
        for await (const roleId of roleIds) {
            const roleExists = await this._roleRepo.getById(roleId);
            if (!roleExists) {
                logger.info(`Cannot add user role. Role with id ${roleId} not found.`);
                continue;
            }
            await this._userRoleRepo.addUserRole(userId, roleId, tenantId);
        }
        return true;
    };

    removeUserRoles = async (userId: uuid, roleIds: uuid[]): Promise<boolean> => {
        for await (const roleId of roleIds) {
            await this._userRoleRepo.removeUserRole(userId, roleId);
        }
        return true;
    };

    removeAllUserRoles = async (userId: uuid): Promise<boolean> => {
        return await this._userRoleRepo.removeAllUserRoles(userId);
    };

}
