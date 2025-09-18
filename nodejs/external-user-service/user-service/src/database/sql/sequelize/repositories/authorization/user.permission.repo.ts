import { IUserPermissionRepo } from '../../../../repository.interfaces/authorization/user.permission.repo.interface';
import UserPermission from '../../models/authorization/user.permission.model';
import { logger } from '../../../../../logger/logger';
import { ApiError } from '../../../../../common/api.error';
import { uuid } from '../../../../../domain.types/miscellaneous/system.types';
import {
    UserPermissionDto,
} from '../../../../../domain.types/authorization/user.permission.types';

///////////////////////////////////////////////////////////////////////

export class UserPermissionRepo implements IUserPermissionRepo {

    addPermission = async (userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto> => {
        try {
            const entity = {
                UserId       : userId,
                PermissionId : permissionId,
                Scope        : scope,
                Granted      : true,
            };
            const rp = await UserPermission.create(entity);
            const dto: UserPermissionDto = {
                id           : rp.id,
                UserId       : rp.UserId,
                PermissionId : rp.PermissionId,
                Scope        : rp.Scope,
                Granted      : rp.Granted,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    getPermissions = async (userId: uuid): Promise<UserPermissionDto[]> => {
        try {
            const userPermissions = await UserPermission.findAll({
                where : {
                    UserId : userId,
                },
            });
            return userPermissions;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    removePermission = async (userId: uuid, permissionId: uuid): Promise<boolean> => {
        try {
            const rp = await UserPermission.findOne({
                where : {
                    UserId       : userId,
                    PermissionId : permissionId,
                },
            });
            if (!rp) {
                return false;
            }
            await rp.destroy();
            return true;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    grantPermission = async (userId: uuid, permissionId: uuid): Promise<UserPermissionDto> => {
        try {
            let permission = await UserPermission.findOne({
                where : {
                    UserId       : userId,
                    PermissionId : permissionId,
                },
            });

            if (!permission) {
                throw new ApiError(404, 'Permission not found!');
            }
            permission.Granted = true;
            await permission.save();
            const dto: UserPermissionDto = {
                id           : permission.id,
                UserId       : permission.UserId,
                PermissionId : permission.PermissionId,
                Scope        : permission.Scope,
                Granted      : true,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    revokePermission = async (userId: uuid, permissionId: uuid): Promise<UserPermissionDto> => {
        try {
            const permission = await UserPermission.findOne({
                where : {
                    UserId       : userId,
                    PermissionId : permissionId,
                },
            });
            if (!permission) {
                throw new ApiError(404, 'Permission not found!');
            }
            permission.Granted = false;
            await permission.save();
            const dto: UserPermissionDto = {
                id           : permission.id,
                UserId       : permission.UserId,
                PermissionId : permission.PermissionId,
                Scope        : permission.Scope,
                Granted      : permission.Granted,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

    updatePermissionScope = async (userId: uuid, permissionId: uuid, scope: string): Promise<UserPermissionDto> => {
        try {
            const permission = await UserPermission.findOne({
                where : {
                    UserId       : userId,
                    PermissionId : permissionId,
                },
            });
            if (!permission) {
                return null;
            }
            permission.Scope = scope;
            await permission.save();
            const dto: UserPermissionDto = {
                id           : permission.id,
                UserId       : permission.UserId,
                PermissionId : permission.PermissionId,
                Scope        : permission.Scope,
                Granted      : permission.Granted,
            };
            return dto;
        } catch (error) {
            logger.info(error.message);
            throw new ApiError(500, error.message);
        }
    };

}
